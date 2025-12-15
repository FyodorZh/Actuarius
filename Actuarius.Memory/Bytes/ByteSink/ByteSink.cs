namespace Actuarius.Memory
{
    public struct ByteSink : IByteSink
    {
        private int _position;
        private readonly IByteArray _array;

        public int Position
        {
            get => _position;
            set => _position = value;
        }
        
        public ByteSink(IByteArray array)
        {
            _array = array;
            _position = 0;
        }

        public bool Put(byte value)
        {
            if (_position < _array.Count)
            {
                _array[_position++] = value;
                return true;
            }

            return false;
        }

        public bool PutMany<TBytes>(TBytes bytes) where TBytes : IReadOnlyBytes
        {
            if (_position + bytes.Count <= _array.Count)
            {
                if (bytes.CopyTo(_array.Array, _array.Offset + _position, 0, bytes.Count))
                {
                    _position += bytes.Count;
                    return true;
                }
            }

            return false;
        }
    }
}