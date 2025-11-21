using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class DelegatePool<TResource> : Pool<TResource>
        where TResource : class
    {
        private readonly Func<TResource> _ctor;
        
        public DelegatePool(Func<TResource> resourceCtor)
            : this(resourceCtor, new CycleQueue<TResource>())
        {
        }

        public DelegatePool(Func<TResource> resourceCtor, IUnorderedCollection<TResource> pool)
            :base(pool)
        {
            _ctor = resourceCtor;
        }

        protected override TResource Constructor()
        {
            return _ctor();
        }
    }
}