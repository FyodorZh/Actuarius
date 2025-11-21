namespace Actuarius.Memory
{
    public class FixedLengthRawByteArrayPool : Pool<byte[]>
    {
        private readonly int _length;
        
        public FixedLengthRawByteArrayPool(int length)
        {
            _length = length;
        }

        protected override byte[] Constructor()
        {
            return new byte[_length];
        }
    }
}