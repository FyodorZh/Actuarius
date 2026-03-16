using System;
using System.Linq;

namespace Actuarius.Concurrent
{
    public class ObservableSet_Converter<From, To> : IObservableSet<To>
    {
        private readonly IObservableSet<From> _core;
        private readonly Func<From, To> _converter;

        public ObservableSet_Converter(IObservableSet<From> core, Func<From, To> converter)
        {
            _core = core;
            _converter = converter;
        }

        public IDisposable Subscribe(IObserver<ObservableSetPatch<To>> observable)
        {
            return _core.Subscribe(patch =>
                {
                    observable.OnNext(new ObservableSetPatch<To>(
                        patch.Added.Select(_converter).ToArray(), 
                        patch.Removed.Select(_converter).ToArray()));
                },
                observable.OnError,
                observable.OnCompleted);
        }
    }
}