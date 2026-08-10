{
  "library": "Actuarius.Concurrent",
  "meta": {
    "target": "netstandard2.1",
    "lang": 9,
    "nullable": true
  },
  "philosophy": "A thread-safe concurrency toolkit for observable reactive state and serialized coordination. ObservableField, ObservableSet and ObservableDictionary push diff patches to subscribers and signal terminal or rejected states by returning false or a result enum instead of throwing. Coordination helpers (ConcurrentSerializedExecutor, Intention, ConcurrentManualLockGate, AtomicBox) are lock-free or lock-based primitives with explicit lifecycles, and valve classes add open/close gate semantics over Actuarius.Collections concurrent collections. All types live in namespace Actuarius.Concurrent and depend on Actuarius.Collections and System.Reactive.",
  "quickPick": [
    {
      "need": "Apply a dictionary diff patch to an in-memory dictionary",
      "use": "ObservableDictionaryPatch<TKey, TValue>"
    },
    {
      "need": "Apply a set diff patch to an in-memory set",
      "use": "ObservableSetPatch<T>"
    },
    {
      "need": "Atomically read-modify-write a value from any thread",
      "use": "AtomicBox<T>"
    },
    {
      "need": "Batch concurrent work into one serialized run",
      "use": "ConcurrentSerializedExecutor"
    },
    {
      "need": "Close an input collection and drain what remains",
      "use": "ConcurrentQueueValve<TData>"
    },
    {
      "need": "Compare objects by an extracted key",
      "use": "KeyEqualityComparer<T, TKey>"
    },
    {
      "need": "Compare objects by an extracted key using IEquatable strictness",
      "use": "StrictKeyEqualityComparer<T, TKey>"
    },
    {
      "need": "Convert an observable dictionary to another key and value type",
      "use": "IObservableDictionary<TKey, TData>"
    },
    {
      "need": "Convert an observable set to another element type",
      "use": "IObservableSet<TData>"
    },
    {
      "need": "Coordinate a one-shot delayed action across threads",
      "use": "Intention"
    },
    {
      "need": "Observe a dictionary as add/update/remove patches",
      "use": "ObservableDictionary<TKey, TData>"
    },
    {
      "need": "Observe a set as add/remove patches",
      "use": "ObservableSet<TData>"
    },
    {
      "need": "Observe a single mutable value",
      "use": "ObservableField<T>"
    },
    {
      "need": "Signal finalization once all active critical sections exit",
      "use": "ConcurrentManualLockGate"
    },
    {
      "need": "Wait asynchronously until a value satisfies a predicate",
      "use": "IObservableField<T>"
    }
  ],
  "decisionTree": [
    [
      "What do you want to make observable?",
      "A single value",
      "ObservableField<T>"
    ],
    [
      "What do you want to make observable?",
      "A set",
      "ObservableSet<TData>"
    ],
    [
      "What do you want to make observable?",
      "A key-value dictionary",
      "ObservableDictionary<TKey, TData>"
    ],
    [
      "Do you need to close an input collection and drain what remains?",
      "Yes",
      [
        [
          "Which concurrency contract does the underlying collection implement?",
          "Fully concurrent queue",
          "ConcurrentQueueValve<TData>"
        ],
        [
          "Which concurrency contract does the underlying collection implement?",
          "Fully concurrent unordered bag",
          "ConcurrentUnorderedCollectionValve<TData>"
        ],
        [
          "Which concurrency contract does the underlying collection implement?",
          "Single-reader/single-writer queue",
          "SingleReaderWriterConcurrentQueueValve<TData>"
        ],
        [
          "Which concurrency contract does the underlying collection implement?",
          "Single-reader/single-writer unordered bag",
          "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>"
        ]
      ]
    ],
    [
      "Do you need to serialize concurrent invocations of one action?",
      "Yes",
      "ConcurrentSerializedExecutor"
    ],
    [
      "Do you need a one-shot cross-thread coordination state machine?",
      "Yes",
      "Intention"
    ],
    [
      "Do you need a thread-safe box for a value?",
      "Yes",
      "AtomicBox<T>"
    ],
    [
      "Do you need to block new sections after a finalization signal?",
      "Yes",
      "ConcurrentManualLockGate"
    ],
    [
      "Do you need equality and hashing by an extracted key?",
      "Yes",
      [
        [
          "Is TKey IEquatable<TKey>?",
          "No",
          "KeyEqualityComparer<T, TKey>"
        ],
        [
          "Is TKey IEquatable<TKey>?",
          "Yes",
          "StrictKeyEqualityComparer<T, TKey>"
        ]
      ]
    ],
    [
      "Do you want to project an observable to a different element type?",
      "Yes",
      [
        [
          "Which projection?",
          "Dictionary keys and values",
          "ObservableDictionary_Converter<TKey, TData, TKeyFrom, TDataFrom>"
        ],
        [
          "Which projection?",
          "Dictionary values only",
          "ObservableDictionary_Converter_Data<TKey, TFromData, TData>"
        ],
        [
          "Which projection?",
          "Set elements",
          "ObservableSet_Converter<From, To>"
        ]
      ]
    ]
  ],
  "interfaces": [
    {
      "name": "IObservableDictionary<TKey, TData>",
      "extends": [
        "IObservable<ObservableDictionaryPatch<TKey, TData>>"
      ],
      "genericConstraints": "where TKey : notnull",
      "desc": "observable dictionary that emits an ObservableDictionaryPatch<TKey, TData> on every mutation; Subscribe is inherited from IObservable<ObservableDictionaryPatch<TKey, TData>>",
      "members": [
        {
          "sig": "ConvertDataTo<TNewData>(Func<TData, TNewData> converter):IObservableDictionary<TKey, TNewData>",
          "desc": "returns a live projection of this dictionary that converts values to TNewData in every emitted patch; a default interface method",
          "threadSafety": "not thread-safe",
          "postconditions": [
            "the returned view stays in sync with the source dictionary"
          ],
          "example": "IObservableDictionary<string, string> view = dict.ConvertDataTo(v => v.ToString());"
        },
        {
          "sig": "ConvertTo<TNewKey, TNewData>(Func<TKey, TNewKey> keyConverter, Func<TData, TNewData> dataConverter):IObservableDictionary<TNewKey, TNewData> where TNewKey : notnull",
          "desc": "returns a live projection of this dictionary that converts keys and values in every emitted patch; a default interface method",
          "threadSafety": "not thread-safe",
          "postconditions": [
            "the returned view stays in sync with the source dictionary"
          ],
          "remarks": "converted keys can collide; the adapter does not de-duplicate the converted patches"
        }
      ]
    },
    {
      "name": "IObservableField<out T>",
      "variance": "out T",
      "extends": [
        "IObservable<T>"
      ],
      "desc": "observable field that pushes value changes to subscribers; Subscribe is inherited from IObservable<T>",
      "members": [
        {
          "sig": "T Value { get; }",
          "desc": "gets the current value",
          "threadSafety": "thread-safe (lock)",
          "example": "int current = field.Value;"
        },
        {
          "sig": "Task<bool> WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default)",
          "desc": "returns a task that completes true when the current or a later value satisfies predicate, false if the field completes first, or canceled via cancellationToken",
          "threadSafety": "thread-safe (lock)",
          "example": "bool satisfied = await field.WaitFor(v => v > 100);"
        }
      ]
    },
    {
      "name": "IObservableSet<TData>",
      "extends": [
        "IObservable<ObservableSetPatch<TData>>"
      ],
      "desc": "observable set that emits an ObservableSetPatch<TData> on every mutation; Subscribe is inherited from IObservable<ObservableSetPatch<TData>>",
      "members": [
        {
          "sig": "ConvertTo<TNew>(Func<TData, TNew> converter):IObservableSet<TNew>",
          "desc": "returns a live projection of this set that converts elements to TNew in every emitted patch; a default interface method",
          "threadSafety": "not thread-safe",
          "postconditions": [
            "the returned view stays in sync with the source set"
          ],
          "example": "IObservableSet<int> lengths = set.ConvertTo(x => x.Length);"
        }
      ]
    }
  ],
  "types": [
    {
      "name": "AtomicBox<T>",
      "kind": "class",
      "category": "utility",
      "genericConstraints": "where T : struct",
      "isAuxiliary": false,
      "desc": "thread-safe box holding a single struct value, protected by a ReaderWriterLockSlim",
      "threadSafety": "thread-safe (lock)",
      "limitations": "T must be a struct; for reference-typed state use ObservableField<T> or external locking",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a box holding default(T)",
          "example": "var box = new AtomicBox<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Value:T { get; set; }",
          "desc": "gets or sets the stored value; reads take a read lock and writes take a write lock; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "box.Value = 5; int v = box.Value;"
        }
      ],
      "methods": [
        {
          "sig": "Update(Func<T, T> update)",
          "desc": "applies update to the current value inside a write lock so the read-modify-write is atomic",
          "threadSafety": "thread-safe (lock)",
          "preconditions": [
            "update is not null"
          ],
          "example": "box.Update(x => x + 1);",
          "remarks": "update must not call back into the same box; ReaderWriterLockSlim is not reentrant and would throw a LockRecursionException"
        }
      ]
    },
    {
      "name": "ConcurrentManualLockGate",
      "kind": "class",
      "category": "gate",
      "isAuxiliary": false,
      "desc": "lock-free gate tracking open/closed state and the number of active critical sections; when the gate is closed and the last active section exits, the onClose callback fires exactly once",
      "threadSafety": "lock-free",
      "limitations": "onClose runs on the thread performing the final Exit, so it must not assume a particular thread; do not call TryClose from inside onClose",
      "constructors": [
        {
          "sig": "ctor(Action onClose)",
          "desc": "creates a gate that starts open and immediately enters the initial critical section, so onClose cannot fire until TryClose is called and the section count returns to 0",
          "preconditions": [
            "onClose is not null"
          ],
          "remarks": "the constructor's implicit section is released by TryClose"
        }
      ],
      "methods": [
        {
          "sig": "Enter():bool",
          "desc": "atomically increments the active section count and returns true while the gate is open; returns false without incrementing once the gate is closed; O(1)",
          "threadSafety": "lock-free",
          "example": "if (gate.Enter()) { try { } finally { gate.Exit(); } }"
        },
        {
          "sig": "Exit()",
          "desc": "decrements the active section count; when it reaches 0 the onClose callback is invoked exactly once; O(1)",
          "threadSafety": "lock-free",
          "preconditions": [
            "Exit is paired with a successful Enter or with the constructor's implicit section"
          ]
        },
        {
          "sig": "TryClose()",
          "desc": "atomically marks the gate closed so new Enter calls return false, then calls Exit; idempotent; the onClose callback fires when the section count reaches 0; O(1)",
          "threadSafety": "lock-free",
          "example": "gate.TryClose();"
        }
      ]
    },
    {
      "name": "ConcurrentQueueValve<TData>",
      "kind": "class",
      "category": "valve",
      "base": "ConcurrentUnorderedCollectionValve<TData>",
      "implements": [
        "IConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "fully concurrent queue valve wrapping an IConcurrentQueue<TData>; adds Count and inherits gate, drain and rejection behaviour from ConcurrentUnorderedCollectionValve<TData>",
      "threadSafety": "lock-free",
      "limitations": "use the SingleReaderWriter variant when only one writer and one reader exist; use the bare collection when no close/drain semantics are required",
      "constructors": [
        {
          "sig": "ctor(IConcurrentQueue<TData> collection, Action<TData> processor)",
          "desc": "creates a valve that routes both unfinished drain items and post-close rejected items through the same processor; holds reference only",
          "preconditions": [
            "collection is not null"
          ],
          "example": "var valve = new ConcurrentQueueValve<int>(new LimitedConcurrentQueue<int>(100), Process);"
        },
        {
          "sig": "ctor(IConcurrentQueue<TData> collection, Action<TData> unfinishedProcessor, Action<TData> rejectedProcessor)",
          "desc": "creates a valve with separate processors: unfinishedProcessor drains items still queued on close, rejectedProcessor receives items enqueued after the gate closed; holds reference only",
          "preconditions": [
            "collection is not null"
          ]
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements currently held by the underlying collection; O(1)"
        }
      ]
    },
    {
      "name": "ConcurrentSerializedExecutor",
      "kind": "class",
      "category": "executor",
      "isAuxiliary": false,
      "desc": "coalesces concurrent ScheduleOneInvokation calls into batches: only the caller that observes the 0-to-1 transition runs the wrapped action, executing it once per pending call while concurrent callers return immediately",
      "threadSafety": "lock-free",
      "limitations": "concurrent callers never wait for the batch, so work scheduled near the end of a batch can run after ScheduleOneInvokation returns; the action runs on whichever thread won the batch",
      "constructors": [
        {
          "sig": "ctor(Action action)",
          "desc": "creates an executor holding the action to invoke serially",
          "remarks": "no null check is performed; a null action throws NullReferenceException at the first invocation"
        }
      ],
      "methods": [
        {
          "sig": "ScheduleOneInvokation()",
          "desc": "increments the pending count; the caller that transitions 0 to 1 runs the action repeatedly until the pending count returns to 0; all other callers return immediately; exceptions thrown by the action are collected and rethrown as an AggregateException by the executing thread once the batch completes; O(1) per call",
          "threadSafety": "lock-free",
          "postconditions": [
            "the action is invoked exactly once per ScheduleOneInvokation call"
          ],
          "example": "exec.ScheduleOneInvokation();",
          "remarks": "the action is never invoked concurrently with itself but may run on any thread"
        }
      ]
    },
    {
      "name": "ConcurrentUnorderedCollectionValve<TData>",
      "kind": "class",
      "category": "valve",
      "base": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "implements": [
        "IConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": false,
      "desc": "fully concurrent unordered valve wrapping an IConcurrentUnorderedCollection<TData>; inherits gate, drain and rejection behaviour from SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(IConcurrentUnorderedCollection<TData> collection, Action<TData> processor)",
          "desc": "creates a valve that routes both unfinished drain items and post-close rejected items through the same processor",
          "preconditions": [
            "collection is not null"
          ]
        },
        {
          "sig": "ctor(IConcurrentUnorderedCollection<TData> collection, Action<TData> unfinishedProcessor, Action<TData> rejectedProcessor)",
          "desc": "creates a valve with separate processors: unfinishedProcessor drains items still held on close, rejectedProcessor receives items enqueued after the gate closed",
          "preconditions": [
            "collection is not null"
          ]
        }
      ]
    },
    {
      "name": "Intention",
      "kind": "class",
      "category": "state-machine",
      "isAuxiliary": false,
      "desc": "lock-free one-shot 3-state machine NoIntention to HasIntention to HadIntention for registering a deferred desire to perform a single action and realizing it exactly once",
      "threadSafety": "lock-free",
      "nested": [
        {
          "name": "State",
          "kind": "enum",
          "isAuxiliary": true,
          "desc": "NoIntention=0, HasIntention=1, HadIntention=2"
        }
      ],
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an intention in the NoIntention state",
          "example": "var intent = new Intention();"
        }
      ],
      "properties": [
        {
          "sig": "CurState:State { get; }",
          "desc": "returns the current state of the machine; O(1)",
          "threadSafety": "lock-free",
          "example": "Intention.State s = intent.CurState;"
        }
      ],
      "methods": [
        {
          "sig": "Set():bool",
          "desc": "atomically transitions NoIntention to HasIntention; returns true if the desire was registered, false if the machine already had an intention or is terminal; O(1)",
          "threadSafety": "lock-free",
          "example": "bool registered = intent.Set();"
        },
        {
          "sig": "SetAndRealize():bool",
          "desc": "calls Set then TryToRealize; returns true if the desire was realized; one-shot, so a later SetAndRealize returns false; O(1)",
          "threadSafety": "lock-free",
          "example": "bool done = intent.SetAndRealize();"
        },
        {
          "sig": "TryToRealize():bool",
          "desc": "atomically transitions HasIntention to HadIntention; returns true if realized, false if there was no pending desire; O(1)",
          "threadSafety": "lock-free",
          "example": "bool realized = intent.TryToRealize();"
        }
      ]
    },
    {
      "name": "InvariantChecker<TState>",
      "kind": "class",
      "category": "utility",
      "genericConstraints": "where TState : struct",
      "isAuxiliary": false,
      "desc": "abstract base that validates state-machine invariants; every member is protected and the class is intended to be subclassed, never used directly",
      "threadSafety": "lock-free",
      "limitations": "all members are protected; public code only derives from it and must implement ToState and FromState",
      "constructors": [
        {
          "sig": "ctor(TState state, Action<string> onFail)",
          "desc": "protected; initialises the internal state to state and stores the onFail reporter; calls the virtual FromState from the constructor, so the override must not depend on instance fields set by the subclass",
          "preconditions": [
            "onFail is not null"
          ],
          "remarks": "the abstract FromState is invoked during construction, before any subclass constructor body runs"
        }
      ],
      "properties": [
        {
          "sig": "State:TState { get; }",
          "desc": "protected; returns the current state decoded from the internal interlocked int",
          "threadSafety": "lock-free"
        }
      ],
      "methods": [
        {
          "sig": "BeginCriticalSection(ref int sectionId)",
          "desc": "protected; increments sectionId via Interlocked.Increment and reports a failure via Fail if the depth is not exactly 1, detecting nested or unbalanced entry",
          "threadSafety": "lock-free"
        },
        {
          "sig": "ChangeState(TState expected, TState newState)",
          "desc": "protected; atomically swaps the state to newState and reports StateFail(expected, previous) if the previous state was not expected",
          "threadSafety": "lock-free"
        },
        {
          "sig": "CheckState(TState expected)",
          "desc": "protected; reports StateFail(expected, current) if the current state is not expected",
          "threadSafety": "lock-free"
        },
        {
          "sig": "EndCriticalSection(ref int sectionId)",
          "desc": "protected; decrements sectionId and reports Fail(\"wrong nesting\") if the depth does not reach exactly 0",
          "threadSafety": "lock-free"
        },
        {
          "sig": "Fail(string error = \"\")",
          "desc": "protected; reports an invariant violation through onFail with a message naming the runtime type, ToString() and error",
          "threadSafety": "lock-free"
        },
        {
          "sig": "FromState(TState state):int",
          "desc": "protected abstract; converts a TState value to the internal int representation; called from the constructor",
          "threadSafety": "lock-free"
        },
        {
          "sig": "SetState(TState newState):TState",
          "desc": "protected; atomically replaces the state with newState via Interlocked.Exchange and returns the previous state",
          "threadSafety": "lock-free"
        },
        {
          "sig": "StateFail(TState expected, TState received)",
          "desc": "protected; reports Fail(\"required {expected}, but found {received}\")",
          "threadSafety": "lock-free"
        },
        {
          "sig": "ToState(int state):TState",
          "desc": "protected abstract; converts the internal int representation to a TState value",
          "threadSafety": "lock-free"
        }
      ]
    },
    {
      "name": "KeyEqualityComparer<T, TKey>",
      "kind": "class",
      "category": "comparer",
      "implements": [
        "IEqualityComparer<T>"
      ],
      "genericConstraints": "where TKey : notnull",
      "isAuxiliary": false,
      "desc": "IEqualityComparer<T> that compares and hashes objects by an extracted key of type TKey; equality is key-based, not reference-based",
      "threadSafety": "not thread-safe",
      "limitations": "two distinct objects with equal keys compare equal and hash the same; prefer StrictKeyEqualityComparer when TKey is IEquatable<TKey> to avoid boxing in Equals",
      "constructors": [
        {
          "sig": "ctor(Func<T, TKey> keyExtractor)",
          "desc": "stores the key extractor used for equality and hashing; holds reference only",
          "preconditions": [
            "keyExtractor is not null"
          ],
          "example": "var comparer = new KeyEqualityComparer<Person, int>(p => p.Id);"
        }
      ],
      "methods": [
        {
          "sig": "Equals(T? x, T? y):bool",
          "desc": "returns true if both x and y are null, false if exactly one is null, and otherwise compares the extracted keys via key1.Equals(key2); virtual; O(1)",
          "example": "bool eq = comparer.Equals(a, b);"
        },
        {
          "sig": "GetHashCode(T obj):int",
          "desc": "returns the hash code of the extracted key of obj; O(1)",
          "example": "int h = comparer.GetHashCode(p);"
        }
      ]
    },
    {
      "name": "ObservableDictionary<TKey, TData>",
      "kind": "class",
      "category": "observable-map",
      "implements": [
        "IObservableDictionary<TKey, TData>",
        "IMap<TKey, TData>"
      ],
      "genericConstraints": "where TKey : notnull",
      "isAuxiliary": false,
      "desc": "thread-safe observable dictionary that emits an ObservableDictionaryPatch<TKey, TData> diff (added, updated, removed) on every mutation",
      "threadSafety": "thread-safe (lock)",
      "limitations": "patches are emitted synchronously on the mutating thread while holding the lock, so slow observers stall mutators; Set throws on duplicate keys in the input; use IMap members when callers only need single-key access",
      "constructors": [
        {
          "sig": "ctor(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TData>? dataComparer = null)",
          "desc": "creates an empty dictionary; dataComparer decides whether Set treats a value as changed; nulls fall back to EqualityComparer<T>.Default; holds reference only",
          "example": "var dict = new ObservableDictionary<string, int>(StringComparer.OrdinalIgnoreCase);"
        }
      ],
      "methods": [
        {
          "sig": "Add(TKey key, TData element):bool",
          "desc": "adds the entry, or updates the value if the key already exists, and emits an added or updated patch; returns false only when the dictionary is completed; O(1) amortised",
          "threadSafety": "thread-safe (lock)",
          "example": "dict.Add(\"A\", 1);"
        },
        {
          "sig": "AddOrUpdate(IEnumerable<KeyValuePair<TKey, TData>> elements)",
          "desc": "upserts every element: existing keys are updated, missing keys are added; emits a patch with the added and updated entries, always reporting existing keys as updated even when the value is equal; never removes; throws ArgumentNullException if elements is null; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "dict.AddOrUpdate(new[] { new KeyValuePair<string, int>(\"A\", 2) });"
        },
        {
          "sig": "Clear()",
          "desc": "removes all entries and emits a patch listing every removed key; does nothing if completed; O(n)",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "Complete()",
          "desc": "marks the dictionary terminal and raises OnCompleted on the patch stream; later mutations are silently ignored; O(n) in subscribers",
          "threadSafety": "thread-safe (lock)",
          "example": "dict.Complete();"
        },
        {
          "sig": "Remove(TKey key):bool",
          "desc": "removes the entry for key and emits a patch; returns false if the key is absent or the dictionary is completed; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "bool removed = dict.Remove(\"A\");"
        },
        {
          "sig": "RemoveMany(IEnumerable<TKey> keys)",
          "desc": "removes every present key and emits a single patch listing the removed keys; throws ArgumentNullException if keys is null; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "dict.RemoveMany(new[] { \"A\", \"B\" });"
        },
        {
          "sig": "Set(IEnumerable<KeyValuePair<TKey, TData>> elements)",
          "desc": "replaces the contents with exactly elements, diffing against the current state and emitting added, updated and removed lists; throws ArgumentNullException if elements is null; throws InvalidOperationException if a key occurs more than once in the input; emits no patch when the contents are identical; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "dict.Set(new[] { new KeyValuePair<string, int>(\"B\", 20) });"
        },
        {
          "sig": "Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observer):IDisposable",
          "desc": "subscribes to the patch stream; throws ArgumentNullException if observer is null; if completed, calls OnCompleted immediately and returns Disposable.Empty; otherwise, if non-empty, first emits a patch with all current entries as added; O(n) in entries",
          "threadSafety": "thread-safe (lock)",
          "example": "var sub = dict.Subscribe(p => p.ApplyTo(target));"
        },
        {
          "sig": "ToArray():KeyValuePair<TKey, TData>[]",
          "desc": "returns a snapshot of all entries; caller takes ownership; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "var snapshot = dict.ToArray();"
        },
        {
          "sig": "TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element):bool",
          "desc": "retrieves the value for key; returns false and sets element to default if the key is absent; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "if (dict.TryGetValue(\"A\", out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "ObservableDictionaryPatch<TKey, TValue>",
      "kind": "struct",
      "category": "diff",
      "isAuxiliary": false,
      "desc": "readonly diff describing added, updated and removed entries of an observable dictionary",
      "constructors": [
        {
          "sig": "ctor(IReadOnlyList<KeyValuePair<TKey, TValue>> added, IReadOnlyList<KeyValuePair<TKey, TValue>> updated, IReadOnlyList<TKey> removed)",
          "desc": "stores the three diff lists; holds reference only",
          "remarks": "the lists are not copied; mutate them only before the patch is shared"
        }
      ],
      "properties": [
        {
          "sig": "Added:IReadOnlyList<KeyValuePair<TKey, TValue>> { get; }",
          "desc": "returns the entries added since the last patch; holds reference only"
        },
        {
          "sig": "Removed:IReadOnlyList<TKey> { get; }",
          "desc": "returns the keys removed since the last patch; holds reference only"
        },
        {
          "sig": "Updated:IReadOnlyList<KeyValuePair<TKey, TValue>> { get; }",
          "desc": "returns the entries whose value changed since the last patch; holds reference only"
        }
      ],
      "methods": [
        {
          "sig": "ApplyTo(IDictionary<TKey, TValue> dictionary)",
          "desc": "applies the patch to dictionary by removing the Removed keys, then writing the Added entries, then the Updated entries; throws ArgumentNullException if dictionary is null; O(n)",
          "example": "patch.ApplyTo(target);"
        }
      ]
    },
    {
      "name": "ObservableDictionary_Converter<TKey, TData, TKeyFrom, TDataFrom>",
      "kind": "class",
      "category": "adapter",
      "implements": [
        "IObservableDictionary<TKey, TData>"
      ],
      "genericConstraints": "where TKey : notnull and where TKeyFrom : notnull",
      "isAuxiliary": false,
      "desc": "adapts an IObservableDictionary<TKeyFrom, TDataFrom> into an IObservableDictionary<TKey, TData> by converting keys and values in every emitted patch; normally obtained from IObservableDictionary.ConvertTo",
      "threadSafety": "not thread-safe",
      "limitations": "key conversion can collide; the adapter does not de-duplicate converted patches",
      "constructors": [
        {
          "sig": "ctor(IObservableDictionary<TKeyFrom, TDataFrom> core, Func<TKeyFrom, TKey> keyConverter, Func<TDataFrom, TData> dataConverter)",
          "desc": "stores the core stream and both converters, applied lazily per patch; holds reference only",
          "preconditions": [
            "core, keyConverter and dataConverter are not null"
          ]
        }
      ],
      "methods": [
        {
          "sig": "Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observable):IDisposable",
          "desc": "forwards converted patches from core to observable; the returned disposable unsubscribes from core; O(n) per patch",
          "example": "var sub = conv.Subscribe(o => Console.WriteLine(o.Added.Count));"
        }
      ]
    },
    {
      "name": "ObservableDictionary_Converter_Data<TKey, TFromData, TData>",
      "kind": "class",
      "category": "adapter",
      "implements": [
        "IObservableDictionary<TKey, TData>"
      ],
      "genericConstraints": "where TKey : notnull",
      "isAuxiliary": false,
      "desc": "adapts an IObservableDictionary<TKey, TFromData> into an IObservableDictionary<TKey, TData> by converting only values; normally obtained from IObservableDictionary.ConvertDataTo",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IObservableDictionary<TKey, TFromData> core, Func<TFromData, TData> converter)",
          "desc": "stores the core stream and the value converter, applied lazily per patch; holds reference only",
          "preconditions": [
            "core and converter are not null"
          ]
        }
      ],
      "methods": [
        {
          "sig": "Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observable):IDisposable",
          "desc": "forwards value-converted patches from core to observable; the returned disposable unsubscribes from core; O(n) per patch",
          "example": "var sub = conv.Subscribe(o => Console.WriteLine(o.Added.Count));"
        }
      ]
    },
    {
      "name": "ObservableField<T>",
      "kind": "class",
      "category": "observable-value",
      "implements": [
        "IObservableField<T>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe reactive wrapper around a single value; pushes every changed value to subscribers and supports awaiting predicates on the value",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Value setter always uses checkEquality=true, so assigning an equal value emits nothing; use SetValue with checkEquality=false to force notification",
      "constructors": [
        {
          "sig": "ctor(T value)",
          "desc": "creates a field holding the initial value; subscribers receive it immediately",
          "example": "var field = new ObservableField<int>(0);"
        }
      ],
      "operators": [
        {
          "sig": "implicit operator T(ObservableField<T> field):T",
          "desc": "implicitly converts the field to its current value, equivalent to reading Value; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "int x = field;"
        }
      ],
      "properties": [
        {
          "sig": "Value:T { get; set; }",
          "desc": "gets the current value, or sets it via SetValue(value) with checkEquality=true; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "field.Value = 42;"
        }
      ],
      "methods": [
        {
          "sig": "Complete()",
          "desc": "marks the field terminal: blocks further SetValue mutations, raises OnCompleted on subscribers, and resolves every pending WaitFor with false (or canceled if its token was already canceled); O(n) in waiters",
          "threadSafety": "thread-safe (lock)",
          "example": "field.Complete();"
        },
        {
          "sig": "SetValue(T value, bool checkEquality = true)",
          "desc": "sets the value and pushes it to subscribers; with checkEquality=true and the new value Equals the current one, emits nothing; silently does nothing when the field is completed; O(n) in waiters",
          "threadSafety": "thread-safe (lock)",
          "example": "field.SetValue(10, checkEquality: false);"
        },
        {
          "sig": "Subscribe(IObserver<T> observer):IDisposable",
          "desc": "subscribes the observer; if the field is completed, immediately calls OnNext with the current value then OnCompleted and returns Disposable.Empty; otherwise the underlying BehaviorSubject immediately pushes the current value",
          "threadSafety": "thread-safe (lock)",
          "example": "var sub = field.Subscribe(v => Console.WriteLine(v));"
        },
        {
          "sig": "ToString():string?",
          "desc": "returns the string representation of the current value, or null if the value is null",
          "example": "string? s = field.ToString();"
        },
        {
          "sig": "WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default):Task<bool>",
          "desc": "returns a task that completes true when the current or a later value satisfies predicate, false if the field completes first, or canceled via cancellationToken; a predicate already satisfied returns an immediately completed task; O(1) plus O(n) in waiters on each value change",
          "threadSafety": "thread-safe (lock)",
          "example": "bool ok = await field.WaitFor(v => v > 100);"
        }
      ]
    },
    {
      "name": "ObservableSet<TData>",
      "kind": "class",
      "category": "observable-set",
      "implements": [
        "IObservableSet<TData>",
        "Collections.ISet<TData>"
      ],
      "genericConstraints": "where TData : notnull",
      "isAuxiliary": false,
      "desc": "thread-safe observable set that emits an ObservableSetPatch<TData> diff (added, removed) on every mutation",
      "threadSafety": "thread-safe (lock)",
      "limitations": "patches are emitted synchronously on the mutating thread while holding the lock, so slow observers stall mutators; Append and Remove list every input element in the patch even when nothing actually changed; Put is the precise single-element API",
      "constructors": [
        {
          "sig": "ctor(IEqualityComparer<TData>? comparer = null)",
          "desc": "creates an empty set; a null comparer falls back to EqualityComparer<TData>.Default; holds reference only",
          "example": "var set = new ObservableSet<string>();"
        }
      ],
      "methods": [
        {
          "sig": "Append(IEnumerable<TData> elements)",
          "desc": "adds all elements and emits a patch listing every input element as added, even ones already present; throws ArgumentNullException if elements is null; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "set.Append(new[] { \"A\", \"B\" });"
        },
        {
          "sig": "Clear()",
          "desc": "removes all elements and emits a patch listing every removed element; does nothing if completed; O(n)",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "Complete()",
          "desc": "marks the set terminal and raises OnCompleted on the patch stream; later mutations are silently ignored; O(n) in subscribers",
          "threadSafety": "thread-safe (lock)",
          "example": "set.Complete();"
        },
        {
          "sig": "Contains(TData element):bool",
          "desc": "returns true if element is present in the set; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "bool has = set.Contains(\"A\");"
        },
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value and emits a patch; returns false if the value is already present or the set is completed; O(1) amortised",
          "threadSafety": "thread-safe (lock)",
          "example": "if (!set.Put(\"A\")) Console.WriteLine(\"already present\");"
        },
        {
          "sig": "Remove(IEnumerable<TData> elements)",
          "desc": "removes all listed elements and emits a patch listing every input element as removed, even absent ones; throws ArgumentNullException if elements is null; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "set.Remove(new[] { \"A\" });"
        },
        {
          "sig": "Remove(TData element):bool",
          "desc": "removes element and emits a patch; returns false if the element is absent or the set is completed; O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "bool removed = set.Remove(\"A\");"
        },
        {
          "sig": "Set(IEnumerable<TData> elements)",
          "desc": "replaces the contents with exactly elements, diffing against the current state and emitting added and removed lists; throws ArgumentNullException if elements is null; throws InvalidOperationException if an element occurs more than once in the input; emits no patch when the contents are identical; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "set.Set(new[] { \"A\", \"D\" });"
        },
        {
          "sig": "Subscribe(IObserver<ObservableSetPatch<TData>> observer):IDisposable",
          "desc": "subscribes to the patch stream; throws ArgumentNullException if observer is null; if completed, calls OnCompleted immediately and returns Disposable.Empty; otherwise, if non-empty, first emits a patch with all current elements as added; O(n) in elements",
          "threadSafety": "thread-safe (lock)",
          "example": "var sub = set.Subscribe(p => p.ApplyTo(local));"
        },
        {
          "sig": "ToArray():TData[]",
          "desc": "returns a snapshot of all elements; caller takes ownership; O(n)",
          "threadSafety": "thread-safe (lock)",
          "example": "var snapshot = set.ToArray();"
        }
      ]
    },
    {
      "name": "ObservableSetPatch<T>",
      "kind": "struct",
      "category": "diff",
      "isAuxiliary": false,
      "desc": "readonly diff describing added and removed elements of an observable set",
      "constructors": [
        {
          "sig": "ctor(IReadOnlyList<T> added, IReadOnlyList<T> removed)",
          "desc": "stores the added and removed lists; holds reference only",
          "remarks": "the lists are not copied; mutate them only before the patch is shared"
        }
      ],
      "properties": [
        {
          "sig": "Added:IReadOnlyList<T> { get; }",
          "desc": "returns the elements added since the last patch; holds reference only"
        },
        {
          "sig": "Removed:IReadOnlyList<T> { get; }",
          "desc": "returns the elements removed since the last patch; holds reference only"
        }
      ],
      "methods": [
        {
          "sig": "ApplyTo(ISet<T> set)",
          "desc": "applies the patch to set by removing the Removed elements, then adding the Added elements; throws ArgumentNullException if set is null; O(n)",
          "example": "patch.ApplyTo(target);"
        }
      ]
    },
    {
      "name": "ObservableSet_Converter<From, To>",
      "kind": "class",
      "category": "adapter",
      "implements": [
        "IObservableSet<To>"
      ],
      "isAuxiliary": false,
      "desc": "adapts an IObservableSet<From> into an IObservableSet<To> by converting every element in each emitted patch; normally obtained from IObservableSet.ConvertTo",
      "threadSafety": "not thread-safe",
      "limitations": "converted elements can collide if the converter is not injective; the adapter does not de-duplicate converted patches",
      "constructors": [
        {
          "sig": "ctor(IObservableSet<From> core, Func<From, To> converter)",
          "desc": "stores the core stream and the converter, applied lazily per patch; holds reference only",
          "preconditions": [
            "core and converter are not null"
          ]
        }
      ],
      "methods": [
        {
          "sig": "Subscribe(IObserver<ObservableSetPatch<To>> observable):IDisposable",
          "desc": "forwards converted patches from core to observable; the returned disposable unsubscribes from core; O(n) per patch",
          "example": "var sub = conv.Subscribe(o => Console.WriteLine(o.Added.Count));"
        }
      ]
    },
    {
      "name": "SingleReaderWriterConcurrentQueueValve<TData>",
      "kind": "class",
      "category": "valve",
      "base": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "implements": [
        "ISingleReaderWriterConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "single-reader/single-writer queue valve wrapping an ISingleReaderWriterConcurrentQueue<TData>; adds Count and inherits gate, drain and rejection behaviour from SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(ISingleReaderWriterConcurrentQueue<TData> collection, Action<TData> processor)",
          "desc": "creates a valve that routes both unfinished drain items and post-close rejected items through the same processor",
          "preconditions": [
            "collection is not null"
          ]
        },
        {
          "sig": "ctor(ISingleReaderWriterConcurrentQueue<TData> collection, Action<TData> unfinishedProcessor, Action<TData> rejectedProcessor)",
          "desc": "creates a valve with separate processors: unfinishedProcessor drains items still queued on close, rejectedProcessor receives items enqueued after the gate closed",
          "preconditions": [
            "collection is not null"
          ]
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements currently held by the underlying collection; O(1)"
        }
      ]
    },
    {
      "name": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "kind": "class",
      "category": "valve",
      "implements": [
        "ISingleReaderWriterConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": false,
      "desc": "wraps an input collection with gate semantics: while open, EnqueueEx and Put forward into the collection; CloseValve closes the gate, drains the remaining items through unfinishedProcessor once all active sections exit, and routes post-close items to rejectedProcessor",
      "threadSafety": "lock-free",
      "limitations": "Put reports true for rejected items, so use EnqueueEx when the Ok/Overflown/Rejected distinction matters; the drain consumes the collection via TryPop, so this must be the only consumer of the wrapped collection",
      "constructors": [
        {
          "sig": "ctor(ISingleReaderWriterConcurrentUnorderedCollection<TData> collection, Action<TData> processor)",
          "desc": "creates a valve that routes both unfinished drain items and post-close rejected items through the same processor; holds reference only",
          "preconditions": [
            "collection is not null"
          ],
          "example": "var valve = new SingleReaderWriterConcurrentUnorderedCollectionValve<int>(inner, Process);"
        },
        {
          "sig": "ctor(ISingleReaderWriterConcurrentUnorderedCollection<TData> collection, Action<TData> unfinishedProcessor, Action<TData> rejectedProcessor)",
          "desc": "creates a valve with separate processors: unfinishedProcessor drains items still held on close, rejectedProcessor receives items enqueued after the gate closed; holds reference only",
          "preconditions": [
            "collection is not null"
          ]
        }
      ],
      "methods": [
        {
          "sig": "CloseValve()",
          "desc": "requests closure: new EnqueueEx and TryPop calls fail once the gate closes, and when every active section exits the remaining items are drained via TryPop and passed to unfinishedProcessor; O(1)",
          "threadSafety": "lock-free",
          "example": "valve.CloseValve();"
        },
        {
          "sig": "EnqueueEx(TData value):ValveEnqueueResult",
          "desc": "enqueues value while the gate is open; returns Ok when the underlying collection accepted it, Overflown when the underlying Put returned false, or Rejected when the gate is closed (the value is routed to rejectedProcessor); O(1)",
          "threadSafety": "lock-free",
          "example": "ValveEnqueueResult r = valve.EnqueueEx(42);"
        },
        {
          "sig": "Put(TData value):bool",
          "desc": "returns true when the value was enqueued (Ok) or routed to rejectedProcessor (Rejected); returns false only when the underlying collection overflowed; O(1)",
          "threadSafety": "lock-free",
          "remarks": "a rejected value still returns true because it was not lost"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData value):bool",
          "desc": "removes one element from the underlying collection through the gate; returns false and sets value to default if the gate is closed or the collection is empty; O(1)",
          "threadSafety": "lock-free",
          "example": "if (valve.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "StrictKeyEqualityComparer<T, TKey>",
      "kind": "class",
      "category": "comparer",
      "base": "KeyEqualityComparer<T, TKey>",
      "implements": [
        "IEqualityComparer<T>"
      ],
      "genericConstraints": "where TKey : IEquatable<TKey>",
      "isAuxiliary": false,
      "desc": "KeyEqualityComparer whose Equals resolves to the typed IEquatable<TKey>.Equals, avoiding the object.Equals boxing path for value-type keys",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(Func<T, TKey> keyExtractor)",
          "desc": "stores the key extractor; GetHashCode behaviour is inherited from KeyEqualityComparer",
          "preconditions": [
            "keyExtractor is not null"
          ],
          "example": "var comparer = new StrictKeyEqualityComparer<Person, int>(p => p.Id);"
        }
      ],
      "methods": [
        {
          "sig": "Equals(T? x, T? y):bool",
          "desc": "overrides the base: returns true if both x and y are null, false if exactly one is null, and otherwise compares the extracted keys via IEquatable<TKey>.Equals; O(1)",
          "example": "bool eq = comparer.Equals(a, b);"
        }
      ]
    },
    {
      "name": "ValveEnqueueResult",
      "kind": "enum",
      "isAuxiliary": true,
      "desc": "Ok=0, Overflown=1, Rejected=2"
    }
  ],
  "gotchas": [
    "ConcurrentManualLockGate invokes onClose on whichever thread performs the final Exit; do not assume a specific thread runs the drain.",
    "ConcurrentQueueValve.Put returns true for Rejected items: when the gate is closed the value is still routed to the rejected processor, so Put only distinguishes overflow; use EnqueueEx for the three-way Ok/Overflown/Rejected result.",
    "ConcurrentSerializedExecutor callers that do not win the 0-to-1 transition return immediately, so the action may run on another thread after ScheduleOneInvokation returns and exceptions may surface to a different thread.",
    "ConcurrentSerializedExecutor re-runs the action while pending calls arrive, so a caller that schedules work in a tight loop can hold the executing thread and starve other threads of CPU.",
    "ObservableDictionary and ObservableSet emit patches synchronously on the mutating thread while holding their lock, so a slow observer stalls every mutator.",
    "ObservableDictionary.Set and ObservableSet.Set throw InvalidOperationException when the input sequence itself contains a duplicate key or element, not when the target already contains it.",
    "ObservableField.WaitFor can complete with false when the field completes instead of waiting forever; always check the returned boolean.",
    "ObservableSet.Append and ObservableSet.Remove report every input element in the patch even when nothing actually changed, so duplicates or absent elements still appear in Added or Removed.",
    "ObservableSet.Put returns false for an already-present value, while Append always reports the element as added; they have different duplicate semantics.",
    "Patch structs hold references to the caller's lists, so do not mutate those lists after a patch is stored or applied.",
    "Subscribing to a completed ObservableField replays the last value followed by OnCompleted, so treat completion as a one-shot replay, not a guarantee of no further OnNext."
  ],
  "commonMistakes": [
    "Calling Complete() on an ObservableField, ObservableSet or ObservableDictionary and expecting later mutations to throw; they are silently ignored.",
    "Checking only the valve Put return value and assuming true means 'stored'; a true can also mean the item was rejected to the rejected processor after CloseValve.",
    "Expecting ObservableField.Value to notify on every assignment; the setter uses checkEquality=true, so assigning an equal value emits nothing.",
    "Forgetting that Valve TryPop returns false once the gate is closed, so the external consumer cannot drain the collection after CloseValve; the unfinished processor owns the drain.",
    "Passing the same IConcurrentQueue to a valve and also consuming it elsewhere; the valve's drain loop expects to be the only consumer.",
    "Treating ObservableDictionary.Add as add-only-if-absent; it overwrites the value when the key already exists and emits an update patch.",
    "Using KeyEqualityComparer with a non-IEquatable value-type key and relying on fast Equals; key1.Equals(key2) boxes, so prefer StrictKeyEqualityComparer when TKey is IEquatable<TKey>."
  ],
  "patterns": [
    {
      "goal": "Apply an observable dictionary patch to a plain dictionary",
      "code": "var target = new Dictionary<string, int>(); var sub = dict.Subscribe(p => p.ApplyTo(target)); sub.Dispose();"
    },
    {
      "goal": "Apply an observable set patch to a local HashSet",
      "code": "var local = new HashSet<string>(); var sub = set.Subscribe(p => p.ApplyTo(local)); sub.Dispose();"
    },
    {
      "goal": "Atomically read-modify-write an AtomicBox value",
      "code": "var box = new AtomicBox<int>(); box.Update(v => v + 1); int v = box.Value;"
    },
    {
      "goal": "Await a value threshold on an observable field",
      "code": "var field = new ObservableField<int>(0); bool ok = await field.WaitFor(v => v >= 10);"
    },
    {
      "goal": "Convert an observable set to a different element type",
      "code": "IObservableSet<int> lengths = new ObservableSet<string>().ConvertTo(x => x.Length);"
    },
    {
      "goal": "Create a valve with an overflow signal and handle it gracefully",
      "code": "var valve = new ConcurrentQueueValve<int>(new LimitedConcurrentQueue<int>(2), x => Console.WriteLine(x)); if (valve.EnqueueEx(1) == ValveEnqueueResult.Overflown) Console.WriteLine(\"dropped\");"
    },
    {
      "goal": "Serialize concurrent invocations of a non-thread-safe action",
      "code": "var exec = new ConcurrentSerializedExecutor(() => counter++); exec.ScheduleOneInvokation(); exec.ScheduleOneInvokation();"
    },
    {
      "goal": "Use a valve, close it, and let it drain unfinished items",
      "code": "var valve = new ConcurrentQueueValve<int>(new LimitedConcurrentQueue<int>(3), x => Console.WriteLine(x)); valve.EnqueueEx(1); valve.CloseValve();"
    },
    {
      "goal": "Use an Intention as a one-shot cross-thread signal",
      "code": "var intent = new Intention(); bool set = intent.Set(); bool realized = intent.TryToRealize();"
    },
    {
      "goal": "Watch an observable set, falling back when Put fails",
      "code": "var set = new ObservableSet<string>(); if (!set.Put(\"A\")) Console.WriteLine(\"already present\");"
    }
  ],
  "relationships": [
    {
      "from": "ConcurrentQueueValve<TData>",
      "to": "ConcurrentUnorderedCollectionValve<TData>",
      "rel": "extends"
    },
    {
      "from": "ConcurrentQueueValve<TData>",
      "to": "IConcurrentQueue<TData>",
      "rel": "implements and takes in constructor"
    },
    {
      "from": "ConcurrentUnorderedCollectionValve<TData>",
      "to": "IConcurrentUnorderedCollection<TData>",
      "rel": "implements and takes in constructor"
    },
    {
      "from": "ConcurrentUnorderedCollectionValve<TData>",
      "to": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "rel": "extends"
    },
    {
      "from": "Intention",
      "to": "Intention.State",
      "rel": "contains nested State enum"
    },
    {
      "from": "ObservableDictionary<TKey, TData>",
      "to": "IMap<TKey, TData>",
      "rel": "implements"
    },
    {
      "from": "ObservableDictionary<TKey, TData>",
      "to": "IObservableDictionary<TKey, TData>",
      "rel": "implements"
    },
    {
      "from": "ObservableDictionary<TKey, TData>",
      "to": "ObservableDictionaryPatch<TKey, TData>",
      "rel": "emits patches of this type via IObservable"
    },
    {
      "from": "ObservableDictionary_Converter<TKey, TData, TKeyFrom, TDataFrom>",
      "to": "IObservableDictionary<TKey, TData>",
      "rel": "implements; returned by IObservableDictionary.ConvertTo"
    },
    {
      "from": "ObservableDictionary_Converter_Data<TKey, TFromData, TData>",
      "to": "IObservableDictionary<TKey, TData>",
      "rel": "implements; returned by IObservableDictionary.ConvertDataTo"
    },
    {
      "from": "ObservableField<T>",
      "to": "IObservableField<T>",
      "rel": "implements"
    },
    {
      "from": "ObservableSet<TData>",
      "to": "Collections.ISet<TData>",
      "rel": "implements"
    },
    {
      "from": "ObservableSet<TData>",
      "to": "IObservableSet<TData>",
      "rel": "implements"
    },
    {
      "from": "ObservableSet<TData>",
      "to": "ObservableSetPatch<TData>",
      "rel": "emits patches of this type via IObservable"
    },
    {
      "from": "ObservableSet_Converter<From, To>",
      "to": "IObservableSet<To>",
      "rel": "implements; returned by IObservableSet.ConvertTo"
    },
    {
      "from": "SingleReaderWriterConcurrentQueueValve<TData>",
      "to": "ISingleReaderWriterConcurrentQueue<TData>",
      "rel": "implements and takes in constructor"
    },
    {
      "from": "SingleReaderWriterConcurrentQueueValve<TData>",
      "to": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "rel": "extends"
    },
    {
      "from": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "to": "ConcurrentManualLockGate",
      "rel": "uses to implement gate semantics"
    },
    {
      "from": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "to": "ISingleReaderWriterConcurrentUnorderedCollection<TData>",
      "rel": "implements and takes in constructor"
    },
    {
      "from": "SingleReaderWriterConcurrentUnorderedCollectionValve<TData>",
      "to": "ValveEnqueueResult",
      "rel": "EnqueueEx returns this enum"
    },
    {
      "from": "StrictKeyEqualityComparer<T, TKey>",
      "to": "KeyEqualityComparer<T, TKey>",
      "rel": "extends"
    }
  ]
}
