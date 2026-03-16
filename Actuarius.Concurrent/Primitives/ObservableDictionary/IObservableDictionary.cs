using System;

namespace Actuarius.Concurrent
{
    public interface IObservableDictionary<TKey, TData> : IObservable<ObservableDictionaryPatch<TKey, TData>>
        where TKey : notnull
    {
    }
}