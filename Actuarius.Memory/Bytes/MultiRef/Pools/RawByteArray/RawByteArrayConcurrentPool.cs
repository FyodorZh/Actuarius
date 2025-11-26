using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawByteArrayConcurrentPool : ConcurrentPool<byte[], int>
    {
        private readonly int _bucketCapacity;

        public RawByteArrayConcurrentPool(int bucketCapacity)
            : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<byte[]>>())
        {
            _bucketCapacity = bucketCapacity;
        }

        protected override IConcurrentPool<byte[]> CreatePool(int classId)
        {
            return new FixedLengthRawByteArrayConcurrentPool(classId, _bucketCapacity);
        }

        protected sealed override int Classify(int param)
        {
            return BitMath.NextPow2((uint)param);
        }

        protected sealed override int Classify(byte[] array)
        {
            return array.Length;
        }
    }
}