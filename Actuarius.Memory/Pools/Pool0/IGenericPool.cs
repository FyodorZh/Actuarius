using System;
using System.Threading;

namespace Actuarius.Memory
{
    public interface IGenericPool
    {
        IPool<TResource> GetPool<TResource>() where TResource : class, new();
        (TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>() where TResource : class, new();
    }
    
    public interface IGenericConcurrentPool : IGenericPool
    {
        new IConcurrentPool<TResource> GetPool<TResource>() where TResource : class, new();
    }
    
    public static class IGenericPool_Ext
    {
        public static PoolableResourceDisposer<TResource> AcquireAsDisposable<TResource>(this IGenericPool pool, Action<TResource> onDispose)
            where TResource : class, new()
        {
            var pair = pool.Acquire<TResource>();
            return new PoolableResourceDisposer<TResource>(pair.resource, pair.poolSink, onDispose);
        }
    }
}