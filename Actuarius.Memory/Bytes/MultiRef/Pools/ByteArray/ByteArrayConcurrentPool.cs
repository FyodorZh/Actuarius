namespace Actuarius.Memory
{
    public class ByteArrayConcurrentPool : ByteArrayPool, IConcurrentPool<IMultiRefByteArray, int>
    {
        public ByteArrayConcurrentPool(IConcurrentPool<byte[], int> bytesPool) : base(bytesPool)
        {
        }
    }
}