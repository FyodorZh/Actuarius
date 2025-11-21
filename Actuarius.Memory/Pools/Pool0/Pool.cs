using Actuarius.Collections;

namespace Actuarius.Memory
{
    public abstract class Pool<TResource> : IPool<TResource>
        where TResource : class
    {
        private readonly IUnorderedCollection<TResource> _pool;
        
        protected abstract TResource Constructor();
        
        protected Pool()
            : this(new CycleQueue<TResource>())
        {
        }

        protected Pool(IUnorderedCollection<TResource> pool)
        {
            _pool = pool;
        }

        public void Release(TResource? resource)
        {
            if (resource != null)
            {
                _pool.Put(resource);
            }
        }

        public TResource Acquire()
        {
            if (_pool.TryPop(out var resource))
            {
                return resource;
            }
            return Constructor();
        }
    }
}