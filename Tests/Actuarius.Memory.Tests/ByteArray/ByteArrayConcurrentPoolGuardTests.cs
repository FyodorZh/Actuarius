namespace Actuarius.Memory.Tests
{
    internal sealed class FakeByteArrayPool : IConcurrentPool<IMultiRefByteArray, int>
    {
        public List<IMultiRefByteArray> Acquired { get; } = new();
        public List<IMultiRefByteArray?> Released { get; } = new();

        public IMultiRefByteArray Acquire(int size)
        {
            var resource = new MultiRefByteArray(new byte[size]);
            Acquired.Add(resource);
            return resource;
        }

        public (IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink) AcquireEx(int size)
        {
            var resource = Acquire(size);
            return (resource, new Sink(this));
        }

        public void Release(IMultiRefByteArray? resource) => Released.Add(resource);

        private sealed class Sink : IPoolSink<IMultiRefByteArray>
        {
            private readonly FakeByteArrayPool _owner;

            public Sink(FakeByteArrayPool owner) => _owner = owner;

            public void Release(IMultiRefByteArray? resource) => _owner.Released.Add(resource);
        }
    }

    [TestFixture]
    public class ByteArrayConcurrentPoolGuardTests
    {
        private FakeByteArrayPool _inner = null!;
        private ByteArrayConcurrentPoolGuard _pool = null!;
        private List<(string Owner, ResourceUsageErrorType Error)> _errors = null!;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeByteArrayPool();
            _pool = new ByteArrayConcurrentPoolGuard(_inner);
            _errors = new List<(string, ResourceUsageErrorType)>();
            _pool.Error += (owner, error) => _errors.Add((owner, error));
        }

        [Test]
        public void Ctor_NullPool_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ByteArrayConcurrentPoolGuard(null!));
        }

        [Test]
        public void Acquire_ReturnsWorkingView_OfTheInnerResource()
        {
            var guard = _pool.Acquire(4);
            guard[0] = 42;

            Assert.That(guard.Count, Is.EqualTo(4));
            Assert.That(guard[0], Is.EqualTo(42));
            Assert.That(guard.Array, Is.SameAs(_inner.Acquired[0].Array));
            guard.Release();
        }

        [Test]
        public void Acquire_ReturnsDistinctGuards_WithDistinctCores()
        {
            var a = _pool.Acquire(2);
            var b = _pool.Acquire(2);

            Assert.That(a, Is.Not.SameAs(b));
            Assert.That(a.Array, Is.Not.SameAs(b.Array));
            a.Release();
            b.Release();
        }

        [Test]
        public void Release_RetiresTheCore_NotThePool()
        {
            var guard = _pool.Acquire(3);
            var core = _inner.Acquired[0];
            Assert.That(core.IsAlive, Is.True);

            guard.Release();

            Assert.That(core.IsAlive, Is.False);
            Assert.That(guard.IsValid, Is.False);
            Assert.That(_inner.Released, Is.Empty);
        }

        [Test]
        public void AddRefThenRelease_KeepsGuardAlive_UntilLastRelease()
        {
            var guard = _pool.Acquire(2);
            var core = _inner.Acquired[0];

            guard.AddRef();
            guard.Release();
            Assert.That(guard.IsValid, Is.True);
            Assert.That(core.IsAlive, Is.True);

            guard.Release();
            Assert.That(core.IsAlive, Is.False);
        }

        [Test]
        public void UseAfterRelease_Throws()
        {
            var guard = _pool.Acquire(3);
            guard.Release();

            Assert.Throws<ObjectDisposedException>(() => _ = guard.Count);
            Assert.Throws<ObjectDisposedException>(() => _ = guard[0]);
            Assert.Throws<ObjectDisposedException>(() => guard[0] = 1);
            Assert.Throws<ObjectDisposedException>(() => _ = guard.Array);
            Assert.Throws<ObjectDisposedException>(() => _ = guard.ReadOnlyArray);
            Assert.Throws<ObjectDisposedException>(() => guard.CopyTo(new byte[3], 0, 0, 3));
        }

        [Test]
        public void DoubleRelease_ReportsReleaseOfReleasedObject()
        {
            var guard = _pool.Acquire(1);
            guard.Release();
            guard.Release();

            Assert.That(_pool.ErrorCount, Is.EqualTo(1));
            Assert.That(_errors.Single().Error, Is.EqualTo(ResourceUsageErrorType.ReleaseOfReleasedObject));
        }

        [Test]
        public void AddRefAfterRelease_ReportsAddRefOfReleasedObject()
        {
            var guard = _pool.Acquire(1);
            guard.Release();
            guard.AddRef();

            Assert.That(_pool.ErrorCount, Is.EqualTo(1));
            Assert.That(_errors.Single().Error, Is.EqualTo(ResourceUsageErrorType.AddRefOfReleasedObject));
        }

        [Test]
        public void Errors_AggregateAcrossGuards()
        {
            var a = _pool.Acquire(1);
            var b = _pool.Acquire(1);
            a.Release();
            a.Release();
            b.Release();
            b.Release();

            Assert.That(_pool.ErrorCount, Is.EqualTo(2));
            Assert.That(_errors, Has.Count.EqualTo(2));
        }

        [Test]
        public void ThrowingErrorHandler_DoesNotEscape()
        {
            _pool.Error += (_, _) => throw new InvalidOperationException("boom");
            var guard = _pool.Acquire(1);
            guard.Release();

            Assert.DoesNotThrow(() => guard.Release());
            Assert.That(_pool.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void AssertNoOutstanding_FlagsUnreleasedGuards()
        {
            var guard = _pool.Acquire(1);

            _pool.AssertNoOutstanding();

            Assert.That(_errors.Single().Error, Is.EqualTo(ResourceUsageErrorType.NoUsageAssertionFail));
            guard.Release();
        }

        [Test]
        public void AssertNoOutstanding_IgnoresReleasedGuards()
        {
            var released = _pool.Acquire(1);
            released.Release();
            var leaked = _pool.Acquire(1);

            _pool.AssertNoOutstanding();

            Assert.That(_errors, Has.Count.EqualTo(1));
            leaked.Release();
        }

        [Test]
        public void AssertNoOutstanding_CleanRun_ReportsNothing()
        {
            _pool.Acquire(1).Release();

            _pool.AssertNoOutstanding();

            Assert.That(_pool.ErrorCount, Is.EqualTo(0));
            Assert.That(_errors, Is.Empty);
        }

        [Test]
        public void Release_Null_IsNoOp()
        {
            Assert.DoesNotThrow(() => _pool.Release(null));
            Assert.That(_pool.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void Release_ForeignType_Throws()
        {
            var foreign = new MultiRefByteArray(new byte[1]);

            Assert.Throws<InvalidOperationException>(() => _pool.Release(foreign));
            foreign.Release();
        }

        [Test]
        public void Release_GuardThroughPool_DoesNotReleaseIt()
        {
            var guard = _pool.Acquire(1);

            _pool.Release(guard);
            Assert.That(guard.IsValid, Is.True);

            guard.Release();
            Assert.That(guard.IsValid, Is.False);
        }

        [Test]
        public void AcquireEx_GuardsTheResource_AndTheSinkIsNoOp()
        {
            var (guard, sink) = _pool.AcquireEx(2);
            var core = _inner.Acquired[0];

            sink.Release(guard);
            Assert.That(guard.IsValid, Is.True);

            guard.Release();
            Assert.That(core.IsAlive, Is.False);
        }

        [Test]
        public void ConcurrentAcquireAndRelease_ProducesNoErrors()
        {
            Parallel.For(0, 10_000, _ =>
            {
                var guard = _pool.Acquire(8);
                guard.AddRef();
                guard.Release();
                guard.Release();
            });

            _pool.AssertNoOutstanding();
            Assert.That(_pool.ErrorCount, Is.EqualTo(0));
        }
    }
}
