namespace Actuarius.Collections
{
    public interface IMap<in TKey, TData>
    {
        bool Add(TKey key, TData element);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TData element);
    }

    public interface IConcurrentMap<in TKey, TData> : IMap<TKey, TData>
    {
    }
}