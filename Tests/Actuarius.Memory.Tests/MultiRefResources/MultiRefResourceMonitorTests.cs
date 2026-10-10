using Actuarius.Memory.Internal;

namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class MultiRefResourceMonitorTests
    {
        private FakeRefCounter _counter = null!;
        private List<(string Name, ResourceUsageErrorType Error)> _errors = null!;

        [SetUp]
        public void SetUp()
        {
            _counter = new FakeRefCounter(1);
            _errors = new List<(string, ResourceUsageErrorType)>();
        }

        private MultiRefResourceMonitor CreateMonitor(
            bool collectHistory = true,
            bool traceEnabled = true)
        {
            var monitor = new MultiRefResourceMonitor(
                _counter, "res", collectHistory, trackLeaks: false, traceEnabled);
            monitor.Error += (name, error) => _errors.Add((name, error));
            return monitor;
        }

        [Test]
        public void Counter_ReturnsObservedCounter()
        {
            var monitor = CreateMonitor();

            Assert.That(monitor.Counter, Is.SameAs(_counter));
        }

        [Test]
        public void Constructor_SubscribesToUpdated_AndRecordsHistory()
        {
            var monitor = CreateMonitor();

            _counter.EmitUpdated(2);

            var history = monitor.ExportHistory();
            Assert.That(history, Has.Count.EqualTo(1));
            Assert.That(history[0].Action, Is.EqualTo("AddRef"));
            Assert.That(history[0].Count, Is.EqualTo(2));
            Assert.That(history[0].IsError, Is.False);
        }

        [Test]
        public void TransitionNames_AreInferredFromCountDeltas()
        {
            var monitor = CreateMonitor();

            _counter.EmitUpdated(2);
            _counter.EmitUpdated(3);
            _counter.EmitUpdated(2);

            Assert.That(
                monitor.ExportHistory().Select(h => h.Action),
                Is.EqualTo(new[] { "AddRef", "AddRef", "Release" }));
        }

        [Test]
        public void TransitionName_FromZero_IsRevive()
        {
            _counter = new FakeRefCounter(0);
            var monitor = CreateMonitor();

            _counter.EmitUpdated(1);

            Assert.That(monitor.ExportHistory()[0].Action, Is.EqualTo("Revive"));
        }

        [TestCase(RefCounterErrorEvent.FailedToIncrement, ResourceUsageErrorType.AddRefOfReleasedObject)]
        [TestCase(RefCounterErrorEvent.FailedToDecrement, ResourceUsageErrorType.ReleaseOfReleasedObject)]
        [TestCase(RefCounterErrorEvent.FailedToRevive, ResourceUsageErrorType.WrongReviveUsage)]
        public void Error_MapsCounterFailure(RefCounterErrorEvent counterError, ResourceUsageErrorType expected)
        {
            var monitor = CreateMonitor();

            _counter.EmitError(counterError, 0);

            Assert.That(_errors, Has.Count.EqualTo(1));
            Assert.That(_errors[0].Name, Is.EqualTo("res"));
            Assert.That(_errors[0].Error, Is.EqualTo(expected));
        }

        [Test]
        public void Error_IsAlsoRecordedInHistory()
        {
            var monitor = CreateMonitor();

            _counter.EmitError(RefCounterErrorEvent.FailedToDecrement, -1);

            var history = monitor.ExportHistory();
            Assert.That(history, Has.Count.EqualTo(1));
            Assert.That(history[0].IsError, Is.True);
            Assert.That(history[0].Action, Is.EqualTo("ReleaseOfReleasedObject"));
            Assert.That(history[0].Count, Is.EqualTo(-1));
        }

        [Test]
        public void CollectHistoryFalse_KeepsNoHistory_ButStillRaisesError()
        {
            var monitor = CreateMonitor(collectHistory: false);

            _counter.EmitUpdated(2);
            _counter.EmitError(RefCounterErrorEvent.FailedToIncrement, 0);

            Assert.That(monitor.ExportHistory(), Is.Empty);
            Assert.That(_errors, Has.Count.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TraceEnabled_ControlsStackCapture(bool traceEnabled)
        {
            var monitor = CreateMonitor(traceEnabled: traceEnabled);

            _counter.EmitUpdated(2);

            var record = monitor.ExportHistory()[0];
            Assert.That(record.Stack is not null, Is.EqualTo(traceEnabled));
        }

        [Test]
        public void History_IsCapped_AndDropsOldest()
        {
            var monitor = CreateMonitor();

            for (int i = 2; i <= 151; i++)
            {
                _counter.EmitUpdated(i);
            }

            var history = monitor.ExportHistory();
            Assert.That(history, Has.Count.EqualTo(100));
            Assert.That(history[0].Count, Is.EqualTo(52));
            Assert.That(history[history.Count - 1].Count, Is.EqualTo(151));
        }

        [Test]
        public void ExportHistory_ReturnsSnapshot()
        {
            var monitor = CreateMonitor();

            _counter.EmitUpdated(2);
            var snapshot = monitor.ExportHistory();
            _counter.EmitUpdated(3);

            Assert.That(snapshot, Has.Count.EqualTo(1));
            Assert.That(monitor.ExportHistory(), Has.Count.EqualTo(2));
        }

        [Test]
        public void Clear_EmptiesHistory()
        {
            var monitor = CreateMonitor();

            _counter.EmitUpdated(2);
            monitor.Clear();

            Assert.That(monitor.ExportHistory(), Is.Empty);
        }

        [Test]
        public void Detach_StopsRecordingAndRaising_AndIsIdempotent()
        {
            var monitor = CreateMonitor();
            monitor.Detach();

            _counter.EmitUpdated(2);
            _counter.EmitError(RefCounterErrorEvent.FailedToIncrement, 0);
            monitor.Detach();

            Assert.That(monitor.ExportHistory(), Is.Empty);
            Assert.That(_errors, Is.Empty);
        }

        [Test]
        public void AssertNoUsage_WhenAlive_RaisesNoUsageAssertionFail()
        {
            var monitor = CreateMonitor();
            _counter.RefCount = 3;

            monitor.AssertNoUsage();

            Assert.That(_errors, Has.Count.EqualTo(1));
            Assert.That(_errors[0].Error, Is.EqualTo(ResourceUsageErrorType.NoUsageAssertionFail));
            Assert.That(monitor.ExportHistory()[0].Action, Is.EqualTo("AssertNoUsage"));
        }

        [Test]
        public void AssertNoUsage_WhenReleased_DoesNothing()
        {
            var monitor = CreateMonitor();
            _counter.RefCount = 0;

            monitor.AssertNoUsage();

            Assert.That(_errors, Is.Empty);
            Assert.That(monitor.ExportHistory(), Is.Empty);
        }

        [Test]
        public void ErrorHandlerThatThrows_IsSwallowed()
        {
            var monitor = CreateMonitor();
            monitor.Error += (_, _) => throw new InvalidOperationException("handler");

            Assert.DoesNotThrow(() => _counter.EmitError(RefCounterErrorEvent.FailedToRevive, 1));
        }

        [Test]
        public void ConcurrentRecordingAndExport_DoesNotThrow()
        {
            var monitor = CreateMonitor();

            Parallel.For(0, 10000, i =>
            {
                _counter.EmitUpdated(i % 5 + 2);
                monitor.ExportHistory();
            });

            Assert.That(monitor.ExportHistory().Count, Is.LessThanOrEqualTo(100));
        }
    }
}
