using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawByteArrayConcurrentPool : ConcurrentPool<byte[], int>
    {
        private readonly int _bucketCapacity;

        public RawByteArrayConcurrentPool(int bucketCapacity)
            : base(new SynchronizedConcurrentDictionary<int, IPool<byte[]>>())
        {
            _bucketCapacity = bucketCapacity;
        }

        protected override IPool<byte[]> CreatePool(int classId)
        {
            return new FixedLengthRawByteArrayConcurrentPool(classId, _bucketCapacity);
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