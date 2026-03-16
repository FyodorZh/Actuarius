using System;
using System.Collections.Generic;
using System.Linq;

namespace Actuarius.Concurrent
{
    public class ObservableDictionary_Converter_Data<TKey, TFromData, TData> : IObservableDictionary<TKey, TData>
        where TKey : notnull
    {
        private readonly IObservableDictionary<TKey, TFromData> _core;
        private readonly Func<TFromData, TData> _converter;

        public ObservableDictionary_Converter_Data(IObservableDictionary<TKey, TFromData> core, Func<TFromData, TData> converter)
        {
            _core = core;
            _converter = converter;
        }

        public IDisposable Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observable)
        {
            return _core.Subscribe(patch =>
                {
                    observable.OnNext(new ObservableDictionaryPatch<TKey, TData>(
                        patch.Added.Select(kv => new KeyValuePair<TKey, TData>(kv.Key, _converter(kv.Value))).ToArray(), 
                        patch.Updated.Select(kv => new KeyValuePair<TKey, TData>(kv.Key, _converter(kv.Value))).ToArray(), 
                        patch.Removed));
                },
                observable.OnError,
                observable.OnCompleted);
        }
    }
}