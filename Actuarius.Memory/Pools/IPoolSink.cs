namespace Actuarius.Memory
{
    public interface IPoolRef {}
    
    public interface IPoolSink<in TResource> : IPoolRef
    {
        /// <summary>
        /// Вернуть объект в пул.
        /// </summary>
        /// <param name="resource"> Можно возвращать default(TObject) </param>
        void Release(TResource? resource);
    }

    public interface IConcurrentPoolSink<in TResource> : IPoolSink<TResource>
    {
    }
}