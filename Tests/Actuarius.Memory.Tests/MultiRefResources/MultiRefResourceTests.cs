namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class MultiRefResourceTests
    {
        [Test]
        public void NonRevivable_StartsAlive()
        {
            var resource = new TestResource();

            Assert.That(resource.IsAlive, Is.True);
            Assert.That(resource.RefCount.RefCount, Is.EqualTo(1));
        }

        [Test]
        public void Revivable_StartsNotAlive()
        {
            var resource = new TestResource(revivable: true);

            Assert.That(resource.IsAlive, Is.False);
            Assert.That(resource.RefCount.RefCount, Is.EqualTo(0));
        }

        [Test]
        public void AddRef_Increments()
        {
            var resource = new TestResource();

            resource.AddRef();

            Assert.That(resource.RefCount.RefCount, Is.EqualTo(2));
            Assert.That(resource.IsAlive, Is.True);
        }

        [Test]
        public void Release_ToZero_CallsOnReleasedOnce()
        {
            var resource = new TestResource();

            resource.Release();

            Assert.That(resource.ReleasedCount, Is.EqualTo(1));
            Assert.That(resource.IsAlive, Is.False);
        }

        [Test]
        public void Release_BelowZero_DoesNotCallOnReleasedAgain()
        {
            var resource = new TestResource();

            resource.Release();
            resource.Release();

            Assert.That(resource.ReleasedCount, Is.EqualTo(1));
            Assert.That(resource.RefCount.RefCount, Is.EqualTo(-1));
        }

        [Test]
        public void Acquire_ReturnsSameInstance_AndIncrements()
        {
            var resource = new TestResource();

            var acquired = resource.Acquire();

            Assert.That(acquired, Is.SameAs(resource));
            Assert.That(resource.RefCount.RefCount, Is.EqualTo(2));

            acquired.Release();
            resource.Release();

            Assert.That(resource.ReleasedCount, Is.EqualTo(1));
        }

        [Test]
        public void Revive_Revivable_Succeeds()
        {
            var resource = new TestResource(revivable: true);

            Assert.That(resource.ReviveEx(), Is.True);
            Assert.That(resource.IsAlive, Is.True);
        }

        [Test]
        public void Revive_NonRevivable_Fails()
        {
            var resource = new TestResource();

            Assert.That(resource.ReviveEx(), Is.False);
        }

        [Test]
        public void AddRefAfterRelease_RaisesAddRefOfReleasedObject()
        {
            var resource = new TestResource();
            var errors = Subscribe(resource);

            resource.Release();
            resource.AddRef();

            Assert.That(errors, Is.EqualTo(new[] { ResourceUsageErrorType.AddRefOfReleasedObject }));
        }

        [Test]
        public void DoubleRelease_RaisesReleaseOfReleasedObject()
        {
            var resource = new TestResource();
            var errors = Subscribe(resource);

            resource.Release();
            resource.Release();

            Assert.That(errors, Is.EqualTo(new[] { ResourceUsageErrorType.ReleaseOfReleasedObject }));
        }

        [Test]
        public void WrongRevive_RaisesWrongReviveUsage()
        {
            var resource = new TestResource();
            var errors = Subscribe(resource);

            resource.ReviveEx();

            Assert.That(errors, Is.EqualTo(new[] { ResourceUsageErrorType.WrongReviveUsage }));
        }

        [Test]
        public void ConstructMonitor_IsIdempotent()
        {
            var resource = new TestResource();

            var first = resource.ConstructMonitor(trackLeaks: false);
            var second = resource.ConstructMonitor(trackLeaks: false);

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ConstructMonitor_AfterDetach_CreatesNewMonitor()
        {
            var resource = new TestResource();

            var first = resource.ConstructMonitor(trackLeaks: false);
            first.Detach();
            var second = resource.ConstructMonitor(trackLeaks: false);

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Monitor_IsNullAfterDetach()
        {
            var resource = new TestResource();

            var monitor = resource.ConstructMonitor(trackLeaks: false);
            Assert.That(resource.MonitorEx, Is.SameAs(monitor));

            monitor.Detach();
            Assert.That(resource.MonitorEx, Is.Null);
        }

        [Test]
        public void ConstructMonitor_PropagatesOptions()
        {
            var resource = new TestResource();

            var monitor = resource.ConstructMonitor(collectHistory: false, trackLeaks: false);
            resource.AddRef();

            Assert.That(monitor.ExportHistory(), Is.Empty);
        }

        [Test]
        public void ConstructMonitor_Concurrent_ReturnsSameInstance()
        {
            var resource = new TestResource();
            var monitors = new MultiRefResourceMonitor[32];

            Parallel.For(0, monitors.Length, i => monitors[i] = resource.ConstructMonitor(trackLeaks: false));

            Assert.That(monitors.All(m => ReferenceEquals(m, monitors[0])), Is.True);
        }

        [Test]
        public void ToString_DescribesResource()
        {
            var resource = new TestResource();

            var text = resource.ToString();

            Assert.That(text, Does.StartWith("TestResource: "));
            Assert.That(text, Does.Contain("RefCount:1"));
        }

        private static List<ResourceUsageErrorType> Subscribe(TestResource resource)
        {
            var monitor = resource.ConstructMonitor(trackLeaks: false);
            var errors = new List<ResourceUsageErrorType>();
            monitor.Error += (_, error) => errors.Add(error);
            return errors;
        }
    }
}
