using System;

namespace Actuarius.Memory
{
    public struct ReleasableResourceAccessor<TResource> : IDisposable
        where TResource : class
    {
        private TResource? _resource;
        private IReleasableResource? _owner;
        
        public TResource Resource => _resource ?? throw new InvalidOperationException();
        
        public ReleasableResourceAccessor(TResource resource, IReleasableResource owner)
        {
            _resource = resource;
            _owner = owner;
        }

        public void Dispose()
        {
            _resource = null;
            _owner?.Release();
            _owner = null;
        }
    }
}