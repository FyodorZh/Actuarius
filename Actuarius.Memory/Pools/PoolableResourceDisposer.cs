using System;
using System.Threading;

namespace Actuarius.Memory
{
    public struct PoolableResourceDisposer<TResource> : IDisposable
        where TResource : class
    {
        private TResource? _resource;
        private readonly IPoolSink<TResource> _sink;
        private readonly Action<TResource> _onDispose;

        public readonly TResource Resource => _resource ?? throw new NullReferenceException();
            
        public PoolableResourceDisposer(TResource resource, IPoolSink<TResource> sink, Action<TResource> onDispose)
        {
            _resource = resource;
            _sink = sink;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            var resource = Interlocked.Exchange(ref _resource, null);
            if (resource != null)
            {
                _onDispose.Invoke(resource);
                _sink.Release(resource);
            }
        }
    }
}