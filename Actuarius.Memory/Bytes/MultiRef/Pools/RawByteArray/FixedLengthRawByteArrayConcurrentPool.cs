using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class FixedLengthRawByteArrayConcurrentPool: ConcurrentPool<byte[]>
    {
        private readonly int _length;
        
        public FixedLengthRawByteArrayConcurrentPool(int length, int capacity)
            :base(new LimitedConcurrentQueue<byte[]>(capacity))
        {
            _length = length;
        }

        protected override byte[] Constructor()
        {
            return new byte[_length];
        }
    }
}