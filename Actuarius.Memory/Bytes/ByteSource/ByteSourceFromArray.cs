using System;

namespace Actuarius.Memory
{
    public struct ByteSourceFromArray : IByteSource
    {
        private readonly IReadOnlyByteArray _array;
        private int _position;

        public int Position
        {
            get => _position;
            set => _position = value;
        }
        
        public ByteSourceFromArray(IReadOnlyByteArray array)
        {
            _array = array;
            _position = 0;
        }

        public bool TryPop(out byte value)
        {
            if (_position < _array.Count)
            {
                value = _array[_position++];
                return true;
            }

            value = 0;
            return false;
        }

        public bool TakeMany(IMultiRefByteArray dst)
        {
            if (_position + dst.Count <= _array.Count)
            {
                Buffer.BlockCopy(_array.ReadOnlyArray, _array.Offset + _position, dst.Array, dst.Offset, dst.Count);
                _position += dst.Count;
                return true;
            }

            return false;
        }
    }
}