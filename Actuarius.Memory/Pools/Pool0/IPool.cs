using System;

namespace Actuarius.Memory
{
    public interface IPool<TResource> : IPoolSink<TResource>
    {
        /// <summary>
        /// Получить свободный объект из пула
        /// </summary>
        TResource Acquire();
    }

    public interface IConcurrentPool<TResource> : IPool<TResource>, IConcurrentPoolSink<TResource>
    {
    }
    
    public static class IPool_Ext
    {
        public static PoolableResourceDisposer<TResource> AcquireAsDisposable<TResource>(this IPool<TResource> pool, Action<TResource> onDispose)
            where TResource : class, new()
        {
            var resource = pool.Acquire();
            return new PoolableResourceDisposer<TResource>(resource, pool, onDispose);
        }
    }
}