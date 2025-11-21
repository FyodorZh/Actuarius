using System;
using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class ConcurrentDelegatePool<TResource> : DelegatePool<TResource>, IConcurrentPool<TResource>
        where TResource : class
    {
        public ConcurrentDelegatePool(Func<TResource> resourceCtor, IConcurrentUnorderedCollection<TResource> pool)
            : base(resourceCtor, pool)
        {
        }
    }
}