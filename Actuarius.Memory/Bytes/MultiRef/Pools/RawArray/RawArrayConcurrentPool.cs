using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawArrayConcurrentPool<T> : ConcurrentPool<T[], int>
    {
        public delegate int ArrayPoolCapacityDelegate(int arraySize);
        
        private readonly ArrayPoolCapacityDelegate _capacityDelegate;

        public RawArrayConcurrentPool(int bucketCapacity)
            : this(_ => bucketCapacity)
        {
        }
        
        public RawArrayConcurrentPool(ArrayPoolCapacityDelegate capacityDelegate)
            : base(new SynchronizedConcurrentDictionary<int, IConcurrentPool<T[]>>())
        {
            _capacityDelegate = capacityDelegate;
        }

        protected override IConcurrentPool<T[]> CreatePool(int classId)
        {
            int capacity = _capacityDelegate(classId);
            if (capacity > 0)
            {
                return new FixedLengthRawArrayConcurrentPool<T>(classId, capacity);
            }

            return new DelegateNoPool<T[]>(() => new T[classId]);
        }

        protected sealed override int Classify(int param)
        {
            return BitMath.NextPow2((uint)param);
        }

        protected sealed override int Classify(T[] array)
        {
            return array.Length;
        }
    }
}