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
        
        public StaticReadOnlyByteArray(byte[] array, int offset = 0, int length = -1)
        {
            length = length < 0 ? array.Length - offset : length;
            
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

        public IMultiRefResourceMonitor ConstructMonitor(bool collectHistory = true, bool trackLeaks = true, bool traceEnabled = true)
        {
            return MultiRefResourceVoidMonitor.Instance;
        }

        public int Count => _length;

        public bool IsValid => _array != null!;

        public bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)
        {
            return IReadOnlyByteArray_Ext.CopyTo(this, dst, dstOffset, srcOffset, count);
        }

        public byte this[int id] => _array[_offset + id];

        public byte[] ReadOnlyArray => _array;

        public int Offset => _offset;
    }
}