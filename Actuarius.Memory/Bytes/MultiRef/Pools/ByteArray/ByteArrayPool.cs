namespace Actuarius.Memory
{
    public class ByteArrayPool : IPool<IMultiRefByteArray, int>
    {
        private readonly IPool<byte[], int> _rawPool;
        
        public ByteArrayPool(IPool<byte[], int> rawPool)
        {
            _rawPool = rawPool;
        }

        public void Release(IMultiRefByteArray? resource)
        {
            // DO NOTHING. OR Collect MultiRefByteArrayFromPool instance
        }

        public IMultiRefByteArray Acquire(int param0)
        {
            return new MultiRefByteArrayFromPool(_rawPool, _rawPool.Acquire(param0), 0,  param0);
        }

        public (IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink) AcquireEx(int param0)
        {
            (byte[] resource, IPoolSink<byte[]> poolSink) = _rawPool.AcquireEx(param0);
            return (new MultiRefByteArrayFromPool(poolSink, resource, 0,  param0), this);
        }
    }
}