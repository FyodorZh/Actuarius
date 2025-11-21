using Actuarius.Collections;

namespace Actuarius.Memory
{
    public abstract class ConcurrentPool<TResource, TParam> : Pool<TResource, TParam>, IConcurrentPool<TResource, TParam>
        where TResource : class
    {
        protected ConcurrentPool(IConcurrentMap<int, IPool<TResource>> table)
            : base(table)
        {
        }
    }
}