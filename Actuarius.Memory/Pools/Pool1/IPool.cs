namespace Actuarius.Memory
{
    public interface IPool<TResource, in TParam0> : IPoolSink<TResource>
    {
        /// <summary>
        /// Получить свободный объект из пула
        /// </summary>
        TResource Acquire(TParam0 param0);
    }

    public interface IConcurrentPool<TResource, in TParam0> : IPool<TResource, TParam0>, IConcurrentPoolSink<TResource>
    {
    }
}