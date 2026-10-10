using Actuarius.Memory.Internal;

namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class RefCounterTests
    {
        [Test]
        public void NonRevivable_StartsAtOne()
        {
            var counter = new RefCounter(revivable: false);

            Assert.That(counter.RefCount, Is.EqualTo(1));
            Assert.That(counter.IsValid, Is.True);
        }

        [Test]
        public void Revivable_StartsAtZero()
        {
            var counter = new RefCounter(revivable: true);

            Assert.That(counter.RefCount, Is.EqualTo(0));
            Assert.That(counter.IsValid, Is.False);
        }

        [Test]
        public void AddRef_IncrementsAndRaisesUpdated()
        {
            var counter = new RefCounter(revivable: false);
            var updated = new List<int>();
            counter.Updated += updated.Add;

            int result = counter.AddRef();

            Assert.That(result, Is.EqualTo(2));
            Assert.That(counter.RefCount, Is.EqualTo(2));
            Assert.That(counter.IsValid, Is.True);
            Assert.That(updated, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void AddRef_OnRevivableIdleCounter_FailsAndLeavesCountUnchanged()
        {
            var counter = new RefCounter(revivable: true);
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            int result = counter.AddRef();

            Assert.That(result, Is.EqualTo(0));
            Assert.That(counter.RefCount, Is.EqualTo(0));
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToIncrement, 0) }));
        }

        [Test]
        public void AddRef_OnReleasedCounter_FailsAndLeavesCountUnchanged()
        {
            var counter = new RefCounter(revivable: false);
            counter.Release();
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            int result = counter.AddRef();

            Assert.That(result, Is.EqualTo(0));
            Assert.That(counter.RefCount, Is.EqualTo(0));
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToIncrement, 0) }));
        }

        [Test]
        public void AddRef_AfterUnderflow_ReportsNegativeCount()
        {
            var counter = new RefCounter(revivable: true);
            counter.Release();
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            int result = counter.AddRef();

            Assert.That(result, Is.EqualTo(0));
            Assert.That(counter.RefCount, Is.EqualTo(-1));
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToIncrement, -1) }));
        }

        [Test]
        public void Release_ReturnsNewCountAndRaisesUpdated()
        {
            var counter = new RefCounter(revivable: false);
            counter.AddRef();
            var updated = new List<int>();
            counter.Updated += updated.Add;

            int result = counter.Release();

            Assert.That(result, Is.EqualTo(1));
            Assert.That(counter.RefCount, Is.EqualTo(1));
            Assert.That(updated, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Release_LastReference_ReturnsZeroAndBecomesInvalid()
        {
            var counter = new RefCounter(revivable: false);
            var updated = new List<int>();
            counter.Updated += updated.Add;

            int result = counter.Release();

            Assert.That(result, Is.EqualTo(0));
            Assert.That(counter.RefCount, Is.EqualTo(0));
            Assert.That(counter.IsValid, Is.False);
            Assert.That(updated, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Release_OnZero_UnderflowsAndRaisesFailedToDecrement()
        {
            var counter = new RefCounter(revivable: true);
            var updated = new List<int>();
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Updated += updated.Add;
            counter.Error += (error, count) => errors.Add((error, count));

            int result = counter.Release();

            Assert.That(result, Is.EqualTo(-1));
            Assert.That(counter.RefCount, Is.EqualTo(-1));
            Assert.That(updated, Is.Empty);
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToDecrement, -1) }));
        }

        [Test]
        public void Release_DoesNotClampBelowZero()
        {
            var counter = new RefCounter(revivable: true);
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            counter.Release();
            counter.Release();
            counter.Release();

            Assert.That(counter.RefCount, Is.EqualTo(-3));
            Assert.That(errors, Is.EqualTo(new[]
            {
                (RefCounterErrorEvent.FailedToDecrement, -1),
                (RefCounterErrorEvent.FailedToDecrement, -2),
                (RefCounterErrorEvent.FailedToDecrement, -3),
            }));
        }

        [Test]
        public void Revive_FromZero_SetsToOneAndRaisesUpdated()
        {
            var counter = new RefCounter(revivable: true);
            var updated = new List<int>();
            counter.Updated += updated.Add;

            bool result = counter.Revive();

            Assert.That(result, Is.True);
            Assert.That(counter.RefCount, Is.EqualTo(1));
            Assert.That(counter.IsValid, Is.True);
            Assert.That(updated, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Revive_WhenCountNotZero_FailsAndLeavesCountUnchanged()
        {
            var counter = new RefCounter(revivable: true);
            counter.Revive();
            counter.AddRef();
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            bool result = counter.Revive();

            Assert.That(result, Is.False);
            Assert.That(counter.RefCount, Is.EqualTo(2));
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToRevive, 2) }));
        }

        [Test]
        public void Revive_OnNonRevivableCounter_AlwaysFails()
        {
            var counter = new RefCounter(revivable: false);
            var errors = new List<(RefCounterErrorEvent Error, int Count)>();
            counter.Error += (error, count) => errors.Add((error, count));

            bool result = counter.Revive();

            Assert.That(result, Is.False);
            Assert.That(counter.RefCount, Is.EqualTo(1));
            Assert.That(errors, Is.EqualTo(new[] { (RefCounterErrorEvent.FailedToRevive, 1) }));
        }

        [Test]
        public void ToString_FormatsCurrentCount()
        {
            var counter = new RefCounter(revivable: false);

            Assert.That(counter.ToString(), Is.EqualTo("RefCount:1"));

            counter.AddRef();

            Assert.That(counter.ToString(), Is.EqualTo("RefCount:2"));
        }

        [Test]
        public void BalancedAddRefAndRelease_AreAtomicUnderConcurrency()
        {
            var counter = new RefCounter(revivable: false);
            const int threads = 8;
            const int iterations = 10_000;

            using var start = new ManualResetEventSlim(false);
            var tasks = new Task[threads];
            for (int t = 0; t < threads; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    start.Wait();
                    for (int i = 0; i < iterations; i++)
                    {
                        counter.AddRef();
                    }

                    for (int i = 0; i < iterations; i++)
                    {
                        counter.Release();
                    }
                });
            }

            start.Set();
            Task.WaitAll(tasks);

            Assert.That(counter.RefCount, Is.EqualTo(1));
            Assert.That(counter.IsValid, Is.True);
        }
    }
}
