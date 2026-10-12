using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class ByteArrayPool : IPool<IMultiRefByteArray, int>, IPoolSink<RecyclableMultiRefByteArrayFromPool>
    {
        private readonly IPool<byte[], int> _bytesPool;
        private readonly IUnorderedCollection<RecyclableMultiRefByteArrayFromPool>? _wrapperStore;

        public ByteArrayPool(IPool<byte[], int> bytesPool)
            : this(bytesPool, wrapperStore: null)
        {
        }

        /// <summary>
        /// Creates a pool that recycles <see cref="MultiRefByteArrayFromPool"/> wrappers instead of dropping them,
        /// removing the per-acquire wrapper allocation while the underlying byte[] buckets are warm.
        /// </summary>
        /// <param name="bytesPool">The underlying byte[] pool.</param>
        /// <param name="wrapperCacheCapacity">
        /// Maximum number of released wrappers kept for reuse. Values less than or equal to zero disable recycling.
        /// </param>
        /// <remarks>
        /// Recycled wrappers are revived and rebound, so their object identity is reused across generations.
        /// A stale reference that is released twice (or released after the resource was re-acquired by someone else)
        /// can corrupt the current owner's reference count and return an in-use byte[] to its bucket. Only enable
        /// recycling when acquisitions and releases are balanced, and keep guard/monitor diagnostics enabled in tests.
        /// </remarks>
        public ByteArrayPool(IPool<byte[], int> bytesPool, int wrapperCacheCapacity)
            : this(bytesPool, CreateWrapperStore(wrapperCacheCapacity))
        {
        }

        protected ByteArrayPool(
            IPool<byte[], int> bytesPool,
            IUnorderedCollection<RecyclableMultiRefByteArrayFromPool>? wrapperStore)
        {
            _bytesPool = bytesPool;
            _wrapperStore = wrapperStore;
        }

        private static IUnorderedCollection<RecyclableMultiRefByteArrayFromPool>? CreateWrapperStore(int capacity)
        {
            if (capacity <= 0)
            {
                return null;
            }

            return new CycleQueue<RecyclableMultiRefByteArrayFromPool>(capacity, canGrow: false);
        }

        void IPoolSink<RecyclableMultiRefByteArrayFromPool>.Release(RecyclableMultiRefByteArrayFromPool? resource)
        {
            if (resource != null)
            {
                _wrapperStore?.Put(resource);
            }
        }

        public void Release(IMultiRefByteArray? resource)
        {
            // DO NOTHING. Resources are released via IMultiRefByteArray.Release()
        }

        public IMultiRefByteArray Acquire(int param0)
        {
            if (_wrapperStore == null)
            {
                return new MultiRefByteArrayFromPool(_bytesPool, _bytesPool.Acquire(param0), 0, param0);
            }

            return AcquireRecycled(param0);
        }

        public (IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink) AcquireEx(int param0)
        {
            if (_wrapperStore == null)
            {
                (byte[] resource, IPoolSink<byte[]> poolSink) = _bytesPool.AcquireEx(param0);
                return (new MultiRefByteArrayFromPool(poolSink, resource, 0, param0), this);
            }

            return (AcquireRecycled(param0), this);
        }

        private RecyclableMultiRefByteArrayFromPool AcquireRecycled(int param0)
        {
            var store = _wrapperStore!;
            if (store.TryPop(out var wrapper))
            {
                (byte[] array, IPoolSink<byte[]> poolSink) = _bytesPool.AcquireEx(param0);
                if (wrapper.TryRebind(poolSink, array, 0, param0))
                {
                    return wrapper;
                }

                poolSink.Release(array);
            }

            (byte[] newArray, IPoolSink<byte[]> newPoolSink) = _bytesPool.AcquireEx(param0);
            return new RecyclableMultiRefByteArrayFromPool(newPoolSink, newArray, 0, param0, this);
        }
    }
}
