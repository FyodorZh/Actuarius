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

        /// <summary>
        /// Applies this patch to the specified set
        /// </summary>
        /// <param name="set">The set to apply changes to</param>
        public void ApplyTo(ISet<T> set)
        {
            if (set == null)
            {
                throw new System.ArgumentNullException(nameof(set));
            }
            
            // Remove elements first
            foreach (var item in Removed)
            {
                set.Remove(item);
            }
            
            // Then add new elements
            foreach (var item in Added)
            {
                set.Add(item);
            }
        }
    }
}