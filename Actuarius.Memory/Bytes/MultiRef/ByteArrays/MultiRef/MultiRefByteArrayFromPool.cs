namespace Actuarius.Memory
{
    public class MultiRefByteArrayFromPool : MultiRefByteArray
    {
        protected IPoolSink<byte[]>? Pool { get; set; }

        public MultiRefByteArrayFromPool(IPoolSink<byte[]> pool, byte[] array, int offset, int count)
            : this(pool, array, offset, count, revivable: false)
        {
        }

        protected MultiRefByteArrayFromPool(
            IPoolSink<byte[]> pool,
            byte[] array,
            int offset,
            int count,
            bool revivable)
            : base(array, offset, count, revivable)
        {
            Pool = pool;

            if (revivable)
            {
                Revive();
            }
        }

        protected override void OnReleased()
        {
            var array = Array;
            base.OnReleased();
            Pool?.Release(array);
            Pool = null;
        }
    }

    public sealed class RecyclableMultiRefByteArrayFromPool : MultiRefByteArrayFromPool
    {
        private readonly IPoolSink<RecyclableMultiRefByteArrayFromPool> _wrapperSink;

        public RecyclableMultiRefByteArrayFromPool(
            IPoolSink<byte[]> pool,
            byte[] array,
            int offset,
            int count,
            IPoolSink<RecyclableMultiRefByteArrayFromPool> wrapperSink)
            : base(pool, array, offset, count, revivable: true)
        {
            _wrapperSink = wrapperSink;
        }

        internal bool TryRebind(IPoolSink<byte[]> pool, byte[] array, int offset, int count)
        {
            if (!Revive())
            {
                return false;
            }

            Init(array, offset, count);
            Pool = pool;
            return true;
        }

        protected override void OnReleased()
        {
            base.OnReleased();

            if (Monitor == null)
            {
                _wrapperSink.Release(this);
            }
        }
    }
}
