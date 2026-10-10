namespace Actuarius.Memory
{
    public class ByteArrayPool : IPool<IMultiRefByteArray, int>
    {
        private readonly IPool<byte[], int> _bytesPool;
        
        public ByteArrayPool(IPool<byte[], int> bytesPool)
        {
            _bytesPool = bytesPool;
        }

        public void Release(IMultiRefByteArray? resource)
        {
            // DO NOTHING. OR Collect MultiRefByteArrayFromPool instance
        }

        public IMultiRefByteArray Acquire(int param0)
        {
            return new MultiRefByteArrayFromPool(_bytesPool, _bytesPool.Acquire(param0), 0,  param0);
        }

        public (IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink) AcquireEx(int param0)
        {
            (byte[] resource, IPoolSink<byte[]> poolSink) = _bytesPool.AcquireEx(param0);
            return (new MultiRefByteArrayFromPool(poolSink, resource, 0,  param0), this);
        }
    }
}