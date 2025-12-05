using System;
using System.Runtime.CompilerServices;
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

        protected override void OnReleased()
        {
            var resource = Interlocked.Exchange(ref _resource, null);
            if (resource != null) // to make compiler happy
            {
                _poolSink.Release(resource);
            }
        }

        public TResource ExposeResourceUnsafe(out TResource resource)
        {
            return resource = Resource;
        }
    }

    public static class PoolableResourceOwner_Ext
    {
        public static IMultiRefResourceOwner<TResource> AsOwner<TResource>(this (TResource resource, IPoolSink<TResource> poolSink) self)
            where TResource : class
        {
            return new PoolableResourceOwner<TResource>(self.resource, self.poolSink);
        }
    }
}