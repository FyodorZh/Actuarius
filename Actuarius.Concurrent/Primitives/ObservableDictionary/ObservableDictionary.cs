using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Actuarius.Concurrent
{
    /// <summary>
    /// Represents an observable dictionary that emits patches describing changes to its contents.
    /// This class is thread-safe and uses locking to ensure consistency.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. Must be not null.</typeparam>
    /// <typeparam name="TData">The type of the values in the dictionary.</typeparam>
    public class ObservableDictionary<TKey, TData> : IObservableDictionary<TKey, TData>
        where TKey : notnull
    {
        private readonly Subject<ObservableDictionaryPatch<TKey, TData>> _patchStream = new();

        private readonly Dictionary<TKey, TData> _currentDictionary;
        private readonly Dictionary<TKey, Flag> _keyMap;

        private readonly IEqualityComparer<TData> _dataComparer = null!;

        private readonly object _lock = new();

        private bool _completed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TData}"/> class.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for keys, or null to use the default comparer.</param>
        /// <param name="dataComparer">The comparer to use for data equality checks, or null to use the default comparer.</param>
        public ObservableDictionary(
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TData>? dataComparer = null)
        {
            _currentDictionary = new(keyComparer);
            _keyMap = new Dictionary<TKey, Flag>(keyComparer);
            _dataComparer = dataComparer ?? EqualityComparer<TData>.Default;
        }

        /// <summary>
        /// Subscribes an observer to the dictionary's patch stream.
        /// If the dictionary is already completed, the observer is immediately notified of completion.
        /// Otherwise, if the dictionary has existing elements, an initial patch with all current elements is emitted.
        /// </summary>
        /// <param name="observer">The observer to subscribe. Cannot be null.</param>
        /// <returns>A disposable that can be used to unsubscribe from the patch stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="observer"/> is null.</exception>
        public IDisposable Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_lock)
            {
                if (_completed)
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                if (_currentDictionary.Count > 0)
                {
                    var delta = new ObservableDictionaryPatch<TKey, TData>(
                        _currentDictionary.ToArray(),
                        Array.Empty<KeyValuePair<TKey, TData>>(),
                        Array.Empty<TKey>());
                    observer.OnNext(delta);
                }

                return _patchStream.Subscribe(observer);
            }
        }

        /// <summary>
        /// Clears all elements from the dictionary and emits a patch indicating all keys were removed.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                if (_completed)
                {
                    return;
                }

                ObservableDictionaryPatch<TKey, TData> patch = new ObservableDictionaryPatch<TKey, TData>(
                    Array.Empty<KeyValuePair<TKey, TData>>(),
                    Array.Empty<KeyValuePair<TKey, TData>>(),
                    _currentDictionary.Keys.ToArray());

                _currentDictionary.Clear();
                _keyMap.Clear();

                _patchStream.OnNext(patch);
            }
        }

        /// <summary>
        /// Sets the dictionary to contain exactly the specified elements.
        /// This method compares the new set with the current state and emits a patch describing additions, updates, and removals.
        /// Keys that are present in the new set but not in the current dictionary are added.
        /// Keys that are present in both but with different values are updated.
        /// Keys that are present in the current dictionary but not in the new set are removed.
        /// </summary>
        /// <param name="elements">The elements to set in the dictionary. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="elements"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a key is redefined in the elements collection.</exception>
        public void Set(IEnumerable<KeyValuePair<TKey, TData>> elements)
        {
            if (elements == null!)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            lock (_lock)
            {
                if (_completed)
                {
                    return;
                }

                foreach (var kv in _keyMap)
                {
                    _keyMap[kv.Key] = Flag.Present;
                }

                foreach (var kvp in elements)
                {
                    if (_keyMap.TryGetValue(kvp.Key, out var flag))
                    {
                        if (flag == Flag.Present)
                        {
                            if (_dataComparer.Equals(kvp.Value, _currentDictionary[kvp.Key]))
                            {
                                _keyMap[kvp.Key] = Flag.Preserved;
                            }
                            else
                            {
                                _currentDictionary[kvp.Key] = kvp.Value;
                                _keyMap[kvp.Key] = Flag.Updated;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Key {kvp.Key} redefinition");
                        }
                    }
                    else
                    {
                        _keyMap.Add(kvp.Key, Flag.Added);
                        _currentDictionary.Add(kvp.Key, kvp.Value);
                    }
                }

                List<KeyValuePair<TKey, TData>>? added = null;
                List<KeyValuePair<TKey, TData>>? updated = null;
                List<TKey>? removed = null;

                foreach (var kv in _keyMap)
                {
                    if (kv.Value == Flag.Added)
                    {
                        added ??= new List<KeyValuePair<TKey, TData>>();
                        added.Add(new KeyValuePair<TKey, TData>(kv.Key, _currentDictionary[kv.Key]));
                    }
                    else if (kv.Value == Flag.Updated)
                    {
                        updated ??= new List<KeyValuePair<TKey, TData>>();
                        updated.Add(new KeyValuePair<TKey, TData>(kv.Key, _currentDictionary[kv.Key]));
                    }
                    else if (kv.Value == Flag.Present)
                    {
                        removed ??= new List<TKey>();
                        removed.Add(kv.Key);
                    }
                }

                if (added == null && updated == null && removed == null)
                {
                    return;
                }

                if (removed != null)
                {
                    foreach (var key in removed)
                    {
                        _keyMap.Remove(key);
                        _currentDictionary.Remove(key);
                    }
                }

                ObservableDictionaryPatch<TKey, TData> patch = new(
                    added ?? (IReadOnlyList<KeyValuePair<TKey, TData>>)Array.Empty<KeyValuePair<TKey, TData>>(),
                    updated ?? (IReadOnlyList<KeyValuePair<TKey, TData>>)Array.Empty<KeyValuePair<TKey, TData>>(),
                    removed ?? (IReadOnlyList<TKey>)Array.Empty<TKey>());

                _patchStream.OnNext(patch);
            }
        }

        /// <summary>
        /// Adds new elements or updates existing elements in the dictionary.
        /// For each element, if the key already exists, its value is updated; otherwise, a new key-value pair is added.
        /// Emits a patch with the added and updated elements.
        /// </summary>
        /// <param name="elements">The elements to add or update. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="elements"/> is null.</exception>
        public void AddOrUpdate(IEnumerable<KeyValuePair<TKey, TData>> elements)
        {
            if (elements == null!)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            lock (_lock)
            {
                if (_completed)
                {
                    return;
                }

                List<KeyValuePair<TKey, TData>>? added = null;
                List<KeyValuePair<TKey, TData>>? updated = null;

                foreach (var kvp in elements)
                {
                    if (_currentDictionary.ContainsKey(kvp.Key))
                    {
                        _currentDictionary[kvp.Key] = kvp.Value;

                        updated ??= new List<KeyValuePair<TKey, TData>>();
                        updated.Add(kvp);
                    }
                    else
                    {
                        _currentDictionary[kvp.Key] = kvp.Value;
                        _keyMap[kvp.Key] = Flag.Present;

                        added ??= new List<KeyValuePair<TKey, TData>>();
                        added.Add(kvp);
                    }
                }

                if (added != null || updated != null)
                {
                    _patchStream.OnNext(new ObservableDictionaryPatch<TKey, TData>(
                        added ?? (IReadOnlyList<KeyValuePair<TKey, TData>>)Array.Empty<KeyValuePair<TKey, TData>>(),
                        updated ?? (IReadOnlyList<KeyValuePair<TKey, TData>>)Array.Empty<KeyValuePair<TKey, TData>>(),
                        Array.Empty<TKey>()));
                }
            }
        }

        /// <summary>
        /// Removes the specified keys from the dictionary.
        /// Emits a patch with the removed keys if any were successfully removed.
        /// </summary>
        /// <param name="keys">The keys to remove. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="keys"/> is null.</exception>
        public void Remove(IEnumerable<TKey> keys)
        {
            if (keys == null!)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            lock (_lock)
            {
                if (_completed)
                {
                    return;
                }

                List<TKey>? removed = null;
                foreach (var key in keys)
                {
                    if (_currentDictionary.Remove(key))
                    {
                        _keyMap.Remove(key);
                        removed ??= new List<TKey>();
                        removed.Add(key);
                    }
                }

                if (removed != null)
                {
                    _patchStream.OnNext(new ObservableDictionaryPatch<TKey, TData>(
                        Array.Empty<KeyValuePair<TKey, TData>>(),
                        Array.Empty<KeyValuePair<TKey, TData>>(),
                        removed));
                }
            }
        }

        /// <summary>
        /// Completes the observable sequence, notifying all subscribers that no more patches will be emitted.
        /// </summary>
        public void Complete()
        {
            lock (_lock)
            {
                if (!_completed)
                {
                    _completed = true;
                    _patchStream.OnCompleted();
                }
            }
        }

        private enum Flag : byte
        {
            Present = 1,
            Added = 2,
            Updated = 4,
            Preserved = 8
        }
    }
}