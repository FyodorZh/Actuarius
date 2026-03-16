using System.Collections.Generic;

namespace Actuarius.Concurrent
{
    public readonly struct ObservableSetPatch<T>
    {
        public IReadOnlyList<T> Added { get; }
        public IReadOnlyList<T> Removed { get; }

        public ObservableSetPatch(IReadOnlyList<T> added, IReadOnlyList<T> removed)
        {
            Added = added;
            Removed = removed;
        }
    }
}