using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawByteArrayConcurrentPool : ConcurrentPool<byte[], int>
    {
        public delegate int ArrayPoolCapacityDelegate(int arraySize);
        
        private readonly ArrayPoolCapacityDelegate _capacityDelegate;


        public RawByteArrayConcurrentPool(int bucketCapacity)
            : this(_ => bucketCapacity)
        {
        }
        
        public RawByteArrayConcurrentPool(ArrayPoolCapacityDelegate capacityDelegate)
            : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<byte[]>>())
        {
            _capacityDelegate = capacityDelegate;
        }

        protected override IConcurrentPool<byte[]> CreatePool(int classId)
        {
            int capacity = _capacityDelegate(classId);
            if (capacity > 0)
            {
                return new FixedLengthRawByteArrayConcurrentPool(classId, capacity);
            }

            return new DelegateNoPool<byte[]>(() => new byte[classId]);
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