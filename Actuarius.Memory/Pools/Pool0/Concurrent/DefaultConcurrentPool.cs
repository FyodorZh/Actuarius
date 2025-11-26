using Actuarius.Collections;

namespace Actuarius.Memory
{
    public class DefaultConcurrentPool<TResource> : ConcurrentDelegatePool<TResource>
        where TResource : class, new()
    {
        public DefaultConcurrentPool(IConcurrentUnorderedCollection<TResource> pool)
            : base(() => new(), pool)
        {
        }
    }
}