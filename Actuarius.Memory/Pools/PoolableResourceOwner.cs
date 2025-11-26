using System;
using System.Threading;

namespace Actuarius.Memory
{
    /// <summary>
    /// Управляет ресурсом полученным из пула
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class PoolableResourceOwner<TResource> : MultiRefResource, IMultiRefResourceOwner<TResource>
        where TResource : class
    {
        private readonly IPoolSink<TResource> _poolSink;

        private TResource? _resource;

        private TResource Resource => _resource ?? throw new Exception($"{GetType()}: access after final release");

        public PoolableResourceOwner(TResource resource, IPoolSink<TResource> poolSink) 
            : base(false)
        {
            _poolSink = poolSink;
            _resource = resource;
        }
        
        public TResource ShowResourceUnsafe()
        {
            return _resource ?? throw new NullReferenceException($"{GetType()}: access after final release");
        }

        protected override void OnReleased()
        {
            var resource = Interlocked.Exchange(ref _resource, null);
            if (resource != null) // to make compiler happy
            {
                _poolSink.Release(resource);
            }
        }

        public TResource ShowResourceUnsafe(out TResource resource)
        {
            return resource = Resource;
        }
    }
}