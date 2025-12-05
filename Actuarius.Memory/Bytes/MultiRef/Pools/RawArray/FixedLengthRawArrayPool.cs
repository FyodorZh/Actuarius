namespace Actuarius.Memory
{
    public class FixedLengthRawArrayPool<T> : Pool<T[]>
    {
        private readonly int _length;
        
        public FixedLengthRawArrayPool(int length)
        {
            _length = length;
        }

        protected override T[] Constructor()
        {
            return new T[_length];
        }
    }
}