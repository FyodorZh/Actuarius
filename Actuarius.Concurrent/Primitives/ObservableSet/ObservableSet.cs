using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Actuarius.Concurrent
{   
    public class ObservableSet<TData>: IObservableSet<TData>, Collections.ISet<TData>
        where TData: notnull
    {
        private readonly Subject<ObservableSetPatch<TData>> _patchStream = new();

        private readonly HashSet<TData> _currentSet;
        private readonly Dictionary<TData, Flag> _elementMap;

        private readonly object _lock = new();

        private bool _completed = false;

        public ObservableSet(IEqualityComparer<TData>? comparer = null)
        {
            _currentSet = new(comparer);
            _elementMap = new Dictionary<TData, Flag>(comparer);
        }
        
        public IDisposable Subscribe(IObserver<ObservableSetPatch<TData>> observer)
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
                    var delta = new ObservableSetPatch<TData>(_currentSet.ToArray(), Array.Empty<TData>());
                    observer.OnNext(delta);
                }

                return _patchStream.Subscribe(observer);    
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                if (_completed)
                {
                    return;
                }

                ObservableSetPatch<TData> patch = new ObservableSetPatch<TData>(Array.Empty<TData>(), _currentSet.ToArray());
                
                _currentSet.Clear();
                _elementMap.Clear();
                
                _patchStream.OnNext(patch);
            }
        }

        public void Set(IEnumerable<TData> elements)
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

                foreach (TData element in elements)
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

                List<TData>? added = null;
                List<TData>? removed = null;

                foreach (var kv in _elementMap)
                {
                    if (kv.Value == Flag.Added)
                    {
                        added ??= new List<TData>();
                        added.Add(kv.Key);
                        _currentSet.Add(kv.Key);
                    }
                    else if (kv.Value == Flag.Present)
                    {
                        removed ??= new List<TData>();
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
                
                ObservableSetPatch<TData> patch = new(added ?? (IReadOnlyList<TData>)Array.Empty<TData>(), removed ?? (IReadOnlyList<TData>)Array.Empty<TData>());

                _patchStream.OnNext(patch);
            }
        }

        public void Append(IEnumerable<TData> elements)
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
                
                List<TData>? added = null;
                foreach (var element in elements)
                {
                    if (_currentSet.Add(element))
                    {
                        _elementMap[element] = Flag.Present;
                    }

                    added ??= new List<TData>();
                    added.Add(element);
                }

                if (added != null)
                {
                    _patchStream.OnNext(new ObservableSetPatch<TData>(added, Array.Empty<TData>()));
                }
            }
        }
        
        public void Remove(IEnumerable<TData> elements)
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
                
                List<TData>? removed = null;
                foreach (var element in elements)
                {
                    if (_currentSet.Remove(element))
                    {
                        _elementMap.Remove(element);
                    }

                    removed ??= new List<TData>();
                    removed.Add(element);
                }

                if (removed != null)
                {
                    _patchStream.OnNext(new ObservableSetPatch<TData>(Array.Empty<TData>(), removed));
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
        
        public TData[] ToArray()
        {
            lock (_lock)
            {
                return _currentSet.ToArray();
            }
        }

        public bool Put(TData value)
        {
            lock (_lock)
            {
                if (_completed)
                {
                    return false;
                }

                if (_currentSet.Add(value))
                {
                    _elementMap[value] = Flag.Present;
                    _patchStream.OnNext(new ObservableSetPatch<TData>(new[] { value }, Array.Empty<TData>()));
                    return true;
                }

                return false;
            }
        }

        public bool Remove(TData element)
        {
            lock (_lock)
            {
                if (_completed)
                {
                    return false;
                }

                if (_currentSet.Remove(element))
                {
                    _elementMap.Remove(element);
                    _patchStream.OnNext(new ObservableSetPatch<TData>(Array.Empty<TData>(), new[] { element }));
                    return true;
                }

                return false;
            }
        }

        public bool Contains(TData element)
        {
            lock (_lock)
            {
                return _currentSet.Contains(element);
            }
        }
    }
}