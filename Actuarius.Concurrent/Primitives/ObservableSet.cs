using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Actuarius.Concurrent
{   
    public interface IObservableSet<T> : IObservable<ObservableSetPatch<T>>
    {
    }
    
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
    
    public class ObservableSet<T>: IObservableSet<T>
        where T: notnull
    {
        private readonly Subject<ObservableSetPatch<T>> _patchStream = new();

        private readonly HashSet<T> _currentSet;
        private readonly Dictionary<T, Flag> _elementMap = new();

        private readonly object _lock = new();

        private bool _completed = false;

        public ObservableSet()
        {
            _currentSet = new();
        }
        
        public IDisposable Subscribe(IObserver<ObservableSetPatch<T>> observer)
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

                if (_currentSet.Count > 0)
                {
                    var delta = new ObservableSetPatch<T>(_currentSet.ToArray(), Array.Empty<T>());
                    observer.OnNext(delta);
                }

                return _patchStream.Subscribe(observer);    
            }
            
        }

        public void Set(IEnumerable<T> elements)
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
                
                foreach (var kv in _elementMap)
                {
                    _elementMap[kv.Key] = Flag.Present;
                }

                foreach (T element in elements)
                {
                    if (_elementMap.TryGetValue(element, out var flag))
                    {
                        if (flag == Flag.Present)
                        {
                            _elementMap[element] = Flag.Preserved;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Element {element} redefinition");
                        }
                    }
                    else
                    {
                        _elementMap[element] = Flag.Added;
                    }
                }

                List<T>? added = null;
                List<T>? removed = null;

                foreach (var kv in _elementMap)
                {
                    if (kv.Value == Flag.Added)
                    {
                        added ??= new List<T>();
                        added.Add(kv.Key);
                        _currentSet.Add(kv.Key);
                    }
                    else if (kv.Value == Flag.Present)
                    {
                        removed ??= new List<T>();
                        removed.Add(kv.Key);
                        _currentSet.Remove(kv.Key);
                    }
                }
                
                if (added == null && removed == null)
                {
                    return;
                }

                if (removed != null)
                {
                    foreach (var element in removed)
                    {
                        _elementMap.Remove(element);
                    }
                }
                
                ObservableSetPatch<T> patch = new(added ?? (IReadOnlyList<T>)Array.Empty<T>(), removed ?? (IReadOnlyList<T>)Array.Empty<T>());

                _patchStream.OnNext(patch);
            }
        }

        public void Append(IEnumerable<T> elements)
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
                
                List<T>? added = null;
                foreach (var element in elements)
                {
                    if (_currentSet.Add(element))
                    {
                        _elementMap[element] = Flag.Present;
                    }

                    added ??= new List<T>();
                    added.Add(element);
                }

                if (added != null)
                {
                    _patchStream.OnNext(new ObservableSetPatch<T>(added, Array.Empty<T>()));
                }
            }
        }
        
        public void Remove(IEnumerable<T> elements)
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
                
                List<T>? removed = null;
                foreach (var element in elements)
                {
                    if (_currentSet.Remove(element))
                    {
                        _elementMap.Remove(element);
                    }

                    removed ??= new List<T>();
                    removed.Add(element);
                }

                if (removed != null)
                {
                    _patchStream.OnNext(new ObservableSetPatch<T>(Array.Empty<T>(), removed));
                }
            }
        }

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
            Preserved = Present | Added
        }
    }
}