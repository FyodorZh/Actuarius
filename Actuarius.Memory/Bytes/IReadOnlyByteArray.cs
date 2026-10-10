using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public interface IReadOnlyByteArray: IReadOnlyBytes, IReadOnlyArray<byte>
    {
        byte[] ReadOnlyArray { get; }
        int Offset { get; }
    }
    
    public static class IReadOnlyByteArray_Ext
    {
        public static bool CopyTo(this IReadOnlyByteArray src, byte[] dst, int dstOffset, int srcOffset, int count)
        {
            if (src.ReadOnlyArray == null! || dst == null! || srcOffset < 0 || 
                !ArrayHelper.CheckFromTo(src.Offset + src.Count, src.Offset + srcOffset, dst.Length, dstOffset, count))
            {
                return false;
            }
            Buffer.BlockCopy(src.ReadOnlyArray, src.Offset + srcOffset, dst, dstOffset, count);
            return true;
        }
    }
}