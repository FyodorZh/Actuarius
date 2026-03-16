using System.Collections.Generic;

namespace Actuarius.Concurrent
{
    public readonly struct ObservableDictionaryPatch<TKey, TValue>
    {
        public IReadOnlyList<KeyValuePair<TKey, TValue>> Added { get; }
        public IReadOnlyList<KeyValuePair<TKey, TValue>> Updated { get; }
        public IReadOnlyList<TKey> Removed { get; }

        public ObservableDictionaryPatch(
            IReadOnlyList<KeyValuePair<TKey, TValue>> added,
            IReadOnlyList<KeyValuePair<TKey, TValue>> updated, 
            IReadOnlyList<TKey> removed)
        {
            Added = added;
            Updated = updated;
            Removed = removed;
        }
        
        /// <summary>
        /// Applies this patch to the specified dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary to apply changes to</param>
        public void ApplyTo(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new System.ArgumentNullException(nameof(dictionary));
            }
            
            // Remove keys first
            foreach (var key in Removed)
            {
                dictionary.Remove(key);
            }
            
            // Then add new elements
            foreach (var kvp in Added)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
            
            // Finally, update existing elements
            foreach (var kvp in Updated)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }
    }
}