using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawByteArrayPool : Pool<byte[], int>
    {
        public RawByteArrayPool()
            : base(new SystemDictionary<int, IPool<byte[]>>())
        {
        }
        
        protected override IPool<byte[]> CreatePool(int classId)
        {
            return new FixedLengthRawByteArrayPool(classId);
        }

        protected sealed override int Classify(int requiredArrayLength)
        {
            return BitMath.NextPow2((uint)requiredArrayLength);
        }

        protected sealed override int Classify(byte[] array)
        {
            return array.Length;
        }
    }
}