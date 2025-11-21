namespace Actuarius.Memory
{
    public class MultiRefByteArrayFromPool : MultiRefByteArray
    {
        private IPoolSink<byte[]>? _pool;
        
        public MultiRefByteArrayFromPool(IPoolSink<byte[]> pool, byte[] array, int offset, int count)
            : base(array, offset, count)
        {
            _pool = pool;
        }

        protected override void OnReleased()
        {
            var array = Array;
            base.OnReleased();
            _pool?.Release(array);
            _pool = null;
        }
    }
}