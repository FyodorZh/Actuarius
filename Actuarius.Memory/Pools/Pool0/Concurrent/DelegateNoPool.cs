using System;

namespace Actuarius.Memory
{
    public class DelegateNoPool<TResource> : IConcurrentPool<TResource>
    {
        private readonly Func<TResource> _resourceFactory;

        public DelegateNoPool(Func<TResource> resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }
        
        public TResource Acquire()
        {
            return _resourceFactory.Invoke();
        }
        
        public void Release(TResource? obj)
        {
            // DO NOTHING
        }
    }
}