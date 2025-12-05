using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class FixedLengthRawArrayConcurrentPool<T>: ConcurrentPool<T[]>
    {
        private readonly int _length;
        
        public FixedLengthRawArrayConcurrentPool(int length, int capacity)
            :base(new LimitedConcurrentQueue<T[]>(capacity))
        {
            _length = length;
        }

        protected override T[] Constructor()
        {
            return new T[_length];
        }
    }
}