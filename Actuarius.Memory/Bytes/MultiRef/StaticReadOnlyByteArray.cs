using System;

namespace Actuarius.Memory
{
    public class StaticReadOnlyByteArray : IMultiRefReadOnlyByteArray
    {
        protected readonly byte[] _array;
        protected readonly int _offset;
        protected readonly int _length;
        
        public StaticReadOnlyByteArray(byte[] array)
            : this(array, 0, array.Length)
        {}
        
        public StaticReadOnlyByteArray(byte[] array, int offset, int length)
        {
            if (!ArrayHelper.CheckRange(array.Length, offset, length))
            {
                throw new ArgumentOutOfRangeException();
            }
            _array = array;
            _offset = offset;
            _length = length;
        }
        
        public void Release() { }

        public bool IsAlive => true;

        public void AddRef()
        {
        }

        public int Count => _length;

        public bool IsValid => true;

        public bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            if (!ArrayHelper.CheckFromTo(_length, _offset + srcOffset, dst.Length, dstOffset, count))
            {
                return false;
            }
            Buffer.BlockCopy(_array, _offset + srcOffset, dst, dstOffset, count);
            return true;
        }

        public byte this[int id] => _array[_offset + id];

        public byte[] ReadOnlyArray => _array;

        public int Offset => _offset;

    }
}