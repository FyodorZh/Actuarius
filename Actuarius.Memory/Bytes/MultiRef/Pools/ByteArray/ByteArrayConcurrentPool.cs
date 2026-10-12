using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class ByteArrayConcurrentPool : ByteArrayPool, IConcurrentPool<IMultiRefByteArray, int>
    {
        public ByteArrayConcurrentPool(IConcurrentPool<byte[], int> bytesPool) : base(bytesPool)
        {
        }

        /// <summary>
        /// Creates a concurrent pool that recycles <see cref="MultiRefByteArrayFromPool"/> wrappers instead of
        /// dropping them. See <see cref="ByteArrayPool"/> for the recycling caveats.
        /// </summary>
        /// <param name="bytesPool">The underlying byte[] pool.</param>
        /// <param name="wrapperCacheCapacity">
        /// Maximum number of released wrappers kept for reuse. Values less than or equal to zero disable recycling.
        /// </param>
        public ByteArrayConcurrentPool(IConcurrentPool<byte[], int> bytesPool, int wrapperCacheCapacity)
            : base(bytesPool, CreateConcurrentWrapperStore(wrapperCacheCapacity))
        {
        }

        private static IUnorderedCollection<RecyclableMultiRefByteArrayFromPool>? CreateConcurrentWrapperStore(int capacity)
        {
            if (capacity <= 0)
            {
                return null;
            }

            return new LimitedConcurrentQueue<RecyclableMultiRefByteArrayFromPool>(capacity);
        }
    }
}
