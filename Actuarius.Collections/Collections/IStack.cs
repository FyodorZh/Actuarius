namespace Actuarius.Collections
{
    /// <summary>
    /// Элементы изымаются в порядке LIFO
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IStack<TData> : IStream<TData>, ICountable
    {
    }
}