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
            throw new System.NotImplementedException();
        }

        public IMultiRefByteArray Acquire(int param0)
        {
            return new MultiRefByteArrayFromPool(_rawPool, _rawPool.Acquire(param0), 0,  param0);
        }
    }
}