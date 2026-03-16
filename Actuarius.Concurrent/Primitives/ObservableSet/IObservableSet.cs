using System;

namespace Actuarius.Concurrent
{
    public interface IObservableSet<TData> : IObservable<ObservableSetPatch<TData>>
    {
#if NETSTANDARD2_1
        IObservableSet<TNew> ConvertTo<TNew>(Func<TData, TNew> converter)
        {
            return new ObservableSet_Converter<TData, TNew>(this, converter);
        }
#endif
    }
}