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
}