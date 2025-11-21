using System;
using System.Threading;

namespace Actuarius.Memory
{
    public class MultiRefResourceOwner<TResource> : MultiRefResource, IMultiRefResourceOwner<TResource>
        where TResource : class
    {
        private readonly IPoolSink<TResource> _poolSink;

        private TResource? _resource;

        private TResource Resource => _resource ?? throw new Exception($"{GetType()}: access after final release");

        public MultiRefResourceOwner(TResource resource, IPoolSink<TResource> poolSink) 
            : base(false)
        {
            _poolSink = poolSink;
            _resource = resource;
        }
        
        public ReleasableResourceAccessor<TResource> GetAccessor()
        {
            return new ReleasableResourceAccessor<TResource>(Resource, this.Acquire());
        }

        public TResource ShowUnsafe()
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
    }
}