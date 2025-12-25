using System;

namespace Actuarius.Memory
{
    public interface IPool<TResource, in TParam0> : IPoolSink<TResource>
    {
        /// <summary>
        /// Получить свободный объект из пула
        /// </summary>
        TResource Acquire(TParam0 param0);
        
        (TResource resource, IPoolSink<TResource> poolSink) AcquireEx(TParam0 param0); // TODO
    }

    public interface IConcurrentPool<TResource, in TParam0> : IPool<TResource, TParam0>, IConcurrentPoolSink<TResource>
    {
    }
    
    public static class IPool2_Ext
    {
        public static PoolableResourceDisposer<TResource> AcquireAsDisposable<TResource, TParam0>(this IPool<TResource, TParam0> pool, TParam0 param0, Action<TResource> onDispose)
            where TResource : class
        {
            var pair = pool.AcquireEx(param0);
            return new PoolableResourceDisposer<TResource>(pair.resource, pair.poolSink, onDispose);
        }
    }
}