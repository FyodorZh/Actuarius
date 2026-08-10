{
  "library": "Actuarius.Collections",
  "meta": {
    "target": "netstandard2.1",
    "lang": 9,
    "nullable": true
  },
  "philosophy": "A collections library whose abstractions split producers and consumers (Put/TryPop) and encode thread-safety into the interface hierarchy via IConcurrent* marker interfaces. Concrete classes wrap the BCL collections or add fixed-capacity circular-buffer and priority-queue semantics; bounded variants signal overflow by returning false rather than throwing. Most types live in namespace Actuarius.Collections; BitMath lives in namespace Actuarius.",
  "quickPick": [
    {
      "need": "1 producer / 1 consumer lock-free FIFO queue",
      "use": "SingleReaderWriterConcurrentQueue<TData>"
    },
    {
      "need": "Adapt a Func<bool> as an IConsumer",
      "use": "ConsumerDelegate<TData>"
    },
    {
      "need": "Adapt a factory Func<T> as an IProducer",
      "use": "ProducerDelegate<TData>"
    },
    {
      "need": "Bounded lock-free FIFO with overflow signal (returns false)",
      "use": "LimitedConcurrentQueue<T>"
    },
    {
      "need": "Circular buffer with indexer and head/tail peeks",
      "use": "CycleQueue<T>"
    },
    {
      "need": "Compare two IReadOnlyArray instances by content",
      "use": "IArray_Ext.EqualByContent"
    },
    {
      "need": "Concurrent LIFO stack protected by a lock",
      "use": "SynchronizedByLockConcurrentStack<TData>"
    },
    {
      "need": "Concurrent map protected by ReaderWriterLockSlim over Dictionary",
      "use": "SynchronizedConcurrentDictionary<TKey, TData>"
    },
    {
      "need": "Concurrent map with atomic add-or-replace / add-or-get",
      "use": "SystemConcurrentDictionary<TKey, TData>"
    },
    {
      "need": "Concurrent set with deduplication",
      "use": "SynchronizedConcurrentSet<TData>"
    },
    {
      "need": "Enumerate any IReadOnlyArray<TData> with foreach",
      "use": "IReadOnlyArray_Enumerators.Enumerate"
    },
    {
      "need": "FIFO queue (non-concurrent)",
      "use": "SystemQueue<TData>"
    },
    {
      "need": "LIFO stack (non-concurrent)",
      "use": "SystemStack<TData>"
    },
    {
      "need": "Min-heap priority queue (smallest key first)",
      "use": "PriorityQueue<Key, Data>"
    },
    {
      "need": "Priority queue with arbitrary-data lookup and key update",
      "use": "PriorityQueueEx<Key, Data>"
    },
    {
      "need": "Unbounded concurrent FIFO for many producers and consumers",
      "use": "SystemConcurrentQueue<TData>"
    },
    {
      "need": "Unordered concurrent bag (removal order undefined)",
      "use": "SystemConcurrentUnorderedCollection<TData>"
    },
    {
      "need": "Wrap an existing IQueue under a lock with a capacity cap",
      "use": "SynchronizedByLockConcurrentQueue<TData>"
    },
    {
      "need": "Wrap an existing IQueue under a spin lock with a capacity cap",
      "use": "SynchronizedBySpinLockConcurrentQueue<TData>"
    }
  ],
  "decisionTree": [
    [
      "Do you need to adapt a Func<bool> or Func<T> to the IConsumer/IProducer abstraction?",
      "Yes",
      [
        [
          "Which contract?",
          "Write side (Put)",
          "ConcurrentConsumerDelegate<TData>"
        ],
        [
          "Which contract?",
          "Read side (TryPop)",
          "ConcurrentProducerDelegate<TData>"
        ],
        [
          "Which contract?",
          "Write side (Put)",
          "ConsumerDelegate<TData>"
        ],
        [
          "Which contract?",
          "Read side (TryPop)",
          "ProducerDelegate<TData>"
        ]
      ]
    ],
    [
      "Do you need thread-safe access from multiple threads?",
      "No",
      [
        [
          "Which removal order?",
          "Circular buffer with indexer and head/tail peeks",
          "CycleQueue<T>"
        ],
        [
          "Which removal order?",
          "Priority, smallest key first",
          "PriorityQueue<Key, Data>"
        ],
        [
          "Which removal order?",
          "Lookup by key",
          "SystemDictionary<TKey, TData>"
        ],
        [
          "Which removal order?",
          "FIFO",
          "SystemQueue<TData>"
        ],
        [
          "Which removal order?",
          "LIFO",
          "SystemStack<TData>"
        ]
      ]
    ],
    [
      "Do you need thread-safe access from multiple threads?",
      "Yes",
      [
        [
          "Which removal order?",
          "FIFO",
          [
            [
              "Which concurrency profile?",
              "Bounded capacity, lock-free",
              "LimitedConcurrentQueue<T>"
            ],
            [
              "Which concurrency profile?",
              "One producer / one consumer",
              "SingleReaderWriterConcurrentQueue<TData>"
            ],
            [
              "Which concurrency profile?",
              "Wrap an existing IQueue under a lock",
              "SynchronizedByLockConcurrentQueue<TData>"
            ],
            [
              "Which concurrency profile?",
              "Wrap an existing IQueue under a spin lock",
              "SynchronizedBySpinLockConcurrentQueue<TData>"
            ],
            [
              "Which concurrency profile?",
              "Unbounded, many-to-many",
              "SystemConcurrentQueue<TData>"
            ]
          ]
        ],
        [
          "Which removal order?",
          "LIFO",
          [
            [
              "Which concurrency profile?",
              "Lock-protected stack",
              "SynchronizedByLockConcurrentStack<TData>"
            ],
            [
              "Which concurrency profile?",
              "Spin-lock-protected stack",
              "SynchronizedBySpinLockConcurrentStack<TData>"
            ],
            [
              "Which concurrency profile?",
              "System stack typed as a queue",
              "SystemConcurrentStack<TData>"
            ]
          ]
        ],
        [
          "Which removal order?",
          "Unordered bag",
          "SystemConcurrentUnorderedCollection<TData>"
        ],
        [
          "Which removal order?",
          "Lookup by key",
          [
            [
              "Which backing?",
              "ReaderWriterLockSlim over Dictionary",
              "SynchronizedConcurrentDictionary<TKey, TData>"
            ],
            [
              "Which backing?",
              "Lock-free System.Concurrent dictionary",
              "SystemConcurrentDictionary<TKey, TData>"
            ]
          ]
        ],
        [
          "Which removal order?",
          "Set with no duplicates",
          "SynchronizedConcurrentSet<TData>"
        ]
      ]
    ]
  ],
  "interfaces": [
    {
      "name": "IArray<TData>",
      "extends": [
        "IReadOnlyArray<TData>"
      ],
      "members": [
        {
          "sig": "this[int id]:TData { get; set; }",
          "desc": "gets or sets the element at 0-based logical index id; position 0 is the head and Count-1 is the tail",
          "preconditions": [
            "id must be in the range [0, Count)"
          ]
        }
      ]
    },
    {
      "name": "IConcurrentConsumer<in TData>",
      "variance": "in TData",
      "extends": [
        "IConsumer<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for an IConsumer whose Put is safe to call from any thread; adds no new members"
    },
    {
      "name": "IConcurrentMap<in TKey, TData>",
      "variance": "in TKey, TData",
      "extends": [
        "IMap<TKey, TData>"
      ],
      "members": [
        {
          "sig": "AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "atomically returns the existing value for key, or adds newElement and returns it; returns true on success",
          "postconditions": [
            "resultElement is the stored value for key after the call"
          ]
        },
        {
          "sig": "AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement):bool",
          "desc": "atomically adds newElement if key is absent or replaces the existing value; oldElement is the replaced value (or default for a new key); returns true on success",
          "postconditions": [
            "the stored value for key is newElement"
          ]
        },
        {
          "sig": "GetAndRemove(TKey key, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "removes the entry with the given key and returns its value; returns false if the key is absent"
        }
      ]
    },
    {
      "name": "IConcurrentProducer<TData>",
      "extends": [
        "IProducer<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for an IProducer whose TryPop is safe to call from any thread; adds no new members"
    },
    {
      "name": "IConcurrentQueue<TData>",
      "extends": [
        "ISingleReaderWriterConcurrentQueue<TData>",
        "IConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a fully thread-safe FIFO queue (many producers and many consumers); adds no new members"
    },
    {
      "name": "IConcurrentSet<in TData>",
      "variance": "in TData",
      "extends": [
        "ISet<TData>",
        "IConcurrentConsumer<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a thread-safe set; adds no new members"
    },
    {
      "name": "IConcurrentStack<TData>",
      "extends": [
        "ISingleReaderWriterConcurrentStack<TData>",
        "IConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a fully thread-safe LIFO stack; adds no new members"
    },
    {
      "name": "IConcurrentUnorderedCollection<TData>",
      "extends": [
        "ISingleReaderWriterConcurrentUnorderedCollection<TData>",
        "IConcurrentConsumer<TData>",
        "IConcurrentProducer<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a fully thread-safe bag with undefined removal order; adds no new members"
    },
    {
      "name": "IConsumer<in TData>",
      "variance": "in TData",
      "members": [
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value to the sink; returns false if the element was not accepted, for example when capacity is exhausted; holds reference only"
        }
      ]
    },
    {
      "name": "ICountable",
      "members": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements currently held"
        }
      ]
    },
    {
      "name": "IMap<in TKey, TData>",
      "variance": "in TKey, TData",
      "members": [
        {
          "sig": "Add(TKey key, TData element):bool",
          "desc": "adds the entry for key; returns false if the key already exists or capacity is exhausted"
        },
        {
          "sig": "Remove(TKey key):bool",
          "desc": "removes the entry with the given key; returns false if the key is absent"
        },
        {
          "sig": "TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element):bool",
          "desc": "retrieves the value for key; returns false and sets element to default if the key is absent"
        }
      ]
    },
    {
      "name": "IProducer<TData>",
      "members": [
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData value):bool",
          "desc": "retrieves the next element; returns false and sets value to default if no element is available"
        }
      ]
    },
    {
      "name": "IQueue<TData>",
      "extends": [
        "IUnorderedCollection<TData>",
        "ICountable"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a FIFO queue in which elements are removed in insertion order; thread-safety is not guaranteed; adds no new members"
    },
    {
      "name": "IReadOnlyArray<out TData>",
      "variance": "out TData",
      "extends": [
        "ICountable"
      ],
      "members": [
        {
          "sig": "this[int id]:TData { get; }",
          "desc": "returns the element at 0-based logical index id; behaviour for out-of-range id is implementation-defined"
        }
      ]
    },
    {
      "name": "ISet<in TData>",
      "variance": "in TData",
      "extends": [
        "IConsumer<TData>"
      ],
      "members": [
        {
          "sig": "Contains(TData element):bool",
          "desc": "returns true if element is present in the set"
        },
        {
          "sig": "Remove(TData element):bool",
          "desc": "removes element from the set; returns false if it was not present"
        }
      ]
    },
    {
      "name": "ISingleReaderWriterConcurrentQueue<TData>",
      "extends": [
        "IQueue<TData>",
        "ISingleReaderWriterConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a FIFO queue safe when one thread writes and another reads; adds no new members"
    },
    {
      "name": "ISingleReaderWriterConcurrentStack<TData>",
      "extends": [
        "IStack<TData>",
        "ISingleReaderWriterConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a LIFO stack safe when one thread writes and another reads; adds no new members"
    },
    {
      "name": "ISingleReaderWriterConcurrentUnorderedCollection<TData>",
      "extends": [
        "IUnorderedCollection<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a bag safe when one thread writes and another reads; adds no new members"
    },
    {
      "name": "IStack<TData>",
      "extends": [
        "IUnorderedCollection<TData>",
        "ICountable"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a LIFO stack; thread-safety is not guaranteed; adds no new members"
    },
    {
      "name": "IUnorderedCollection<TData>",
      "extends": [
        "IConsumer<TData>",
        "IProducer<TData>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for a bag in which removal order is undefined; adds no new members"
    }
  ],
  "types": [
    {
      "name": "BitMath",
      "kind": "staticClass",
      "category": "utility",
      "isAuxiliary": false,
      "desc": "static bit-twiddling helpers used to round capacities to powers of two",
      "methods": [
        {
          "sig": "HiBit(uint n):int",
          "desc": "returns the 0-based position of the highest set bit in n; returns 0 if n is 0; O(1)",
          "example": "int p = BitMath.HiBit(16);",
          "remarks": "lives in namespace Actuarius, not Actuarius.Collections"
        },
        {
          "sig": "NextPow2(uint n):int",
          "desc": "returns the smallest power of two greater than or equal to n; returns 1 if n is 0; O(1)",
          "preconditions": [
            "n must be less than 2^31 for a valid power-of-two result"
          ],
          "postconditions": [
            "result is a power of two and result >= n"
          ],
          "example": "int cap = BitMath.NextPow2(100);",
          "remarks": "for n with the top bit set (n >= 2^31) the result is not a valid power of two"
        }
      ]
    },
    {
      "name": "ConcurrentConsumerDelegate<TData>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IConcurrentConsumer<TData>"
      ],
      "isAuxiliary": false,
      "desc": "struct adapter that exposes a Func<TData, bool> as an IConcurrentConsumer",
      "constructors": [
        {
          "sig": "ctor(Func<TData, bool> action)",
          "desc": "creates a consumer that invokes action for every Put; action must not be null",
          "preconditions": [
            "action is not null"
          ],
          "remarks": "holds reference only; thread-safety depends on the wrapped delegate"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "invokes the wrapped action with value and returns its result; O(1) plus delegate cost",
          "example": "IConcurrentConsumer<int> c = new ConcurrentConsumerDelegate<int>(x => x > 0);"
        }
      ]
    },
    {
      "name": "ConcurrentProducerDelegate<TData>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IConcurrentProducer<TData>"
      ],
      "isAuxiliary": false,
      "desc": "struct adapter that exposes a Func<TData?> factory as an IConcurrentProducer",
      "constructors": [
        {
          "sig": "ctor(Func<TData?> producerFunc)",
          "desc": "creates a producer that calls producerFunc for every TryPop; producerFunc must not be null",
          "preconditions": [
            "producerFunc is not null"
          ],
          "remarks": "holds reference only; thread-safety depends on the wrapped delegate"
        }
      ],
      "methods": [
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "invokes the wrapped function, sets value to its result, and returns true only if the result is non-null; null signals an empty producer; O(1) plus delegate cost",
          "example": "IConcurrentProducer<int> p = new ConcurrentProducerDelegate<int>(() => 5);"
        }
      ]
    },
    {
      "name": "ConsumerDelegate<TData>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IConsumer<TData>"
      ],
      "isAuxiliary": false,
      "desc": "struct adapter that exposes a Func<TData, bool> as an IConsumer",
      "constructors": [
        {
          "sig": "ctor(Func<TData, bool> action)",
          "desc": "creates a consumer that invokes action for every Put; action must not be null",
          "preconditions": [
            "action is not null"
          ],
          "remarks": "holds reference only; thread-safety depends on the wrapped delegate"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "invokes the wrapped action with value and returns its result; O(1) plus delegate cost",
          "example": "IConsumer<int> c = new ConsumerDelegate<int>(x => x > 0); bool ok = c.Put(5);"
        }
      ]
    },
    {
      "name": "CycleQueue<T>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IQueue<T>",
        "IArray<T>"
      ],
      "isAuxiliary": false,
      "desc": "fixed-capacity circular FIFO queue that can also be indexed and peeked at both ends; capacity is always a power of two",
      "threadSafety": "not thread-safe",
      "limitations": "not thread-safe; Head, Tail and the indexer throw plain Exception on empty or out-of-range access; capacity is always rounded up to a power of two; Clear does not release element references",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a circular queue with capacity 16 and canGrow=true",
          "example": "var q = new CycleQueue<int>();"
        },
        {
          "sig": "ctor(int capacity, bool canGrow = true)",
          "desc": "creates a circular queue whose capacity is rounded up to the next power of two (minimum 1); if canGrow, Put may double the capacity when full",
          "preconditions": [
            "capacity is 0 or greater (0 is treated as 1)"
          ],
          "postconditions": [
            "Capacity is a power of two and Capacity >= capacity"
          ],
          "example": "var q = new CycleQueue<int>(100, canGrow: false);",
          "remarks": "canGrow=false makes Put and PutToHead return false when full"
        }
      ],
      "properties": [
        {
          "sig": "Capacity:int { get; }",
          "desc": "returns the current backing capacity, always a power of two; O(1)"
        },
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements currently held; O(1)"
        },
        {
          "sig": "Head:T { get; }",
          "desc": "returns the head element without removing it; throws Exception if empty; O(1)",
          "example": "int first = q.Head;"
        },
        {
          "sig": "Tail:T { get; }",
          "desc": "returns the tail element without removing it; throws Exception if empty; O(1)"
        }
      ],
      "indexers": [
        {
          "sig": "this[int id]:T { get; set; }",
          "desc": "gets or sets the element at logical index id where 0 is the head and Count-1 is the tail; throws Exception if id < 0 or id >= Count; O(1)",
          "preconditions": [
            "id must be in the range [0, Count)"
          ],
          "example": "q[1] = 25;"
        }
      ],
      "methods": [
        {
          "sig": "Clear()",
          "desc": "empties the queue; capacity is unchanged and the backing array still references the old elements until overwritten; O(1)"
        },
        {
          "sig": "Enumerate()",
          "desc": "returns an enumerable that iterates the queue from head to tail; O(1)",
          "example": "foreach (var x in q.Enumerate()) {}"
        },
        {
          "sig": "Enumerate(QueueEnumerationOrder order):EnumerableWithOrder",
          "desc": "returns an enumerable that iterates the queue in the given order, either HeadToTail or TailToHead; O(1)",
          "example": "foreach (var x in q.Enumerate(QueueEnumerationOrder.TailToHead)) {}"
        },
        {
          "sig": "Put(T value):bool",
          "desc": "enqueues value at the tail; grows the capacity by doubling when full if canGrow is true; returns false if full and canGrow is false; amortised O(1)",
          "example": "bool ok = q.Put(42);"
        },
        {
          "sig": "PutToHead(T value):bool",
          "desc": "prepends value at the head; grows the capacity by doubling when full if canGrow is true; returns false if full and canGrow is false; amortised O(1)"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out T):bool",
          "desc": "dequeues the head element and clears its slot; returns false and sets value to default if empty; O(1)",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v);"
        }
      ],
      "nested": [
        {
          "name": "EnumerableWithOrder",
          "kind": "struct",
          "implements": [
            "IEnumerable<T>"
          ],
          "isAuxiliary": true,
          "desc": "readonly struct enumerable returned by CycleQueue.Enumerate; supports foreach and GetEnumerator() returning an Enumerator"
        },
        {
          "name": "Enumerator",
          "kind": "struct",
          "implements": [
            "IEnumerator<T>"
          ],
          "isAuxiliary": true,
          "desc": "struct enumerator over a CycleQueue supporting foreach; exposes Current, MoveNext, Reset and Dispose"
        }
      ]
    },
    {
      "name": "IReadOnlyArray_Enumerators.AsEnumerable<TData>",
      "kind": "struct",
      "implements": [
        "IEnumerable<TData>"
      ],
      "isAuxiliary": true,
      "desc": "readonly struct enumerable returned by the Enumerate extension; enables foreach over any IReadOnlyArray<TData>"
    },
    {
      "name": "IReadOnlyArray_Enumerators.Enumerator<TData>",
      "kind": "struct",
      "implements": [
        "IEnumerator<TData>"
      ],
      "isAuxiliary": true,
      "desc": "struct enumerator over an IReadOnlyArray<TData>; throws IndexOutOfRangeException from Current if accessed before the first MoveNext"
    },
    {
      "name": "LimitedConcurrentQueue<T>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IConcurrentQueue<T>"
      ],
      "isAuxiliary": false,
      "desc": "bounded lock-free (CAS) FIFO queue with a fixed capacity that never grows",
      "threadSafety": "lock-free",
      "limitations": "fixed capacity; TryPop busy-waits while a producer is writing the slot it wants to read; prefer SystemConcurrentQueue for unbounded use",
      "constructors": [
        {
          "sig": "ctor(int capacity)",
          "desc": "creates a bounded lock-free FIFO with the given fixed capacity",
          "preconditions": [
            "capacity is greater than 0"
          ],
          "example": "var q = new LimitedConcurrentQueue<int>(100);"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; read via Volatile.Read; lock-free; O(1)",
          "threadSafety": "lock-free"
        }
      ],
      "methods": [
        {
          "sig": "Put(T value):bool",
          "desc": "enqueues value if the queue is below capacity; returns false if full; lock-free; O(1)",
          "threadSafety": "lock-free",
          "example": "bool ok = q.Put(42);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out T):bool",
          "desc": "dequeues the oldest element; returns false and sets value to default if empty; spins until a producer publishes the slot being read; lock-free; O(1)",
          "threadSafety": "lock-free",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v); else Console.WriteLine(\"empty\");"
        }
      ]
    },
    {
      "name": "PriorityQueue<Key, Data>",
      "kind": "class",
      "category": "priority-queue",
      "implements": [
        "IQueue<KeyValuePair<Key, Data>>"
      ],
      "genericConstraints": "where Key : IComparable<Key>",
      "isAuxiliary": false,
      "desc": "min-heap priority queue: the smallest key has the highest priority and is dequeued first",
      "threadSafety": "not thread-safe",
      "limitations": "not thread-safe; Dequeue returns default(Data) when empty instead of throwing; Enumerate walks the heap array rather than sorted order; use PriorityQueueEx to find and update arbitrary data",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty min-heap priority queue",
          "example": "var pq = new PriorityQueue<int, string>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of enqueued elements; O(1)"
        },
        {
          "sig": "Values:DataEnumerable { get; }",
          "desc": "returns a DataEnumerable over the raw backing data list (skipping the internal dummy element); iterating it does not change priority order; O(1)",
          "example": "foreach (var d in pq.Values) {}"
        }
      ],
      "methods": [
        {
          "sig": "Clear()",
          "desc": "removes all elements; O(1)"
        },
        {
          "sig": "Dequeue():Data",
          "desc": "removes and returns the data with the smallest key; returns default(Data) if empty (does not throw); O(log n)",
          "preconditions": [
            "Count is greater than 0 to obtain a meaningful result"
          ],
          "example": "string top = pq.Dequeue();"
        },
        {
          "sig": "Enqueue(Key key, Data data)",
          "desc": "adds data with the given priority key; smaller key means higher priority; O(log n)",
          "example": "pq.Enqueue(1, \"one\");"
        },
        {
          "sig": "Enumerate(QueueEnumerationOrder order):Enumerable",
          "desc": "returns an enumerable over DataRefs in heap-array index order (1..Count), not sorted-priority order; O(1)"
        },
        {
          "sig": "Peek():Data",
          "desc": "returns the data with the smallest key without removing it; throws IndexOutOfRangeException if empty; O(1)",
          "example": "string min = pq.Peek();"
        },
        {
          "sig": "Put(KeyValuePair<Key, Data> value):bool",
          "desc": "enqueues the pair using value.Key as priority; always returns true; O(log n)",
          "example": "pq.Put(new KeyValuePair<int, string>(0, \"zero\"));"
        },
        {
          "sig": "TopKey():Key",
          "desc": "returns the smallest key without removing anything; throws IndexOutOfRangeException if empty; O(1)",
          "example": "int minKey = pq.TopKey();"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out KeyValuePair<Key, Data>):bool",
          "desc": "removes and returns the pair with the smallest key; returns false and sets value to default if empty; O(log n)",
          "example": "if (pq.TryPop(out var kv)) Console.WriteLine(kv.Value);"
        }
      ],
      "nested": [
        {
          "name": "DataEnumerable",
          "kind": "struct",
          "isAuxiliary": true,
          "desc": "readonly struct enumerable over the raw backing data list, returned by PriorityQueue.Values"
        },
        {
          "name": "DataRef",
          "kind": "struct",
          "isAuxiliary": false,
          "desc": "lightweight handle to an element inside a PriorityQueue heap; obtained from PriorityQueueEx.FindData or enumeration; invalid (default) refs report IsValid == false",
          "constructors": [
            {
              "sig": "ctor(PriorityQueue<Key, Data> owner, int id)",
              "desc": "creates a ref to the heap element at index id of owner; normally obtained from FindData or enumeration rather than constructed directly"
            }
          ],
          "properties": [
            {
              "sig": "IsValid:bool { get; }",
              "desc": "returns true if this ref points to a live owner; a default-constructed or Update-invalidated DataRef is invalid",
              "example": "if (r.IsValid) r.Update(10);"
            }
          ],
          "methods": [
            {
              "sig": "Remove():bool",
              "desc": "removes the referenced element from the heap; returns false if the ref is invalid or the id is out of range; does NOT invalidate the ref (IsValid stays true and the id may now point to a different element); O(log n)"
            },
            {
              "sig": "ShowData():Data",
              "desc": "returns the referenced data; throws NullReferenceException if the ref is invalid"
            },
            {
              "sig": "ShowKey():Key",
              "desc": "returns the referenced key; throws NullReferenceException if the ref is invalid"
            },
            {
              "sig": "Update(Key newKey):bool",
              "desc": "changes the priority of the referenced element; returns false if the ref is already invalid or the id is out of range; always invalidates this ref (IsValid becomes false after the call); O(log n)",
              "example": "r.Update(10);"
            }
          ]
        },
        {
          "name": "Enumerable",
          "kind": "struct",
          "isAuxiliary": true,
          "desc": "readonly struct enumerable over DataRefs returned by PriorityQueue.Enumerate"
        },
        {
          "name": "Enumerator",
          "kind": "struct",
          "implements": [
            "IEnumerator<DataRef>"
          ],
          "isAuxiliary": true,
          "desc": "struct enumerator over DataRefs supporting foreach; the non-generic IEnumerator.Current throws InvalidOperationException"
        }
      ]
    },
    {
      "name": "PriorityQueueEx<Key, Data>",
      "kind": "class",
      "category": "priority-queue",
      "base": "PriorityQueue<Key, Data>",
      "genericConstraints": "where Key : IComparable<Key>",
      "isAuxiliary": false,
      "desc": "priority queue extended with arbitrary-data lookup that returns a DataRef for later update or removal",
      "threadSafety": "not thread-safe",
      "limitations": "FindData is O(n); the returned DataRef is only safe while the heap is not modified concurrently; use PriorityQueue when only priority ordering is needed",
      "methods": [
        {
          "sig": "FindData(Func<Data, bool> finder):DataRef",
          "desc": "scans the heap array and returns a DataRef to the first element for which finder returns true; returns an invalid DataRef if no element matches; throws NullReferenceException if finder is null; O(n)",
          "preconditions": [
            "finder is not null"
          ],
          "example": "var r = pqEx.FindData(d => d == \"hello\");"
        }
      ]
    },
    {
      "name": "ProducerDelegate<TData>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IProducer<TData>"
      ],
      "isAuxiliary": false,
      "desc": "struct adapter that exposes a Func<TData?> factory as an IProducer",
      "constructors": [
        {
          "sig": "ctor(Func<TData?> producerFunc)",
          "desc": "creates a producer that calls producerFunc for every TryPop; producerFunc must not be null",
          "preconditions": [
            "producerFunc is not null"
          ],
          "remarks": "holds reference only; thread-safety depends on the wrapped delegate"
        }
      ],
      "methods": [
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "invokes the wrapped function, sets value to its result, and returns true only if the result is non-null; null signals an empty producer; O(1) plus delegate cost",
          "example": "IProducer<int> p = new ProducerDelegate<int>(() => 5);"
        }
      ]
    },
    {
      "name": "QueueEnumerationOrder",
      "kind": "enum",
      "isAuxiliary": true,
      "desc": "HeadToTail=0, TailToHead=1"
    },
    {
      "name": "SingleReaderWriterConcurrentQueue<TData>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "ISingleReaderWriterConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "queue for exactly one producer and one consumer, backed by two internal CycleQueue buffers swapped via Interlocked.Exchange; the producer has priority in conflicts",
      "threadSafety": "safe for 1 writer/1 reader",
      "limitations": "only one writer and one reader thread may use it; Count is approximate and may be momentarily inaccurate during a swap",
      "constructors": [
        {
          "sig": "ctor(int initialCapacity = 10)",
          "desc": "creates a 1-producer/1-consumer queue with two internal buffers of the given initial capacity",
          "example": "var q = new SingleReaderWriterConcurrentQueue<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the sum of both internal buffers and may be momentarily inaccurate during a swap; safe for 1 writer/1 reader; O(1)",
          "threadSafety": "safe for 1 writer/1 reader"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value to the writer buffer, growing it if needed; returns true unless growth fails; safe for 1 writer/1 reader; amortised O(1)",
          "threadSafety": "safe for 1 writer/1 reader",
          "example": "q.Put(1);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "removes the oldest element from the reader buffer, swapping buffers when it is empty and the writer has published new items; returns false if both are empty; safe for 1 writer/1 reader; amortised O(1)",
          "threadSafety": "safe for 1 writer/1 reader",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SynchronizedByLockConcurrentQueue<TData>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "decorator that serialises access to any IQueue<TData> with a lock and optionally bounds its capacity",
      "threadSafety": "thread-safe (lock)",
      "limitations": "not lock-free; the wrapped IQueue must itself only be used through this decorator; prefer LimitedConcurrentQueue or SystemConcurrentQueue for lock-free concurrency",
      "constructors": [
        {
          "sig": "ctor(IQueue<TData> queue, int maxCapacity = -1)",
          "desc": "wraps the given IQueue, serialising access with a lock; maxCapacity <= 0 means unbounded",
          "preconditions": [
            "queue is not null"
          ],
          "remarks": "holds reference only; caller keeps ownership of queue",
          "example": "var q = new SynchronizedByLockConcurrentQueue<int>(new SystemQueue<int>(), maxCapacity: 1000);"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements under the lock; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value if the queue is below maxCapacity; returns false if at capacity or the inner queue rejects it; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)",
          "example": "bool ok = q.Put(42);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "removes the head element; returns false if empty; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SynchronizedByLockConcurrentStack<TData>",
      "kind": "class",
      "category": "stack",
      "implements": [
        "IConcurrentStack<TData>"
      ],
      "isAuxiliary": false,
      "desc": "LIFO stack guarded by a lock, optionally with a maximum capacity",
      "threadSafety": "thread-safe (lock)",
      "limitations": "not lock-free; a maximum capacity causes Put to return false instead of blocking",
      "constructors": [
        {
          "sig": "ctor(int maxCapacity = -1)",
          "desc": "creates a lock-protected stack; maxCapacity <= 0 means unbounded",
          "example": "var s = new SynchronizedByLockConcurrentStack<int>(maxCapacity: 100);"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements under the lock; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "pushes value if the stack is below maxCapacity; returns false if at capacity; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "s.Put(1);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "pops the most recently pushed element; returns false if empty; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)",
          "example": "if (s.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SynchronizedBySpinLockConcurrentQueue<TData>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "decorator that serialises access to any IQueue<TData> with a SpinLock and optionally bounds its capacity",
      "threadSafety": "thread-safe (lock)",
      "limitations": "uses System.Threading.SpinLock, which spins the CPU under contention; the wrapped IQueue must only be used through this decorator",
      "constructors": [
        {
          "sig": "ctor(IQueue<TData> queue, int maxCapacity = -1)",
          "desc": "wraps the given IQueue, serialising access with a SpinLock; maxCapacity <= 0 means unbounded",
          "preconditions": [
            "queue is not null"
          ],
          "remarks": "holds reference only; caller keeps ownership of queue",
          "example": "var q = new SynchronizedBySpinLockConcurrentQueue<int>(new SystemQueue<int>());"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements under the spin lock; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value if the queue is below maxCapacity; returns false if at capacity or the inner queue rejects it; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "removes the head element; returns false if empty; thread-safe (lock); O(1) plus inner queue cost",
          "threadSafety": "thread-safe (lock)"
        }
      ]
    },
    {
      "name": "SynchronizedBySpinLockConcurrentStack<TData>",
      "kind": "class",
      "category": "stack",
      "implements": [
        "IConcurrentStack<TData>"
      ],
      "isAuxiliary": false,
      "desc": "LIFO stack guarded by a SpinLock, optionally with a maximum capacity",
      "threadSafety": "thread-safe (lock)",
      "limitations": "uses System.Threading.SpinLock, which spins the CPU under contention",
      "constructors": [
        {
          "sig": "ctor(int maxCapacity = -1)",
          "desc": "creates a spin-lock-protected stack; maxCapacity <= 0 means unbounded",
          "example": "var s = new SynchronizedBySpinLockConcurrentStack<int>(maxCapacity: 100);"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements under the spin lock; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "pushes value if the stack is below maxCapacity; returns false if at capacity; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "pops the most recently pushed element; returns false if empty; thread-safe (lock); O(1)",
          "threadSafety": "thread-safe (lock)"
        }
      ]
    },
    {
      "name": "SynchronizedConcurrentDictionary<TKey, TData>",
      "kind": "class",
      "category": "map",
      "implements": [
        "IConcurrentMap<TKey, TData>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe map backed by a Dictionary<TKey, TData> guarded by ReaderWriterLockSlim",
      "threadSafety": "thread-safe (lock)",
      "limitations": "not lock-free; Add, AddOrReplace, AddOrGet, GetAndRemove and Remove swallow all exceptions and return false; prefer SystemConcurrentDictionary for lock-free concurrency",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty thread-safe dictionary using the default key comparer",
          "example": "var d = new SynchronizedConcurrentDictionary<string, int>();"
        }
      ],
      "methods": [
        {
          "sig": "Add(TKey key, TData element):bool",
          "desc": "adds the entry if key is absent; returns false if the key exists or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "returns the existing value for key, or adds newElement and returns it; always returns true unless an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement):bool",
          "desc": "atomically adds newElement or replaces the existing value; oldElement is the replaced value, or newElement itself if the key is newly added; always returns true unless an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "GetAndRemove(TKey key, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "removes the entry and returns its value; returns false if the key is absent or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "Remove(TKey key):bool",
          "desc": "removes the entry; returns false if the key is absent or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "ReplaceOrAdd(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement):bool",
          "desc": "always overwrites the stored value (adds if absent, replaces if present); oldElement is the previous value or default if absent; always returns true unless an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)",
          "example": "d.ReplaceOrAdd(\"x\", 100, out var prev);"
        },
        {
          "sig": "TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element):bool",
          "desc": "returns the value for key; returns false if the key is absent or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        }
      ]
    },
    {
      "name": "SynchronizedConcurrentSet<TData>",
      "kind": "class",
      "category": "set",
      "implements": [
        "IConcurrentSet<TData>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe set backed by a HashSet<TData> guarded by ReaderWriterLockSlim",
      "threadSafety": "thread-safe (lock)",
      "limitations": "not lock-free; all members swallow exceptions and return false on failure",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty thread-safe set using the default equality comparer",
          "example": "var s = new SynchronizedConcurrentSet<int>();"
        }
      ],
      "methods": [
        {
          "sig": "Contains(TData element):bool",
          "desc": "returns true if element is present; returns false if an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        },
        {
          "sig": "Put(TData element):bool",
          "desc": "adds element; returns false if it is already present or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)",
          "example": "bool added = s.Put(1);"
        },
        {
          "sig": "Remove(TData element):bool",
          "desc": "removes element; returns false if it is absent or an exception occurs; thread-safe (lock); O(1) average",
          "threadSafety": "thread-safe (lock)"
        }
      ]
    },
    {
      "name": "SystemConcurrentDictionary<TKey, TData>",
      "kind": "class",
      "category": "map",
      "implements": [
        "IConcurrentMap<TKey, TData>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe map backed by a System.Collections.Concurrent.ConcurrentDictionary<TKey, TData>",
      "threadSafety": "lock-free",
      "limitations": "keys use the default comparer; prefer SynchronizedConcurrentDictionary if custom locking is required",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty thread-safe dictionary using the default key comparer",
          "example": "var d = new SystemConcurrentDictionary<string, int>();"
        }
      ],
      "methods": [
        {
          "sig": "Add(TKey key, TData element):bool",
          "desc": "adds the entry if key is absent; returns false if the key already exists; lock-free; O(1) average",
          "threadSafety": "lock-free",
          "example": "d.Add(\"a\", 1);"
        },
        {
          "sig": "AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "returns the existing value for key, or adds newElement and returns it; always returns true; lock-free; O(1) average",
          "threadSafety": "lock-free"
        },
        {
          "sig": "AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement):bool",
          "desc": "atomically adds newElement or replaces the existing value; oldElement is the replaced value, or default if the key is new; always returns true; lock-free; O(1) average",
          "threadSafety": "lock-free",
          "example": "d.AddOrReplace(\"a\", 2, out int old);"
        },
        {
          "sig": "GetAndRemove(TKey key, [MaybeNullWhen(false)] out TData resultElement):bool",
          "desc": "removes the entry and returns its value; returns false if the key is absent; lock-free; O(1) average",
          "threadSafety": "lock-free",
          "example": "if (d.GetAndRemove(\"a\", out int v)) Console.WriteLine(v);"
        },
        {
          "sig": "Remove(TKey key):bool",
          "desc": "removes the entry; returns false if the key is absent; lock-free; O(1) average",
          "threadSafety": "lock-free"
        },
        {
          "sig": "TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element):bool",
          "desc": "returns the value for key; returns false if the key is absent; lock-free; O(1) average",
          "threadSafety": "lock-free"
        }
      ]
    },
    {
      "name": "SystemConcurrentQueue<TData>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "unbounded thread-safe FIFO queue backed by a System.Collections.Concurrent.ConcurrentQueue<TData>",
      "threadSafety": "lock-free",
      "limitations": "unbounded; use LimitedConcurrentQueue for a bounded queue",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty unbounded concurrent queue",
          "example": "var q = new SystemConcurrentQueue<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; lock-free; O(n)",
          "threadSafety": "lock-free"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "enqueues value; always returns true; lock-free; amortised O(1)",
          "threadSafety": "lock-free",
          "example": "q.Put(1);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "dequeues the oldest element; returns false if empty; lock-free; O(1)",
          "threadSafety": "lock-free",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SystemConcurrentStack<TData>",
      "kind": "class",
      "category": "stack",
      "implements": [
        "IConcurrentQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe LIFO stack backed by a System.Collections.Concurrent.ConcurrentStack<TData>",
      "threadSafety": "lock-free",
      "limitations": "implements IConcurrentQueue<TData>, not IConcurrentStack<TData>, even though it is a stack; use SynchronizedByLockConcurrentStack or SynchronizedBySpinLockConcurrentStack for a true IConcurrentStack",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty concurrent stack",
          "example": "var s = new SystemConcurrentStack<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; lock-free; O(n)",
          "threadSafety": "lock-free"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "pushes value; always returns true; lock-free; O(1)",
          "threadSafety": "lock-free",
          "example": "s.Put(1);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "pops the most recently pushed element; returns false if empty; lock-free; O(1)",
          "threadSafety": "lock-free",
          "example": "if (s.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SystemConcurrentUnorderedCollection<TData>",
      "kind": "class",
      "category": "bag",
      "implements": [
        "IConcurrentUnorderedCollection<TData>"
      ],
      "isAuxiliary": false,
      "desc": "thread-safe unordered bag backed by a System.Collections.Concurrent.ConcurrentBag<TData>; removal order is undefined",
      "threadSafety": "lock-free",
      "limitations": "removal order is undefined; prefer a queue or stack when ordering matters",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty concurrent bag",
          "example": "var b = new SystemConcurrentUnorderedCollection<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; lock-free; O(n)",
          "threadSafety": "lock-free"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "adds value; always returns true; lock-free; amortised O(1)",
          "threadSafety": "lock-free"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "takes an arbitrary element; returns false if empty; lock-free; O(1)",
          "threadSafety": "lock-free"
        }
      ]
    },
    {
      "name": "SystemDictionary<TKey, TData>",
      "kind": "class",
      "category": "map",
      "implements": [
        "IMap<TKey, TData>"
      ],
      "genericConstraints": "where TKey : IEquatable<TKey>",
      "isAuxiliary": false,
      "desc": "non-thread-safe map backed by a Dictionary<TKey, TData>",
      "threadSafety": "not thread-safe",
      "limitations": "not thread-safe; Add swallows all exceptions and returns false instead of propagating",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty dictionary using the default key comparer",
          "example": "var d = new SystemDictionary<string, int>();"
        }
      ],
      "methods": [
        {
          "sig": "Add(TKey key, TData element):bool",
          "desc": "adds the entry if key is absent; returns false if the key already exists or an exception occurs; O(1) average",
          "example": "d.Add(\"a\", 1);"
        },
        {
          "sig": "Remove(TKey key):bool",
          "desc": "removes the entry; returns false if the key is absent; O(1) average"
        },
        {
          "sig": "TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element):bool",
          "desc": "returns the value for key; returns false if the key is absent; O(1) average"
        }
      ]
    },
    {
      "name": "SystemQueue<TData>",
      "kind": "class",
      "category": "queue",
      "implements": [
        "IQueue<TData>"
      ],
      "isAuxiliary": false,
      "desc": "non-thread-safe FIFO queue backed by a System.Collections.Generic.Queue<TData>",
      "threadSafety": "not thread-safe",
      "limitations": "not thread-safe; use a concurrent queue variant when access is from multiple threads",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty FIFO queue",
          "example": "var q = new SystemQueue<int>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Clear()",
          "desc": "removes all elements; O(n)"
        },
        {
          "sig": "Put(TData value):bool",
          "desc": "enqueues value at the tail; always returns true; O(1); holds reference only",
          "example": "q.Put(1);"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "dequeues the head element; returns false and sets value to default if empty; O(1)",
          "example": "if (q.TryPop(out int v)) Console.WriteLine(v);"
        }
      ]
    },
    {
      "name": "SystemStack<TData>",
      "kind": "class",
      "category": "stack",
      "implements": [
        "IStack<TData>"
      ],
      "isAuxiliary": false,
      "desc": "non-thread-safe LIFO stack backed by a System.Collections.Generic.Stack<TData>",
      "threadSafety": "not thread-safe",
      "limitations": "not thread-safe; use a concurrent stack variant when access is from multiple threads",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty LIFO stack",
          "example": "var s = new SystemStack<string>();"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of elements; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Put(TData value):bool",
          "desc": "pushes value; always returns true; O(1); holds reference only",
          "example": "s.Put(\"a\");"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TData):bool",
          "desc": "pops the most recently pushed element; returns false and sets value to default if empty; O(1)",
          "example": "if (s.TryPop(out string v)) Console.WriteLine(v);"
        }
      ]
    }
  ],
  "extensions": [
    {
      "for": "IReadOnlyArray<T>",
      "members": [
        {
          "sig": "Enumerate<TData>(this IReadOnlyArray<TData> list):AsEnumerable<TData>",
          "desc": "returns an AsEnumerable<TData> that enables foreach over any IReadOnlyArray<TData>; O(1)",
          "example": "foreach (var x in arr.Enumerate()) {}"
        },
        {
          "sig": "EqualByContent<T>(this IReadOnlyArray<T>? self, IReadOnlyArray<T>? other):bool",
          "desc": "compares two arrays element-by-element using IEquatable<T>; returns true if both are null, false if exactly one is null, otherwise true only if counts match and every element is equal; O(n)",
          "preconditions": [
            "T implements IEquatable<T>"
          ],
          "example": "bool same = a.EqualByContent(b);"
        }
      ]
    }
  ],
  "gotchas": [
    "CycleQueue capacity is always rounded up to a power of two (via BitMath.NextPow2), so new CycleQueue<int>(100) actually has Capacity 128.",
    "CycleQueue.Clear resets Count to 0 but keeps the backing array, so element references are not released until overwritten; large objects can be retained.",
    "CycleQueue.Head, CycleQueue.Tail and the indexer throw plain System.Exception (not InvalidOperationException or ArgumentOutOfRangeException) when empty or out of range.",
    "LimitedConcurrentQueue.TryPop busy-waits (spins) until a producer publishes the slot it wants to read; there is no backoff, so contention spins CPU.",
    "PriorityQueue.DataRef.Remove does NOT invalidate the ref: after a successful Remove, IsValid still returns true and the ref may point to a different element after the heap reheapifies. Only Update invalidates.",
    "PriorityQueue.Dequeue does not throw when empty; it returns default(Data). Check Count first or use TryPop.",
    "PriorityQueue.Enumerate walks the raw heap array (indices 1..Count), not sorted-priority order; call Dequeue repeatedly to consume in priority order.",
    "PriorityQueue.TopKey and PriorityQueue.Peek throw IndexOutOfRangeException (not InvalidOperationException) when empty.",
    "ProducerDelegate signals 'empty' by returning null from the wrapped function, so it cannot produce legitimately-null values; prefer value types.",
    "SingleReaderWriterConcurrentQueue.Count is the sum of two internal buffers and may be momentarily inaccurate during a swap.",
    "SynchronizedConcurrentDictionary.Add, AddOrReplace, AddOrGet, GetAndRemove, Remove and all SynchronizedConcurrentSet members swallow exceptions and return false, hiding real failures.",
    "SynchronizedConcurrentDictionary.AddOrReplace sets oldElement = newElement when the key is newly added, unlike SystemConcurrentDictionary.AddOrReplace which sets oldElement = default.",
    "SystemConcurrentStack implements IConcurrentQueue<TData>, not IConcurrentStack<TData>, even though it is a LIFO stack.",
    "SystemDictionary.Add swallows all exceptions and returns false instead of propagating them."
  ],
  "commonMistakes": [
    "Assuming SynchronizedConcurrentDictionary.AddOrReplace returns false for a new key; it always returns true and reports oldElement = newElement for a fresh key.",
    "Calling CycleQueue.Put expecting it to throw when full and cannot grow; it returns false instead.",
    "Calling DataRef.ShowData, ShowKey, Update or Remove on a default or invalid ref throws NullReferenceException; guard with IsValid first.",
    "Expecting CycleQueue indexer to throw ArgumentOutOfRangeException; it throws plain System.Exception, so validate 0 <= id < Count yourself.",
    "Expecting PriorityQueue.Dequeue to throw on an empty queue; it returns default(Data) instead. Use Count or TryPop.",
    "Forgetting CycleQueue rounds capacity up to a power of two, then relying on an exact non-power-of-two capacity.",
    "Using ProducerDelegate<TData> with a reference type that can legitimately be null; null results are treated as 'empty'.",
    "Using SystemConcurrentStack where an IConcurrentStack<TData> parameter is required; it implements IConcurrentQueue<TData>. Use SynchronizedByLockConcurrentStack for a true stack interface.",
    "Using non-concurrent collections (SystemQueue, CycleQueue, PriorityQueue, SystemDictionary) from multiple threads without external locking."
  ],
  "patterns": [
    {
      "goal": "Adapt a Func as an IConsumer and a factory as an IProducer",
      "code": "IConsumer<int> consumer = new ConsumerDelegate<int>(x => x > 0); bool accepted = consumer.Put(5); IProducer<int> producer = new ProducerDelegate<int>(() => 5); bool got = producer.TryPop(out int v);"
    },
    {
      "goal": "Build a bounded lock-free FIFO with graceful overflow handling",
      "code": "var q = new LimitedConcurrentQueue<int>(2); q.Put(1); q.Put(2); if (!q.Put(3)) Console.WriteLine(\"full\"); if (q.TryPop(out int v)) Console.WriteLine(v);"
    },
    {
      "goal": "Compare two IReadOnlyArray instances by content (null-safe)",
      "code": "IReadOnlyArray<int> a = new CycleQueue<int>(4); a.Put(1); a.Put(2); IReadOnlyArray<int> b = new CycleQueue<int>(4); b.Put(1); b.Put(2); bool same = a.EqualByContent(b);"
    },
    {
      "goal": "Consume a priority queue as a min-heap, checking emptiness first",
      "code": "var pq = new PriorityQueue<int, string>(); pq.Enqueue(3, \"c\"); pq.Enqueue(1, \"a\"); if (pq.Count > 0) { string top = pq.Peek(); string first = pq.Dequeue(); }"
    },
    {
      "goal": "Find data in a priority queue and update its key via DataRef, degrading gracefully when absent",
      "code": "var pq = new PriorityQueueEx<int, string>(); pq.Enqueue(1, \"a\"); pq.Enqueue(2, \"b\"); var r = pq.FindData(d => d == \"z\"); if (r.IsValid) r.Update(5); else pq.Enqueue(3, \"z\");"
    },
    {
      "goal": "Iterate a CycleQueue in reverse order and mutate by index",
      "code": "var q = new CycleQueue<int>(8); q.Put(10); q.Put(20); q.Put(30); q[1] = 25; foreach (var x in q.Enumerate(QueueEnumerationOrder.TailToHead)) Console.WriteLine(x);"
    },
    {
      "goal": "Pipe work through a single-reader single-writer queue",
      "code": "var q = new SingleReaderWriterConcurrentQueue<int>(); q.Put(42); if (q.TryPop(out int v)) Console.WriteLine(v);"
    },
    {
      "goal": "Use a concurrent map: add-or-replace and get-and-remove",
      "code": "var m = new SystemConcurrentDictionary<string, int>(); m.Add(\"a\", 1); m.AddOrReplace(\"a\", 2, out int old); if (m.GetAndRemove(\"a\", out int removed)) Console.WriteLine(removed);"
    },
    {
      "goal": "Use a synchronized wrapper around any IQueue with a capacity cap",
      "code": "var inner = new SystemQueue<int>(); var sync = new SynchronizedByLockConcurrentQueue<int>(inner, maxCapacity: 1000); bool ok = sync.Put(1);"
    },
    {
      "goal": "Use an unbounded concurrent queue from many threads with TryPop",
      "code": "var q = new SystemConcurrentQueue<int>(); q.Put(1); q.Put(2); while (q.TryPop(out int v)) Console.WriteLine(v);"
    }
  ],
  "relationships": [
    {
      "from": "ConcurrentConsumerDelegate<TData>",
      "to": "IConcurrentConsumer<TData>",
      "rel": "implements"
    },
    {
      "from": "ConcurrentProducerDelegate<TData>",
      "to": "IConcurrentProducer<TData>",
      "rel": "implements"
    },
    {
      "from": "ConsumerDelegate<TData>",
      "to": "IConsumer<TData>",
      "rel": "implements"
    },
    {
      "from": "CycleQueue<T>",
      "to": "BitMath",
      "rel": "uses NextPow2 in its constructor to round capacity to a power of two"
    },
    {
      "from": "CycleQueue<T>",
      "to": "IArray<T>",
      "rel": "implements"
    },
    {
      "from": "CycleQueue<T>",
      "to": "IQueue<T>",
      "rel": "implements"
    },
    {
      "from": "CycleQueue<T>",
      "to": "QueueEnumerationOrder",
      "rel": "takes as parameter in Enumerate"
    },
    {
      "from": "IArray_Ext",
      "to": "IReadOnlyArray<T>",
      "rel": "extends with EqualByContent"
    },
    {
      "from": "IReadOnlyArray_Enumerators",
      "to": "IReadOnlyArray<TData>",
      "rel": "extends with Enumerate, which returns AsEnumerable<TData>"
    },
    {
      "from": "PriorityQueue<Key, Data>",
      "to": "DataRef",
      "rel": "contains nested DataRef; Enumerator yields DataRef instances"
    },
    {
      "from": "PriorityQueue<Key, Data>",
      "to": "IQueue<KeyValuePair<Key, Data>>",
      "rel": "implements"
    },
    {
      "from": "PriorityQueueEx<Key, Data>",
      "to": "DataRef",
      "rel": "returns from FindData"
    },
    {
      "from": "PriorityQueueEx<Key, Data>",
      "to": "PriorityQueue<Key, Data>",
      "rel": "extends"
    },
    {
      "from": "ProducerDelegate<TData>",
      "to": "IProducer<TData>",
      "rel": "implements"
    },
    {
      "from": "SingleReaderWriterConcurrentQueue<TData>",
      "to": "CycleQueue<TData>",
      "rel": "uses two internal CycleQueue buffers swapped via Interlocked.Exchange"
    },
    {
      "from": "SynchronizedByLockConcurrentQueue<TData>",
      "to": "IQueue<TData>",
      "rel": "takes in constructor and serialises with a lock"
    },
    {
      "from": "SynchronizedByLockConcurrentStack<TData>",
      "to": "IConcurrentStack<TData>",
      "rel": "implements"
    },
    {
      "from": "SynchronizedBySpinLockConcurrentQueue<TData>",
      "to": "IQueue<TData>",
      "rel": "takes in constructor and serialises with a SpinLock"
    },
    {
      "from": "SynchronizedBySpinLockConcurrentStack<TData>",
      "to": "IConcurrentStack<TData>",
      "rel": "implements"
    },
    {
      "from": "SynchronizedConcurrentDictionary<TKey, TData>",
      "to": "IConcurrentMap<TKey, TData>",
      "rel": "implements"
    },
    {
      "from": "SynchronizedConcurrentSet<TData>",
      "to": "IConcurrentSet<TData>",
      "rel": "implements"
    },
    {
      "from": "SystemConcurrentDictionary<TKey, TData>",
      "to": "IConcurrentMap<TKey, TData>",
      "rel": "implements"
    },
    {
      "from": "SystemConcurrentQueue<TData>",
      "to": "IConcurrentQueue<TData>",
      "rel": "implements"
    },
    {
      "from": "SystemConcurrentStack<TData>",
      "to": "IConcurrentQueue<TData>",
      "rel": "implements, misleadingly: it is a LIFO stack but is typed as a queue"
    },
    {
      "from": "SystemConcurrentUnorderedCollection<TData>",
      "to": "IConcurrentUnorderedCollection<TData>",
      "rel": "implements"
    },
    {
      "from": "SystemDictionary<TKey, TData>",
      "to": "IMap<TKey, TData>",
      "rel": "implements"
    },
    {
      "from": "SystemQueue<TData>",
      "to": "IQueue<TData>",
      "rel": "implements"
    },
    {
      "from": "SystemStack<TData>",
      "to": "IStack<TData>",
      "rel": "implements"
    }
  ]
}
