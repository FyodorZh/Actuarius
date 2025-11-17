namespace Actuarius.Collections
{
    /// <summary>
    /// Элементы изымаются в порядке добавления
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IQueue<TData> : IStream<TData>, ICountable
    {
    }
}