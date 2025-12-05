using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class RawArrayPool<T> : Pool<T[], int>
    {
        public RawArrayPool()
            : base(new SystemDictionary<int, IPool<T[]>>())
        {
        }
        
        protected override IPool<T[]> CreatePool(int classId)
        {
            return new FixedLengthRawArrayPool<T>(classId);
        }

        protected sealed override int Classify(int param0)
        {
            return BitMath.NextPow2((uint)param0);
        }

        protected sealed override int Classify(T[] array)
        {
            return array.Length;
        }
    }
}