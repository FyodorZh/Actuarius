using System;

namespace Actuarius.Concurrent
{
    public interface IObservableDictionary<TKey, TData> : IObservable<ObservableDictionaryPatch<TKey, TData>>
        where TKey : notnull
    {
#if NETSTANDARD2_1
        IObservableDictionary<TKey, TNewData> ConvertDataTo<TNewData>(Func<TData, TNewData> converter)
        {
            return new ObservableDictionary_Converter_Data<TKey, TData, TNewData>(this, converter);
        }
        
        IObservableDictionary<TNewKey, TNewData> ConvertTo<TNewKey, TNewData>(Func<TKey, TNewKey> keyConverter, Func<TData, TNewData> dataConverter)
            where TNewKey : notnull
        {
            return new ObservableDictionary_Converter<TNewKey, TNewData, TKey, TData>(this, keyConverter, dataConverter);
        }
#endif
    }
}