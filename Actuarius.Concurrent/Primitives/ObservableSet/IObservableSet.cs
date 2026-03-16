using System;

namespace Actuarius.Concurrent
{
    public interface IObservableSet<T> : IObservable<ObservableSetPatch<T>>
    {
    }
}