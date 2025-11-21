using System;

namespace Actuarius.Memory
{
    public class MultiRefByteArray : MultiRefResource, IMultiRefByteArray
    {
        private byte[] _array;
        private readonly int _offset;
        private readonly int _count;
        
        public MultiRefByteArray(byte[] array, int offset, int count) 
            : base(false)
        {
            _array = array;
            _offset = offset;
            _count = count;
        }

        protected override void OnReleased()
        {
            _array = null!;
        }

        public ReleasableResourceAccessor<IByteArray> GetAccessor()
        {
            return new ReleasableResourceAccessor<IByteArray>(this, this);
        }

        public IByteArray ShowUnsafe()
        {
            return this;
        }

        public int Count => _count;
        
        public int Offset => _offset;

        public virtual bool IsValid => _array != null;

        public bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            Buffer.BlockCopy(_array, _offset, dst, dstOffset, count);
            return true;
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