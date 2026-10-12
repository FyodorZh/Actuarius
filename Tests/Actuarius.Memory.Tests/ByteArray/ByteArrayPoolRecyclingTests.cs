namespace Actuarius.Memory.Tests
{
    [TestFixture]
    public class ByteArrayPoolRecyclingTests
    {
        private const int Capacity = 8;

        private static ByteArrayPool CreateSyncPool(int wrapperCacheCapacity = Capacity)
        {
            return new ByteArrayPool(new RawArrayPool<byte>(null), wrapperCacheCapacity);
        }

        private static ByteArrayConcurrentPool CreateConcurrentPool(int wrapperCacheCapacity = Capacity)
        {
            return new ByteArrayConcurrentPool(new RawArrayConcurrentPool<byte>(_ => 16), wrapperCacheCapacity);
        }

        [Test]
        public void Acquire_Release_Acquire_ReusesWrapper_WhenRecyclingEnabled()
        {
            var pool = CreateSyncPool();

            var first = pool.Acquire(64);
            first[0] = 42;
            first.Release();

            var second = pool.Acquire(64);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Count, Is.EqualTo(64));
            Assert.That(second.Offset, Is.EqualTo(0));
            Assert.That(second.IsValid, Is.True);
            second[0] = 43;
            Assert.That(second[0], Is.EqualTo(43));
            second.Release();
        }

        [Test]
        public void Acquire_Release_Acquire_DoesNotReuseWrapper_WhenRecyclingDisabled()
        {
            var pool = new ByteArrayPool(new RawArrayPool<byte>(null));

            var first = pool.Acquire(64);
            first.Release();

            var second = pool.Acquire(64);

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.Count, Is.EqualTo(64));
            second.Release();
        }

        [Test]
        public void ReusedWrapper_IsReboundToNewSize()
        {
            var pool = CreateSyncPool();

            var first = pool.Acquire(64);
            Assert.That(first.Array.Length, Is.EqualTo(64));
            first.Release();

            var second = pool.Acquire(128);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Count, Is.EqualTo(128));
            Assert.That(second.Offset, Is.EqualTo(0));
            Assert.That(second.Array.Length, Is.EqualTo(128));
            second[127] = 1;
            Assert.That(second[127], Is.EqualTo(1));
            second.Release();
        }

        [Test]
        public void RecycledWrapper_SurvivesManyCycles()
        {
            var pool = CreateSyncPool();

            var expected = pool.Acquire(16);
            expected.Release();

            for (int i = 0; i < 1_000; i++)
            {
                var resource = pool.Acquire(16);
                Assert.That(resource, Is.SameAs(expected));
                Assert.That(resource.IsValid, Is.True);
                Assert.That(resource.Count, Is.EqualTo(16));
                resource.Release();
            }
        }

        [Test]
        public void WrapperStore_DropsWrappersBeyondCapacity()
        {
            var pool = CreateSyncPool(wrapperCacheCapacity: 1);

            var a = pool.Acquire(16);
            var b = pool.Acquire(16);
            a.Release();
            b.Release();

            var c = pool.Acquire(16);
            var d = pool.Acquire(16);

            Assert.That(c, Is.SameAs(a));
            Assert.That(d, Is.Not.SameAs(a));
            Assert.That(d, Is.Not.SameAs(b));
            c.Release();
            d.Release();
        }

        [Test]
        public void AcquireEx_AlsoRecyclesWrappers()
        {
            var pool = CreateSyncPool();

            var (first, _) = pool.AcquireEx(32);
            first.Release();

            var (second, _) = pool.AcquireEx(32);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Count, Is.EqualTo(32));
            second.Release();
        }

        [Test]
        public void MisusedWrapper_IsDroppedInFavorOfFreshOne()
        {
            var pool = CreateSyncPool();

            var misused = pool.Acquire(16);
            misused.Release();
            misused.Release();

            var fresh = pool.Acquire(16);

            Assert.That(fresh, Is.Not.SameAs(misused));
            Assert.That(fresh.IsValid, Is.True);
            Assert.That(fresh.Count, Is.EqualTo(16));
            fresh.Release();

            var reused = pool.Acquire(16);
            Assert.That(reused, Is.SameAs(fresh));
            reused.Release();
        }

        [Test]
        public void MonitoredWrapper_IsNotRecycled()
        {
            var pool = CreateSyncPool();

            var monitored = pool.Acquire(16);
            var monitor = ((MultiRefResource)monitored).ConstructMonitor(
                collectHistory: false, trackLeaks: false, traceEnabled: false);
            monitored.Release();

            var next = pool.Acquire(16);

            Assert.That(next, Is.Not.SameAs(monitored));
            monitor.Detach();
            next.Release();
        }

        [Test]
        public void ConcurrentPool_RecyclesWrappers()
        {
            var pool = CreateConcurrentPool();

            var first = pool.Acquire(64);
            first.Release();

            var second = pool.Acquire(64);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Count, Is.EqualTo(64));
            second.Release();
        }

        [Test]
        public void ConcurrentPool_ParallelAcquireRelease_ProducesUsableResources()
        {
            var pool = CreateConcurrentPool(wrapperCacheCapacity: 1_024);

            Parallel.For(0, 10_000, i =>
            {
                var resource = pool.Acquire(64);
                resource[0] = (byte)i;
                Assert.That(resource[0], Is.EqualTo((byte)i));
                resource.Release();
            });

            var final = pool.Acquire(64);
            Assert.That(final.IsValid, Is.True);
            final.Release();
        }
    }
}
