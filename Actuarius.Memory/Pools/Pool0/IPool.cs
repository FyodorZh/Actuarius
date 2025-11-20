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
}