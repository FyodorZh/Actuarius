namespace Actuarius.Memory
{
    internal static class ArrayHelper
    {
        public static bool CheckRange(int arrayLength, int rangeOffset, int rangeLength)
        {
            return rangeOffset >= 0 && rangeLength >= 0 &&
                   rangeOffset < arrayLength && rangeOffset + rangeLength <= arrayLength;
        }

        public static bool CheckFromTo(int srcLength, int srcOffset, int dstLength, int dstOffset, int count)
        {
            return CheckRange(srcLength, srcOffset, count) && CheckRange(dstLength, dstOffset, count);
        }
        
        public static bool CheckFromTo<T>(T[]? src, int srcOffset, T[]? dst, int dstOffset, int count)
        {
            return src != null && dst != null && CheckFromTo(src.Length, srcOffset, dst.Length, dstOffset, count);
        }
    }
}