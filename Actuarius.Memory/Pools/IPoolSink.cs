namespace Actuarius.Memory
{
    public interface IPoolSink<in TResource>
    {
        /// <summary>
        /// Вернуть объект в пул.
        /// </summary>
        /// <param name="obj"> Можно возвращать default(TObject) </param>
        void Release(TResource? obj);
    }

    public interface IConcurrentPoolSink<in TResource> : IPoolSink<TResource>
    {
    }
}