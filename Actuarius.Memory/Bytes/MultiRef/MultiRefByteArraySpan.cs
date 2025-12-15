using System;

namespace Actuarius.Memory
{
    public class MultiRefByteArraySpan : MultiRefByteArray
    {
        private IMultiRefByteArray _source;
        
        public MultiRefByteArraySpan(IMultiRefByteArray source, int offset, int count) 
            : base(source.Array, source.Offset + offset, count)
        {
            if (!ArrayHelper.CheckRange(source.Count, offset, count))
            {
                throw new IndexOutOfRangeException();
            }
            _source = source.Acquire();
        }

        protected override void OnReleased()
        {
            _source.Release();
            _source = null!;
            base.OnReleased();
        }
    }

    public static class MultiRefByteArraySpan_Ext
    {
        public static IMultiRefByteArray GetSpan(IMultiRefByteArray source, int offset, int length)
        {
            return new MultiRefByteArraySpan(source, offset, length);
        }
    }
}