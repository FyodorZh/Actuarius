using System;

namespace Actuarius.Memory
{
    public class MultiRefByteArray : MultiRefResource, IMultiRefByteArray
    {
        private byte[] _array;
        private readonly int _offset;
        private readonly int _count;
        
        public MultiRefByteArray(byte[] array, int offset = 0, int count = -1) 
            : base(false)
        {
            _array = array;
            _offset = offset;
            _count = count < 0 ? array.Length - offset : count;
        }

        protected override void OnReleased()
        {
            _array = null!;
        }

        public int Count => _count;
        
        public int Offset => _offset;

        public virtual bool IsValid => _array != null!;

        public virtual bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            return IReadOnlyByteArray_Ext.CopyTo(this, dst, dstOffset, srcOffset, count);
        }

        public byte this[int id]
        {
            get => _array[_offset + id];
            set => _array[_offset + id] = value;
        }

        public byte[] ReadOnlyArray => _array;

        public byte[] Array => _array;
    }
}