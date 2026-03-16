using System;
using System.Collections.Generic;
using System.Linq;

namespace Actuarius.Concurrent
{
    public class ObservableDictionary_Converter<TKey, TData, TKeyFrom, TDataFrom> : IObservableDictionary<TKey, TData>
        where TKey : notnull
        where TKeyFrom : notnull
    {
        private readonly IObservableDictionary<TKeyFrom, TDataFrom> _core;
        private readonly Func<TKeyFrom, TKey> _keyConverter;
        private readonly Func<TDataFrom, TData> _dataConverter;

        public ObservableDictionary_Converter(
            IObservableDictionary<TKeyFrom, TDataFrom> core, 
            Func<TKeyFrom, TKey> keyConverter,
            Func<TDataFrom, TData> dataConverter)
        {
            _core = core;
            _keyConverter = keyConverter;
            _dataConverter = dataConverter;
        }

        public IDisposable Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observable)
        {
            return _core.Subscribe(patch =>
                {
                    observable.OnNext(new ObservableDictionaryPatch<TKey, TData>(
                        patch.Added.Select(
                            kv => new KeyValuePair<TKey, TData>(_keyConverter(kv.Key), _dataConverter(kv.Value))).ToArray(), 
                        patch.Updated.Select(
                            kv => new KeyValuePair<TKey, TData>(_keyConverter(kv.Key), _dataConverter(kv.Value))).ToArray(),
                        patch.Removed.Select(_keyConverter).ToArray()));
                },
                observable.OnError,
                observable.OnCompleted);
        }
    }
}