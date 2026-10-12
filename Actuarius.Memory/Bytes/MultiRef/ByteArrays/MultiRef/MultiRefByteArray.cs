using System;

namespace Actuarius.Memory
{
    public class MultiRefByteArray : MultiRefResource, IMultiRefByteArray
    {
        private byte[] _array = null!;
        private int _offset;
        private int _count;

        public MultiRefByteArray(byte[] array, int offset = 0, int count = -1)
            : this(array, offset, count, revivable: false)
        {
        }

        protected MultiRefByteArray(byte[] array, int offset, int count, bool revivable)
            : base(revivable)
        {
            Init(array, offset, count);
        }

        protected void Init(byte[] array, int offset, int count)
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