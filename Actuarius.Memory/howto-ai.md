{
  "library": "Actuarius.Memory",
  "meta": {
    "target": "netstandard2.1",
    "lang": 9,
    "nullable": true
  },
  "philosophy": "A memory-management library built around object pooling and reference-counted byte buffers. Pools are modelled as IPool/IPoolSink pairs so callers either release manually or wrap the resource in a PoolableResourceDisposer, and a global MemoryRental.Shared singleton wires standard byte-array, generic-object and collectable pools. Byte arrays use explicit reference counting via MultiRefResource (AddRef/Release) and adapt raw byte[] pools through MultiRefByteArrayFromPool, keeping pooling and sharing two orthogonal concerns; most types live in namespace Actuarius.Memory. ByteArrayPool can optionally recycle its wrapper instances (wrapperCacheCapacity) so warm acquires allocate nothing. Reference-counting diagnostics are opt-in via MultiRefResource.ConstructMonitor, and the *Guard types (MultiRefByteArrayGuard, ByteArrayConcurrentPoolGuard) wrap resources and pools to surface misuse and leaks in tests.",
  "quickPick": [
    {
      "need": "Auto-release a pooled resource at the end of a using block",
      "use": "PoolableResourceDisposer<TResource>"
    },
    {
      "need": "Create a read-only view over a fixed byte[] without reference counting",
      "use": "StaticReadOnlyByteArray"
    },
    {
      "need": "Detect use-after-release or double-release on a ref-counted resource",
      "use": "MultiRefResourceMonitor"
    },
    {
      "need": "Eliminate per-acquire wrapper allocation with a recycling byte-array pool",
      "use": "ByteArrayPool"
    },
    {
      "need": "Get a stable unique int id per closed generic type",
      "use": "TypeToIntStaticMap"
    },
    {
      "need": "High-performance concurrent pool for small objects",
      "use": "SmallObjectBufferedPool<TObject>"
    },
    {
      "need": "Never pool an object type; allocate a fresh instance each time",
      "use": "NoPool<TResource>"
    },
    {
      "need": "Pool raw byte[]/T[] arrays by size (single-threaded)",
      "use": "RawArrayPool<T>"
    },
    {
      "need": "Pool raw byte[]/T[] arrays by size from multiple threads",
      "use": "RawArrayConcurrentPool<T>"
    },
    {
      "need": "Read bytes from a System.IO.Stream into pooled buffers",
      "use": "ByteSourceFromStream"
    },
    {
      "need": "Read bytes sequentially from an in-memory array",
      "use": "ByteSourceFromArray"
    },
    {
      "need": "Reference-count a shared byte array across many owners",
      "use": "MultiRefByteArray"
    },
    {
      "need": "Share a sub-range of a byte array and auto-release the source",
      "use": "MultiRefByteArraySpan"
    },
    {
      "need": "Use a single global entry point wiring all standard pools",
      "use": "MemoryRental"
    },
    {
      "need": "Wrap a byte-array pool to catch misuse and leaks in tests",
      "use": "ByteArrayConcurrentPoolGuard"
    },
    {
      "need": "Write bytes across multiple pooled arrays",
      "use": "ByteSinkToManyArrays<TArray>"
    },
    {
      "need": "Write bytes into a raw byte[] with an optional offset",
      "use": "ByteSinkToArray"
    },
    {
      "need": "Write bytes into an IByteArray",
      "use": "ByteSink"
    }
  ],
  "decisionTree": [
    [
      "What do you need to pool?",
      "Raw byte[]/T[] arrays by size",
      [
        [
          "Concurrency needed?",
          "Yes",
          "RawArrayConcurrentPool<T>"
        ],
        [
          "Concurrency needed?",
          "No",
          "RawArrayPool<T>"
        ]
      ]
    ],
    [
      "What do you need to pool?",
      "Reference-counted IMultiRefByteArray backed by a byte[] pool",
      [
        [
          "Concurrency needed?",
          "Yes",
          "ByteArrayConcurrentPool"
        ],
        [
          "Concurrency needed?",
          "No",
          "ByteArrayPool"
        ],
        [
          "Global wiring preferred?",
          "Yes",
          "MemoryRental"
        ]
      ]
    ],
    [
      "What do you need to pool?",
      "Arbitrary objects",
      [
        [
          "Objects created by a default ctor?",
          "No",
          [
            [
              "High-performance per-thread buffering?",
              "Yes",
              "BufferedPool<TObject>"
            ],
            [
              "High-performance per-thread buffering?",
              "No",
              "DelegatePool<TResource>"
            ]
          ]
        ],
        [
          "Objects created by a default ctor?",
          "Yes",
          [
            [
              "High-performance per-thread buffering?",
              "Yes",
              [
                [
                  "Object is small and frequently pooled?",
                  "Yes",
                  "LargeObjectBufferedPool<TObject>"
                ],
                [
                  "Object is small and frequently pooled?",
                  "No",
                  "SmallObjectBufferedPool<TObject>"
                ]
              ]
            ],
            [
              "High-performance per-thread buffering?",
              "No",
              [
                [
                  "Concurrency needed?",
                  "Yes",
                  "DefaultConcurrentPool<TResource>"
                ],
                [
                  "Concurrency needed?",
                  "No",
                  "DefaultPool<TResource>"
                ]
              ]
            ]
          ]
        ],
        [
          "Objects need a collect/restore lifecycle (Collected/Restored)?",
          "Yes",
          [
            [
              "Objects are ref-counted subclasses of MultiRefCollectableResource<TSelf>?",
              "Yes",
              "CollectablePool"
            ],
            [
              "Objects are ref-counted subclasses of MultiRefCollectableResource<TSelf>?",
              "No",
              "CollectablePool"
            ]
          ]
        ],
        [
          "Never pool, always allocate fresh?",
          "Yes",
          [
            [
              "Created by a factory Func<TResource>?",
              "Yes",
              "DelegateNoPool<TResource>"
            ],
            [
              "Created by a factory Func<TResource>?",
              "No",
              "NoPool<TResource>"
            ]
          ]
        ]
      ]
    ],
    [
      "What do you need to do with bytes?",
      "Read bytes",
      [
        [
          "Source is an array",
          "Yes",
          "ByteSourceFromArray"
        ],
        [
          "Source is a System.IO.Stream",
          "Yes",
          "ByteSourceFromStream"
        ]
      ]
    ],
    [
      "What do you need to do with bytes?",
      "Write bytes",
      [
        [
          "Destination is a raw byte[]",
          "Yes",
          "ByteSinkToArray"
        ],
        [
          "Destination is an IByteArray",
          "Yes",
          "ByteSink"
        ],
        [
          "Destination is a chain of pooled arrays",
          "Yes",
          "ByteSinkToManyArrays<TArray>"
        ]
      ]
    ],
    [
      "What do you need for diagnostics or testing?",
      "Detect misuse of pooled resources",
      [
        [
          "Which resource?",
          "Byte arrays from a pool",
          "ByteArrayConcurrentPoolGuard"
        ],
        [
          "Which resource?",
          "Any ref-counted resource",
          "MultiRefResourceMonitor"
        ]
      ]
    ]
  ],
  "interfaces": [
    {
      "name": "IByteArray",
      "extends": [
        "IReadOnlyByteArray",
        "IArray<byte>"
      ],
      "members": [
        {
          "sig": "Array:byte[] { get; }",
          "desc": "returns the mutable backing byte[]; holds reference only"
        }
      ]
    },
    {
      "name": "IByteSink",
      "extends": [
        "IConsumer<byte>"
      ],
      "members": [
        {
          "sig": "PutMany<TBytes>(TBytes bytes):bool where TBytes : IReadOnlyBytes",
          "desc": "writes all of bytes into the sink; returns false if the full sequence could not be written"
        }
      ]
    },
    {
      "name": "IByteSource",
      "extends": [
        "IProducer<byte>"
      ],
      "members": [
        {
          "sig": "TakeMany(IMultiRefByteArray dst):bool",
          "desc": "reads up to dst.Count bytes into dst; returns false if fewer than dst.Count bytes are available"
        }
      ]
    },
    {
      "name": "ICollectablePool",
      "members": [
        {
          "sig": "Acquire<TResource>():TResource where TResource : class, ICollectableResource<TResource>, new()",
          "desc": "returns a pooled TResource, calling Restored on it, or creates a new one if none is pooled"
        }
      ]
    },
    {
      "name": "ICollectableResource",
      "members": [
        {
          "sig": "Collected():bool",
          "desc": "is called when the object is returned to the pool; returns true to allow reuse, false discards the object"
        }
      ]
    },
    {
      "name": "ICollectableResource<out TSelf>",
      "variance": "out TSelf",
      "extends": [
        "ICollectableResource"
      ],
      "members": [
        {
          "sig": "Restored(IPoolSink<TSelf> pool)",
          "desc": "is called when the object is acquired from the pool; pool is the sink the object must release itself into when done"
        }
      ]
    },
    {
      "name": "IConcurrentPool<TResource, in TParam0>",
      "variance": "in TParam0",
      "extends": [
        "IPool<TResource, TParam0>",
        "IConcurrentPoolSink<TResource>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IPool<TResource, TParam0> and IConcurrentPoolSink; adds no new members"
    },
    {
      "name": "IConcurrentPool<TResource>",
      "extends": [
        "IPool<TResource>",
        "IConcurrentPoolSink<TResource>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IPool and IConcurrentPoolSink; adds no new members"
    },
    {
      "name": "IConcurrentPoolSink<in TResource>",
      "variance": "in TResource",
      "extends": [
        "IPoolSink<TResource>"
      ],
      "isAuxiliary": true,
      "desc": "marker interface for an IPoolSink whose Release is safe to call from any thread; adds no new members"
    },
    {
      "name": "IGenericConcurrentPool",
      "extends": [
        "IGenericPool"
      ],
      "members": [
        {
          "sig": "GetPool<TResource>():IConcurrentPool<TResource> where TResource : class, new()",
          "desc": "returns the per-type concurrent pool for TResource, creating it on first use; thread-safe: thread-safe (lock); O(1)"
        }
      ]
    },
    {
      "name": "IGenericConcurrentPool<in TParam>",
      "variance": "in TParam",
      "extends": [
        "IGenericPool<TParam>"
      ],
      "members": [
        {
          "sig": "ShowTypedPool<TResource>():IConcurrentPool<TResource, TParam>",
          "desc": "returns the typed concurrent pool for TResource keyed by TParam"
        }
      ]
    },
    {
      "name": "IGenericPool",
      "members": [
        {
          "sig": "Acquire<TResource>():(TResource resource, IPoolSink<TResource> poolSink) where TResource : class, new()",
          "desc": "acquires a TResource and returns it together with the sink that must receive it back"
        },
        {
          "sig": "GetPool<TResource>():IPool<TResource> where TResource : class, new()",
          "desc": "returns the per-type pool for TResource, creating it on first use"
        }
      ]
    },
    {
      "name": "IGenericPool<in TParam>",
      "variance": "in TParam",
      "members": [
        {
          "sig": "Acquire<TResource>(TParam param):(TResource resource, IPoolSink<TResource> poolSink)",
          "desc": "acquires a TResource classified by param and returns it with its sink"
        },
        {
          "sig": "ShowTypedPool<TResource>():IPool<TResource, TParam>",
          "desc": "returns the typed pool for TResource keyed by TParam"
        }
      ]
    },
    {
      "name": "IMemoryRental",
      "members": [
        {
          "sig": "BigObjectsPool:IGenericConcurrentPool { get; }",
          "desc": "returns the generic pool tuned for large objects; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "ByteArraysPool:IConcurrentPool<IMultiRefByteArray, int> { get; }",
          "desc": "returns the reference-counted byte-array pool keyed by requested size; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "CollectablePool:ICollectablePool { get; }",
          "desc": "returns the pool for collectable resources; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "GetArrayPool<T>():IConcurrentPool<T[], int>",
          "desc": "returns a cached per-type concurrent array pool for T[]; thread-safe: thread-safe (lock); O(1)"
        },
        {
          "sig": "SharedByteArraysPool:IConcurrentPool<IMultiRefByteArray, int> { get; }",
          "desc": "returns a concurrent byte-array pool whose wrappers are recycled with capacity 1000; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "SmallObjectsPool:IGenericConcurrentPool { get; }",
          "desc": "returns the generic pool tuned for small objects; thread-safe: thread-safe (lock)"
        }
      ]
    },
    {
      "name": "IMultiRefByteArray",
      "extends": [
        "IMultiRefReadOnlyByteArray",
        "IByteArray"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IMultiRefReadOnlyByteArray and IByteArray; adds no new members"
    },
    {
      "name": "IMultiRefReadOnlyByteArray",
      "extends": [
        "IMultiRefReadOnlyBytes",
        "IReadOnlyByteArray"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IMultiRefReadOnlyBytes and IReadOnlyByteArray; adds no new members"
    },
    {
      "name": "IMultiRefReadOnlyBytes",
      "extends": [
        "IMultiRefResource",
        "IReadOnlyBytes"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IMultiRefResource and IReadOnlyBytes; adds no new members"
    },
    {
      "name": "IMultiRefResource",
      "extends": [
        "IReleasableResource"
      ],
      "members": [
        {
          "sig": "AddRef()",
          "desc": "increments the owner count by one; must be balanced with a Release; lock-free; O(1)"
        },
        {
          "sig": "ConstructMonitor(bool collectHistory = true, bool trackLeaks = true, bool traceEnabled = true):IMultiRefResourceMonitor",
          "desc": "creates the diagnostics monitor bound to this resource, or returns the existing live one; must be called explicitly to enable misuse reporting; lock-free; O(1)"
        },
        {
          "sig": "IsAlive:bool { get; }",
          "desc": "returns true while at least one owner holds the object; lock-free; O(1)"
        }
      ]
    },
    {
      "name": "IMultiRefResourceMonitor",
      "members": [
        {
          "sig": "ExportHistory():IReadOnlyList<ResourceUsageRecord>",
          "desc": "returns a snapshot of the recorded reference-count events, oldest first; returns an empty list when history collection is disabled; O(n)"
        },
        {
          "sig": "event Action<string, ResourceUsageErrorType> Error",
          "desc": "raised when misuse is detected; the first argument identifies the owning resource; handlers run synchronously on the reporting thread, except Leak which is raised on a thread-pool thread"
        }
      ]
    },
    {
      "name": "IPool<TResource, in TParam0>",
      "variance": "in TParam0",
      "extends": [
        "IPoolSink<TResource>"
      ],
      "members": [
        {
          "sig": "Acquire(TParam0 param0):TResource",
          "desc": "returns a pooled resource classified by param0, creating a pool for that class on first use; caller takes ownership"
        },
        {
          "sig": "AcquireEx(TParam0 param0):(TResource resource, IPoolSink<TResource> poolSink)",
          "desc": "acquires a resource classified by param0 and returns it together with the sink that must receive it back"
        }
      ]
    },
    {
      "name": "IPool<TResource>",
      "extends": [
        "IPoolSink<TResource>"
      ],
      "members": [
        {
          "sig": "Acquire():TResource",
          "desc": "returns a pooled resource if one is available, otherwise constructs a new one; caller takes ownership"
        }
      ]
    },
    {
      "name": "IPoolRef",
      "isAuxiliary": true,
      "desc": "marker base interface for all pool sinks; adds no members"
    },
    {
      "name": "IPoolSink<in TResource>",
      "variance": "in TResource",
      "extends": [
        "IPoolRef"
      ],
      "members": [
        {
          "sig": "Release(TResource? resource)",
          "desc": "returns resource to the pool for reuse; is a no-op for null; thread-safety depends on the pool"
        }
      ]
    },
    {
      "name": "IReadOnlyByteArray",
      "extends": [
        "IReadOnlyBytes",
        "IReadOnlyArray<byte>"
      ],
      "members": [
        {
          "sig": "Offset:int { get; }",
          "desc": "returns the 0-based index in ReadOnlyArray where the view starts"
        },
        {
          "sig": "ReadOnlyArray:byte[] { get; }",
          "desc": "returns the backing byte[] (read-only by convention); holds reference only"
        }
      ]
    },
    {
      "name": "IReadOnlyBytes",
      "extends": [
        "ICountable"
      ],
      "members": [
        {
          "sig": "CopyTo(byte[] dst, int dstOffset, int srcOffset, int count):bool",
          "desc": "copies count bytes from srcOffset into dst at dstOffset; returns false and leaves dst in an undefined state if the ranges are invalid; O(count)"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns false when the object represents an empty or invalid byte sequence, equivalent to a null array"
        }
      ]
    },
    {
      "name": "IRefCounterView",
      "members": [
        {
          "sig": "RefCount:int { get; }",
          "desc": "returns the current reference count; lock-free; O(1)"
        },
        {
          "sig": "event Action<RefCounterErrorEvent, int> Error",
          "desc": "raised after a failed operation with the failure kind and the count observed at failure"
        },
        {
          "sig": "event Action<int> Updated",
          "desc": "raised after a successful operation with the resulting count"
        }
      ]
    },
    {
      "name": "IReleasableResource",
      "members": [
        {
          "sig": "Release()",
          "desc": "informs the object that ownership by the caller has ended; what it does is type-specific"
        }
      ]
    }
  ],
  "types": [
    {
      "name": "Bucket<TObject>",
      "kind": "class",
      "category": "pool",
      "isAuxiliary": false,
      "desc": "fixed-capacity bucket of pooled objects used by BufferedPool; lives in namespace Actuarius.Memory.ConcurrentBuffered",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(int capacity, Func<TObject> ctor)",
          "desc": "creates a bucket that can hold up to capacity objects, constructing new ones with ctor when needed"
        }
      ],
      "methods": [
        {
          "sig": "LazyFill()",
          "desc": "marks the remaining free slots as constructible on demand; O(1)"
        },
        {
          "sig": "Put(TObject value):bool",
          "desc": "adds value; returns false if the bucket is full; O(1)"
        },
        {
          "sig": "TryPop([MaybeNullWhen(false)] out TObject value):bool",
          "desc": "returns a stored object, constructing one if a virtual slot is reserved; returns false and sets value to default if the bucket is exhausted; O(1)"
        }
      ]
    },
    {
      "name": "BucketSource<TObject>",
      "kind": "class",
      "category": "pool",
      "isAuxiliary": false,
      "desc": "manages full and empty Bucket pools for BufferedPool; lives in namespace Actuarius.Memory.ConcurrentBuffered",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentUnorderedCollection",
      "constructors": [
        {
          "sig": "ctor(int bucketSize, Func<TObject> objectCtor, Func<IConcurrentUnorderedCollection<Bucket<TObject>>> ctor)",
          "desc": "creates a source with full and empty bucket collections built by ctor"
        }
      ],
      "methods": [
        {
          "sig": "GetEmptyBucket():Bucket<TObject>",
          "desc": "returns an empty bucket from the empty pool or constructs a new one; O(1)"
        },
        {
          "sig": "GetFullBucket():Bucket<TObject>",
          "desc": "returns a full bucket from the full pool, lazily filling an empty one if none is available; O(1)"
        },
        {
          "sig": "ReturnEmptyBucket(Bucket<TObject> bucket):bool",
          "desc": "returns an empty bucket to the pool; returns false if the pool rejected it; O(1)"
        },
        {
          "sig": "ReturnFullBucket(Bucket<TObject> bucket):bool",
          "desc": "returns a full bucket to the pool; returns false if the pool rejected it; O(1)"
        }
      ]
    },
    {
      "name": "BufferedPool<TObject>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IConcurrentPool<TObject>"
      ],
      "genericConstraints": "where TObject : class",
      "isAuxiliary": false,
      "desc": "high-performance concurrent object pool using per-thread buckets; threads grab a bucket via atomic exchange and may briefly spin under contention",
      "threadSafety": "lock-free",
      "limitations": "Acquire can throw InvalidOperationException if object construction fails; Release silently drops the object when both buckets are full; a heavily contended pool spins (Thread.Sleep(0)) while waiting for a free bucket",
      "constructors": [
        {
          "sig": "ctor(int bucketSize, int distributionLevel, Func<TObject> acquire)",
          "desc": "creates a pool with the given bucket size and distribution level, building buckets with SystemConcurrentQueue",
          "example": "var pool = new BufferedPool<MyObject>(100, 10, () => new MyObject());"
        },
        {
          "sig": "ctor(int bucketSize, int distributionLevel, Func<TObject> ctor, Func<IConcurrentUnorderedCollection<Bucket<TObject>>> collectionConstructor)",
          "desc": "creates a pool using a custom collection factory for the bucket queues"
        }
      ],
      "methods": [
        {
          "sig": "Acquire():TObject",
          "desc": "returns an object from the current thread's bucket, constructing a new one if needed; throws InvalidOperationException if construction fails; lock-free; O(1)"
        },
        {
          "sig": "Release(TObject? obj)",
          "desc": "returns obj to the current thread's bucket; is a no-op for null; if both buckets are full the object is dropped; lock-free; O(1)"
        }
      ]
    },
    {
      "name": "ByteArrayConcurrentPool",
      "kind": "class",
      "category": "pool",
      "base": "ByteArrayPool",
      "implements": [
        "IConcurrentPool<IMultiRefByteArray, int>"
      ],
      "isAuxiliary": false,
      "desc": "concurrent variant of ByteArrayPool; the underlying RawArrayConcurrentPool is thread-safe",
      "threadSafety": "thread-safe (lock)",
      "constructors": [
        {
          "sig": "ctor(IConcurrentPool<byte[], int> bytesPool)",
          "desc": "creates a byte-array pool over the given raw array pool"
        },
        {
          "sig": "ctor(IConcurrentPool<byte[], int> bytesPool, int wrapperCacheCapacity)",
          "desc": "creates a concurrent byte-array pool over the given raw array pool; when wrapperCacheCapacity is greater than zero released wrappers are kept and revived, making warm acquire allocation-free; values less than or equal to zero disable recycling",
          "remarks": "recycled wrappers reuse object identity, so a stale reference released twice or released after the resource was re-acquired corrupts the current owner's reference count and can return an in-use byte[] to its raw pool"
        }
      ]
    },
    {
      "name": "ByteArrayConcurrentPoolGuard",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IConcurrentPool<IMultiRefByteArray, int>"
      ],
      "isAuxiliary": false,
      "desc": "debug wrapper around an IConcurrentPool<IMultiRefByteArray, int> for tests: hands out MultiRefByteArrayGuard instances and aggregates their misuse reports through Error/ErrorCount",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Release(IMultiRefByteArray) is a no-op; acquired resources must be released via IMultiRefByteArray.Release(); it only ever issues MultiRefByteArrayGuard instances and never returns anything to the wrapped pool",
      "constructors": [
        {
          "sig": "ctor(IConcurrentPool<IMultiRefByteArray, int> pool, bool collectHistory = true, bool traceEnabled = true)",
          "desc": "creates the guard around pool; throws ArgumentNullException if pool is null; collectHistory and traceEnabled configure the per-resource monitors"
        }
      ],
      "properties": [
        {
          "sig": "ErrorCount:int { get; }",
          "desc": "returns the number of misuse reports raised by guards created by this pool; lock-free; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Acquire(int param0):IMultiRefByteArray",
          "desc": "acquires from the wrapped pool and returns a new MultiRefByteArrayGuard with a constructed monitor; caller takes ownership; O(1)"
        },
        {
          "sig": "AcquireEx(int param0):(IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink)",
          "desc": "acquires a guarded resource and returns it with this pool as sink; the sink's Release is a no-op; O(1)"
        },
        {
          "sig": "AssertNoOutstanding()",
          "desc": "reports NoUsageAssertionFail through Error for every acquired guard that was not released; call at the end of a test"
        },
        {
          "sig": "Release(IMultiRefByteArray? resource)",
          "desc": "is a no-op for null or a MultiRefByteArrayGuard; throws InvalidOperationException if resource is any other type"
        }
      ],
      "events": [
        {
          "sig": "event Action<string, ResourceUsageErrorType> Error",
          "desc": "raised whenever a guard created by this pool reports misuse; the first argument identifies the guard"
        }
      ]
    },
    {
      "name": "ByteArrayPool",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IPool<IMultiRefByteArray, int>",
        "IPoolSink<RecyclableMultiRefByteArrayFromPool>"
      ],
      "isAuxiliary": false,
      "desc": "adapts a raw byte[] pool so Acquire returns a reference-counted MultiRefByteArrayFromPool; the wrapped byte[] returns to the raw pool only when the wrapper reference count reaches zero; an optional wrapperCacheCapacity enables wrapper recycling, making warm acquires allocation-free",
      "threadSafety": "not thread-safe",
      "limitations": "Release(IMultiRefByteArray) is a no-op; the resource must be released via its own Release() (ref count) or the raw array is never returned",
      "constructors": [
        {
          "sig": "ctor(IPool<byte[], int> bytesPool)",
          "desc": "creates a byte-array pool over the given raw array pool"
        },
        {
          "sig": "ctor(IPool<byte[], int> bytesPool, int wrapperCacheCapacity)",
          "desc": "creates a byte-array pool over the given raw array pool; when wrapperCacheCapacity is greater than zero released wrappers are kept and revived, making warm acquire allocation-free; values less than or equal to zero disable recycling",
          "remarks": "recycled wrappers reuse object identity, so a stale reference released twice or released after the resource was re-acquired corrupts the current owner's reference count and can return an in-use byte[] to its raw pool"
        }
      ],
      "methods": [
        {
          "sig": "Acquire(int param0):IMultiRefByteArray",
          "desc": "acquires a raw byte[] of length NextPow2(param0) from the raw pool and wraps it in a MultiRefByteArrayFromPool; caller takes ownership of the wrapper; O(1)"
        },
        {
          "sig": "AcquireEx(int param0):(IMultiRefByteArray resource, IPoolSink<IMultiRefByteArray> poolSink)",
          "desc": "acquires a wrapped byte[] and returns it with this pool as sink; the raw byte[] returns to the raw pool only when the wrapper reference count reaches zero"
        },
        {
          "sig": "Release(IMultiRefByteArray? resource)",
          "desc": "is a no-op; the raw byte[] returns to the raw pool only when the wrapper reference count reaches zero"
        }
      ]
    },
    {
      "name": "ByteSink",
      "kind": "struct",
      "category": "sink",
      "implements": [
        "IByteSink"
      ],
      "isAuxiliary": false,
      "desc": "struct byte sink that writes into an IByteArray at a movable position",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IByteArray array)",
          "desc": "creates a sink writing into array starting at position 0"
        }
      ],
      "properties": [
        {
          "sig": "Position:int { get; set; }",
          "desc": "gets or sets the number of bytes written so far; setting moves the write cursor"
        }
      ],
      "methods": [
        {
          "sig": "Put(byte value):bool",
          "desc": "writes value at the current position; returns false if the sink is full; O(1)"
        },
        {
          "sig": "PutMany<TBytes>(TBytes bytes):bool where TBytes : IReadOnlyBytes",
          "desc": "writes all of bytes at the current position; returns false and leaves the position unchanged if they do not fit; O(count)"
        }
      ]
    },
    {
      "name": "ByteSinkToArray",
      "kind": "struct",
      "category": "sink",
      "implements": [
        "IByteSink"
      ],
      "isAuxiliary": false,
      "desc": "struct byte sink that writes into a raw byte[] with an optional offset",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(byte[] array, int offset = 0, int count = -1)",
          "desc": "creates a sink writing into array; count defaults to array.Length when negative"
        }
      ],
      "methods": [
        {
          "sig": "Put(byte value):bool",
          "desc": "writes value while _position + _offset < _count and returns true, else false; the write index ignores the offset, so prefer PutMany with a non-zero offset; O(1)"
        },
        {
          "sig": "PutMany<TBytes>(TBytes bytes):bool where TBytes : IReadOnlyBytes",
          "desc": "writes all of bytes at _position + _offset; returns false if they do not fit; O(count)"
        }
      ]
    },
    {
      "name": "ByteSinkToManyArrays<TArray>",
      "kind": "struct",
      "category": "sink",
      "implements": [
        "IByteSink"
      ],
      "genericConstraints": "where TArray : IByteArray",
      "isAuxiliary": false,
      "desc": "struct byte sink that accumulates bytes across multiple IByteArray blocks pulled from an empty-array producer and pushes full blocks to a filled-array consumer",
      "threadSafety": "not thread-safe",
      "limitations": "Finish publishes only an exactly-full block; a partially-filled trailing block is silently dropped",
      "constructors": [
        {
          "sig": "ctor(IProducer<TArray> emptyArraysProducer, IConsumer<TArray> filledArraysConsumer)",
          "desc": "creates a multi-block sink; empty blocks come from the producer, full blocks go to the consumer"
        }
      ],
      "methods": [
        {
          "sig": "Finish()",
          "desc": "publishes the current block only if it is exactly full; a partially-filled block is dropped"
        },
        {
          "sig": "Put(byte value):bool",
          "desc": "writes value into the current block, acquiring a new block from the producer as needed; returns false if no empty block is available or publishing fails; O(1)"
        },
        {
          "sig": "PutMany<TBytes>(TBytes bytes):bool where TBytes : IReadOnlyBytes",
          "desc": "writes all of bytes across blocks; returns false if blocks run out mid-copy, leaving a partial write; O(count)"
        }
      ]
    },
    {
      "name": "ByteSourceFromArray",
      "kind": "struct",
      "category": "source",
      "implements": [
        "IByteSource"
      ],
      "isAuxiliary": false,
      "desc": "struct byte source that reads sequentially from an IReadOnlyByteArray at a movable position",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IReadOnlyByteArray array)",
          "desc": "creates a source reading from array starting at position 0"
        }
      ],
      "properties": [
        {
          "sig": "Position:int { get; set; }",
          "desc": "gets or sets the number of bytes consumed so far; setting moves the read cursor"
        }
      ],
      "methods": [
        {
          "sig": "TakeMany(IMultiRefByteArray dst):bool",
          "desc": "reads dst.Count bytes into dst and advances the position; returns false if fewer bytes remain; O(count)"
        },
        {
          "sig": "TryPop(out byte value):bool",
          "desc": "returns the next byte and advances; returns false and sets value to 0 at the end; O(1)"
        }
      ]
    },
    {
      "name": "ByteSourceFromStream",
      "kind": "struct",
      "category": "source",
      "implements": [
        "IByteSource"
      ],
      "isAuxiliary": false,
      "desc": "struct byte source that reads sequentially from a System.IO.Stream up to a byte limit",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(Stream stream, int countToRead = -1)",
          "desc": "creates a source reading from stream; countToRead defaults to the remaining stream length; throws NotSupportedException if stream is not seekable and countToRead is -1"
        }
      ],
      "methods": [
        {
          "sig": "TakeMany(IMultiRefByteArray dst):bool",
          "desc": "reads up to dst.Count bytes into dst; returns false if fewer than dst.Count bytes were read; O(count)"
        },
        {
          "sig": "TryPop(out byte value):bool",
          "desc": "returns the next byte while countToRead is positive; returns false and sets value to 0 otherwise; casts ReadByte to byte, so an early EOF yields 255; O(1)"
        }
      ]
    },
    {
      "name": "CollectablePool",
      "kind": "class",
      "category": "pool",
      "implements": [
        "ICollectablePool"
      ],
      "isAuxiliary": false,
      "desc": "pool for ICollectableResource<T> objects that supports a collect/restore lifecycle; each distinct resource type gets its own concurrent queue created by a factory",
      "threadSafety": "thread-safe (lock)",
      "limitations": "supports at most 1000 distinct resource types; the fixed internal constructors array throws IndexOutOfRangeException beyond that",
      "constructors": [
        {
          "sig": "ctor(Func<IConcurrentUnorderedCollection<ICollectableResource>> singleTypePool)",
          "desc": "creates a pool whose per-type queues are built by singleTypePool"
        }
      ],
      "methods": [
        {
          "sig": "Acquire<TResource>():TResource where TResource : class, ICollectableResource<TResource>, new()",
          "desc": "returns a pooled TResource, calling Restored on it, or constructs and restores a new one; throws IndexOutOfRangeException if more than 1000 distinct types are registered; thread-safe: thread-safe (lock); O(1)"
        }
      ]
    },
    {
      "name": "ConcurrentDelegatePool<TResource, TParam>",
      "kind": "class",
      "category": "pool",
      "base": "ConcurrentPool<TResource, TParam>",
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "concrete concurrent pool that classifies resources by TParam into sub-pools and builds each sub-pool via a factory",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentMap",
      "constructors": [
        {
          "sig": "ctor(IConcurrentMap<int, IConcurrentPool<TResource>> table, Func<int, IConcurrentPool<TResource>> poolCtor, Func<TResource, int> classifyByResource, Func<TParam, int> classifyByParameter)",
          "desc": "creates a pool using table for sub-pool storage, poolCtor to build sub-pools, and classifiers for resource and parameter"
        }
      ]
    },
    {
      "name": "ConcurrentDelegatePool<TResource>",
      "kind": "class",
      "category": "pool",
      "base": "ConcurrentPool<TResource>",
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "concrete concurrent pool that creates resources via a Func<TResource> and stores them in a caller-supplied concurrent collection",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentUnorderedCollection",
      "constructors": [
        {
          "sig": "ctor(Func<TResource> resourceCtor, IConcurrentUnorderedCollection<TResource> pool, Func<TResource, bool>? deInitializer)",
          "desc": "creates a pool using resourceCtor for new objects, pool for storage, and an optional deinitializer that can veto reuse"
        }
      ]
    },
    {
      "name": "ConcurrentPool<TResource, TParam>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IConcurrentPool<TResource, TParam>"
      ],
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "abstract concurrent pool keyed by a classification int derived from TParam; subclasses provide CreatePool(int), Classify(TResource) and Classify(TParam)",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentMap",
      "methods": [
        {
          "sig": "Acquire(TParam param):TResource",
          "desc": "classifies param and returns a resource from the corresponding sub-pool, creating it on first use; thread-safety depends on the map; O(1)"
        },
        {
          "sig": "AcquireEx(TParam param):(TResource resource, IPoolSink<TResource> poolSink)",
          "desc": "acquires and returns the resource with its sub-pool as sink; thread-safety depends on the map; O(1)"
        },
        {
          "sig": "Release(TResource? resource)",
          "desc": "classifies resource and returns it to its sub-pool; is a silent no-op if the class has no sub-pool; thread-safety depends on the map; O(1)"
        }
      ]
    },
    {
      "name": "ConcurrentPool<TResource>",
      "kind": "class",
      "category": "pool",
      "base": "Pool<TResource>",
      "implements": [
        "IConcurrentPool<TResource>"
      ],
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "abstract concurrent pool backed by an IConcurrentUnorderedCollection; thread-safety is provided by the wrapped collection",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentUnorderedCollection"
    },
    {
      "name": "DefaultConcurrentPool<TResource>",
      "kind": "class",
      "category": "pool",
      "base": "ConcurrentDelegatePool<TResource>",
      "genericConstraints": "where TResource : class, new()",
      "isAuxiliary": false,
      "desc": "concrete concurrent pool that creates resources with new TResource()",
      "threadSafety": "thread-safety depends on the wrapped IConcurrentUnorderedCollection",
      "constructors": [
        {
          "sig": "ctor(IConcurrentUnorderedCollection<TResource> pool)",
          "desc": "creates a pool storing objects in the given concurrent collection"
        }
      ]
    },
    {
      "name": "DefaultPool<TResource>",
      "kind": "class",
      "category": "pool",
      "base": "DelegatePool<TResource>",
      "genericConstraints": "where TResource : class, new()",
      "isAuxiliary": false,
      "desc": "concrete single-threaded pool that creates resources with new TResource()",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a pool storing objects in a CycleQueue with no deinitializer"
        }
      ]
    },
    {
      "name": "DelegateNoPool<TResource>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IConcurrentPool<TResource>"
      ],
      "isAuxiliary": false,
      "desc": "pool that never pools: Acquire always returns a fresh object from a factory and Release is a no-op",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(Func<TResource> resourceFactory)",
          "desc": "creates a non-pooling pool using resourceFactory for every Acquire"
        }
      ],
      "methods": [
        {
          "sig": "Acquire():TResource",
          "desc": "returns a fresh object from the factory; never reuses instances; O(1)"
        },
        {
          "sig": "Release(TResource? obj)",
          "desc": "is a no-op; objects are never stored; O(1)"
        }
      ]
    },
    {
      "name": "DelegatePool<TResource, TParam1>",
      "kind": "class",
      "category": "pool",
      "base": "Pool<TResource, TParam1>",
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "concrete single-threaded pool that classifies resources by TParam1 into sub-pools and builds each sub-pool via a factory",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IMap<int, IPool<TResource>> table, Func<int, IPool<TResource>> poolCtor, Func<TResource, int> classifyByResource, Func<TParam1, int> classifyByParameter)",
          "desc": "creates a pool using table for sub-pool storage, poolCtor to build sub-pools, and classifiers for resource and parameter"
        }
      ]
    },
    {
      "name": "DelegatePool<TResource>",
      "kind": "class",
      "category": "pool",
      "base": "Pool<TResource>",
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "concrete single-threaded pool that creates resources via a Func<TResource>",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(Func<TResource> resourceCtor, Func<TResource, bool>? deInitializer)",
          "desc": "creates a pool using resourceCtor for new objects and an optional deinitializer that can veto reuse, storing objects in a CycleQueue"
        },
        {
          "sig": "ctor(Func<TResource> resourceCtor, IUnorderedCollection<TResource> pool, Func<TResource, bool>? deInitializer)",
          "desc": "creates a pool using resourceCtor, the given storage collection, and an optional deinitializer"
        }
      ]
    },
    {
      "name": "EmptyByteArray",
      "kind": "class",
      "category": "array",
      "base": "StaticByteArray",
      "isAuxiliary": false,
      "desc": "singleton zero-length StaticByteArray equivalent to new byte[0]; there is no reference counting, IsValid is true and Release and AddRef are no-ops",
      "threadSafety": "lock-free",
      "properties": [
        {
          "sig": "Instance:EmptyByteArray { get; }",
          "desc": "returns the shared zero-length byte array; never release it",
          "example": "IMultiRefByteArray empty = EmptyByteArray.Instance;"
        }
      ]
    },
    {
      "name": "FixedLengthRawArrayConcurrentPool<T>",
      "kind": "class",
      "category": "pool",
      "base": "ConcurrentPool<T[]>",
      "isAuxiliary": false,
      "desc": "concurrent pool of T[] arrays of a fixed length, backed by a LimitedConcurrentQueue of the given capacity",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(int length, int capacity, Func<T[], bool>? deInitializer)",
          "desc": "creates a pool of T[length] arrays with the given storage capacity and optional deinitializer"
        }
      ],
      "methods": [
        {
          "sig": "Acquire():T[]",
          "desc": "returns a pooled T[length] or allocates a new one; lock-free; O(1)"
        },
        {
          "sig": "Release(T[]? resource)",
          "desc": "returns the array to the pool when the deinitializer allows; null is ignored; lock-free; O(1)"
        }
      ]
    },
    {
      "name": "FixedLengthRawArrayPool<T>",
      "kind": "class",
      "category": "pool",
      "base": "Pool<T[]>",
      "isAuxiliary": false,
      "desc": "single-threaded pool of T[] arrays of a fixed length",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(int length, Func<T[], bool>? deInitializer)",
          "desc": "creates a pool of T[length] arrays with an optional deinitializer"
        }
      ],
      "methods": [
        {
          "sig": "Acquire():T[]",
          "desc": "returns a pooled T[length] or allocates a new one; O(1)"
        },
        {
          "sig": "Release(T[]? resource)",
          "desc": "returns the array to the pool when the deinitializer allows; null is ignored; O(1)"
        }
      ]
    },
    {
      "name": "GenericConcurrentPool",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IGenericConcurrentPool"
      ],
      "isAuxiliary": false,
      "desc": "generic object pool that stores a BufferedPool per closed type TResource inside a caller-supplied type-id map",
      "threadSafety": "thread-safe (lock)",
      "limitations": "GetPool throws Exception if the type-id map cannot accept a new entry; type ids come from TypeToIntStaticMap and are process-wide",
      "constructors": [
        {
          "sig": "ctor(IConcurrentMap<int, object> typeIdMap, int bucketSize, int distributionLevel)",
          "desc": "creates a pool storing per-type BufferedPool instances in typeIdMap"
        }
      ],
      "methods": [
        {
          "sig": "Acquire<TResource>():(TResource resource, IPoolSink<TResource> poolSink) where TResource : class, new()",
          "desc": "acquires a TResource from its per-type pool and returns it with the pool as sink; thread-safe: thread-safe (lock); O(1)"
        },
        {
          "sig": "GetPool<TResource>():IConcurrentPool<TResource> where TResource : class, new()",
          "desc": "returns the per-type BufferedPool, creating it on first use; throws Exception if the type map cannot accept the new entry; thread-safe: thread-safe (lock); O(1)"
        }
      ]
    },
    {
      "name": "LargeObjectBufferedPool<TObject>",
      "kind": "class",
      "category": "pool",
      "base": "BufferedPool<TObject>",
      "genericConstraints": "where TObject : class",
      "isAuxiliary": false,
      "desc": "BufferedPool pre-tuned for large objects with bucketSize 20 and distributionLevel 3",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(Func<TObject> ctor)",
          "desc": "creates a large-object pool using ctor for new objects"
        }
      ]
    },
    {
      "name": "MemoryRental",
      "kind": "class",
      "category": "entry-point",
      "implements": [
        "IMemoryRental"
      ],
      "isAuxiliary": false,
      "desc": "wires the standard pools: ByteArraysPool and SharedByteArraysPool (wrapper recycling capacity 1000, sharing one RawArrayConcurrentPool<byte>), a CollectablePool backed by LimitedConcurrentQueue, and GenericConcurrentPool instances for small (100, 10) and large (10, 2) objects",
      "threadSafety": "thread-safe (lock)",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a fully wired pool set; use MemoryRental.Shared for a process-wide singleton"
        }
      ],
      "properties": [
        {
          "sig": "BigObjectsPool:IGenericConcurrentPool { get; }",
          "desc": "returns the generic pool tuned for large objects; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "ByteArraysPool:IConcurrentPool<IMultiRefByteArray, int> { get; }",
          "desc": "returns the reference-counted byte-array pool keyed by requested size; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "CollectablePool:ICollectablePool { get; }",
          "desc": "returns the pool for collectable resources; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "Shared:IMemoryRental { get; }",
          "desc": "returns the process-wide singleton wiring all standard pools",
          "example": "var pools = MemoryRental.Shared;"
        },
        {
          "sig": "SharedByteArraysPool:IConcurrentPool<IMultiRefByteArray, int> { get; }",
          "desc": "returns a reference-counted byte-array pool with wrapper recycling enabled (capacity 1000), eliminating per-acquire wrapper allocation on warm acquires; thread-safe: thread-safe (lock)"
        },
        {
          "sig": "SmallObjectsPool:IGenericConcurrentPool { get; }",
          "desc": "returns the generic pool tuned for small objects; thread-safe: thread-safe (lock)"
        }
      ],
      "methods": [
        {
          "sig": "GetArrayPool<T>():IConcurrentPool<T[], int>",
          "desc": "returns a cached per-type array pool whose capacity depends on size (1000 for up to 1KiB, 100 up to 10KiB, 10 up to 128KiB, 1 up to 1MiB, 0 above 1MiB); thread-safe: thread-safe (lock); O(1)"
        }
      ]
    },
    {
      "name": "MultiRefByteArray",
      "kind": "class",
      "category": "array",
      "base": "MultiRefResource",
      "implements": [
        "IMultiRefByteArray"
      ],
      "isAuxiliary": false,
      "desc": "reference-counted view over a byte[] with an offset and length; the backing array is nulled on Release",
      "threadSafety": "lock-free",
      "limitations": "the constructor does not validate offset/count against the array length; out-of-range access throws IndexOutOfRangeException at use time; indexing after Release throws NullReferenceException",
      "constructors": [
        {
          "sig": "ctor(byte[] array, int offset = 0, int count = -1)",
          "desc": "creates a ref-counted view of array starting at offset; count defaults to array.Length - offset when negative; the range is not validated",
          "example": "var arr = new MultiRefByteArray(new byte[] { 1, 2, 3, 4, 5 }, 1, 3);"
        }
      ],
      "properties": [
        {
          "sig": "Array:byte[] { get; }",
          "desc": "returns the mutable backing byte[]; is null after Release; holds reference only"
        },
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of bytes in the view; O(1)"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns true while the backing array is non-null, false after Release; lock-free; O(1)"
        },
        {
          "sig": "Offset:int { get; }",
          "desc": "returns the 0-based index in the backing array where the view starts; O(1)"
        },
        {
          "sig": "ReadOnlyArray:byte[] { get; }",
          "desc": "returns the backing byte[] (read-only by convention); is null after Release"
        }
      ],
      "indexers": [
        {
          "sig": "this[int id]:byte { get; set; }",
          "desc": "gets or sets the byte at logical index id, physically _array[Offset + id]; throws IndexOutOfRangeException if the index leaves the backing array; throws NullReferenceException if accessed after Release",
          "example": "byte b = arr[0]; arr[0] = 99;"
        }
      ],
      "methods": [
        {
          "sig": "CopyTo(byte[] dst, int dstOffset, int srcOffset, int count):bool",
          "desc": "copies count bytes from srcOffset into dst at dstOffset; returns false if the object is released or any range is invalid; O(count)"
        }
      ]
    },
    {
      "name": "MultiRefByteArrayFromPool",
      "kind": "class",
      "category": "array",
      "base": "MultiRefByteArray",
      "isAuxiliary": false,
      "desc": "reference-counted byte array that returns its backing byte[] to an IPoolSink<byte[]> when the reference count reaches zero",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(IPoolSink<byte[]> pool, byte[] array, int offset, int count)",
          "desc": "creates a ref-counted view that returns array to pool when its count reaches zero"
        }
      ]
    },
    {
      "name": "MultiRefByteArrayGuard",
      "kind": "class",
      "category": "array",
      "base": "MultiRefResource",
      "implements": [
        "IMultiRefByteArray"
      ],
      "isAuxiliary": false,
      "desc": "debug wrapper around an IMultiRefByteArray that owns the wrapped resource's single reference and releases it when its own count reaches zero; accesses throw ObjectDisposedException after release",
      "threadSafety": "lock-free",
      "limitations": "assumes sole ownership of the wrapped resource, so wrap only a freshly acquired single reference; after Release the wrapper is permanently disposed and all members throw ObjectDisposedException",
      "constructors": [
        {
          "sig": "ctor(IMultiRefByteArray core)",
          "desc": "wraps core and takes ownership of its single reference; the wrapped resource is released when the guard's count reaches zero"
        }
      ],
      "properties": [
        {
          "sig": "Array:byte[] { get; }",
          "desc": "returns the wrapped backing byte[]; throws ObjectDisposedException if the guard was released"
        },
        {
          "sig": "Count:int { get; }",
          "desc": "returns the wrapped byte count; throws ObjectDisposedException if the guard was released; O(1)"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns the wrapped validity, or false when the guard was released; O(1)"
        },
        {
          "sig": "Offset:int { get; }",
          "desc": "returns the wrapped offset; throws ObjectDisposedException if the guard was released; O(1)"
        },
        {
          "sig": "ReadOnlyArray:byte[] { get; }",
          "desc": "returns the wrapped backing byte[]; throws ObjectDisposedException if the guard was released"
        }
      ],
      "indexers": [
        {
          "sig": "this[int id]:byte { get; set; }",
          "desc": "gets or sets the wrapped byte at logical index id; throws ObjectDisposedException if the guard was released"
        }
      ],
      "methods": [
        {
          "sig": "CopyTo(byte[] dst, int dstOffset, int srcOffset, int count):bool",
          "desc": "copies count bytes from the wrapped array into dst; throws ObjectDisposedException if the guard was released; O(count)"
        }
      ]
    },
    {
      "name": "MultiRefByteArraySpan",
      "kind": "class",
      "category": "array",
      "base": "MultiRefByteArray",
      "isAuxiliary": false,
      "desc": "reference-counted sub-range view of an IMultiRefByteArray; acquires a reference on the source and releases it on its own Release",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(IMultiRefByteArray source, int offset, int count)",
          "desc": "creates a view of source[offset..offset+count) and acquires an extra reference on source; throws IndexOutOfRangeException if the range is invalid",
          "example": "var span = new MultiRefByteArraySpan(source, 2, 5);"
        }
      ]
    },
    {
      "name": "MultiRefCollectableResource<TSelf>",
      "kind": "class",
      "category": "array",
      "base": "MultiRefResource",
      "implements": [
        "ICollectableResource<TSelf>"
      ],
      "genericConstraints": "where TSelf : MultiRefCollectableResource<TSelf>",
      "isAuxiliary": false,
      "desc": "abstract base for ref-counted resources pooled by CollectablePool; subclasses override protected OnCollected() (called when returned) and protected OnRestored() (called after revive); the ref counter starts at 0 and is revived by Restored",
      "threadSafety": "lock-free",
      "limitations": "must be obtained through ICollectablePool.Acquire<T> because the ref counter is only initialised by Restored; Restored throws InvalidOperationException if the resource still has owners (Revive fails)"
    },
    {
      "name": "MultiRefResource",
      "kind": "class",
      "category": "refcount",
      "implements": [
        "IMultiRefResource"
      ],
      "isAuxiliary": false,
      "desc": "abstract base for reference-counted resources; subclasses override protected OnReleased() which fires when the count reaches zero; ref counting is pure production code, while diagnostics live in MultiRefResourceMonitor and are only active after ConstructMonitor() is called",
      "threadSafety": "lock-free",
      "limitations": "manual ref counting: every AddRef must be balanced by a Release, otherwise the object leaks or is used after release; without a constructed monitor misuse is not asserted",
      "properties": [
        {
          "sig": "RefCount:IRefCounterView { get; }",
          "desc": "returns the underlying counter view; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "AddRef()",
          "desc": "increments the owner count by one; lock-free; O(1)"
        },
        {
          "sig": "ConstructMonitor(bool collectHistory = true, bool trackLeaks = true, bool traceEnabled = true):MultiRefResourceMonitor",
          "desc": "creates the diagnostics monitor bound to this resource, or returns the existing live one (a detached monitor is replaced); must be called explicitly to enable misuse reporting; lock-free; O(1)"
        },
        {
          "sig": "IsAlive:bool { get; }",
          "desc": "returns true while the reference count is greater than 0; lock-free; O(1)"
        },
        {
          "sig": "Release()",
          "desc": "decrements the owner count; calls OnReleased() when it reaches 0; lock-free; O(1)"
        },
        {
          "sig": "ToString():string",
          "desc": "returns '{TypeName}: RefCount:N'; O(1)"
        }
      ]
    },
    {
      "name": "MultiRefResourceMonitor",
      "kind": "class",
      "category": "refcount",
      "implements": [
        "IMultiRefResourceMonitor"
      ],
      "isAuxiliary": false,
      "desc": "diagnostics monitor for a single MultiRefResource; subscribes to IRefCounterView.Error/Updated, records reference-count events into a capped history and raises the Error event when misuse is detected; when collectHistory is set each recorded event also carries a StackTrace unless TraceEnabled is false",
      "threadSafety": "thread-safe (lock for history)",
      "limitations": "only observes misuse after MultiRefResource.ConstructMonitor() is called; if an external owner keeps holding the monitor it cannot be collected, so leak detection is suppressed; the history holds at most the most recent 100 events",
      "constructors": [
        {
          "sig": "ctor(IRefCounterView counter, string ownerName, bool collectHistory = true, bool trackLeaks = true, bool traceEnabled = true)",
          "desc": "binds the monitor to counter and starts observing its Error/Updated events; ownerName is reported with each error; collectHistory enables event recording, trackLeaks enables finalizer leak reports, traceEnabled makes recorded events capture a StackTrace"
        }
      ],
      "properties": [
        {
          "sig": "Counter:IRefCounterView { get; }",
          "desc": "returns the observed counter; O(1)"
        },
        {
          "sig": "CollectHistory:bool { get; }",
          "desc": "true when reference-count events are recorded into the history"
        },
        {
          "sig": "TrackLeaks:bool { get; }",
          "desc": "true when the finalizer raises Error with Leak while the resource still has owners"
        },
        {
          "sig": "TraceEnabled:bool { get; }",
          "desc": "true when recorded events include a captured StackTrace"
        },
        {
          "sig": "event Error:Action<string, ResourceUsageErrorType>",
          "desc": "raised when misuse is detected; the first argument identifies the owning resource; handlers run synchronously on the reporting thread, except Leak which is raised on a thread-pool thread"
        }
      ],
      "methods": [
        {
          "sig": "ExportHistory():IReadOnlyList<ResourceUsageRecord>",
          "desc": "returns a snapshot of the recorded reference-count history, oldest first"
        },
        {
          "sig": "Clear()",
          "desc": "drains the recorded history; reports read it without draining, so call this to drop accumulated records"
        },
        {
          "sig": "Detach()",
          "desc": "unsubscribes from the counter and suppresses ReportLeak/AssertNoUsage; the monitor records nothing afterwards"
        },
        {
          "sig": "AssertNoUsage()",
          "desc": "raises Error with NoUsageAssertionFail if the counter is still alive"
        }
      ]
    },
    {
      "name": "MultiRefResourceVoidMonitor",
      "kind": "class",
      "category": "refcount",
      "implements": [
        "IMultiRefResourceMonitor"
      ],
      "isAuxiliary": false,
      "desc": "no-op IMultiRefResourceMonitor singleton returned by resources that cannot report misuse, such as StaticReadOnlyByteArray",
      "threadSafety": "lock-free",
      "properties": [
        {
          "sig": "Instance:MultiRefResourceVoidMonitor { get; }",
          "desc": "returns the shared no-op monitor"
        }
      ],
      "methods": [
        {
          "sig": "ExportHistory():IReadOnlyList<ResourceUsageRecord>",
          "desc": "always returns an empty list"
        }
      ],
      "events": [
        {
          "sig": "event Action<string, ResourceUsageErrorType> Error",
          "desc": "declared but never raised"
        }
      ]
    },
    {
      "name": "NoPool<TResource>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IConcurrentPool<TResource>"
      ],
      "genericConstraints": "where TResource : class, new()",
      "isAuxiliary": false,
      "desc": "pool that never pools: Acquire always returns new TResource() and Release is a no-op",
      "threadSafety": "lock-free",
      "properties": [
        {
          "sig": "Instance:NoPool<TResource> { get; }",
          "desc": "returns the shared non-pooling singleton"
        }
      ],
      "methods": [
        {
          "sig": "Acquire():TResource",
          "desc": "returns a new TResource(); never reuses instances; O(1)"
        },
        {
          "sig": "Release(TResource? obj)",
          "desc": "is a no-op; objects are never stored; O(1)"
        }
      ]
    },
    {
      "name": "Pool<TResource, TParam0>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IPool<TResource, TParam0>"
      ],
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "abstract single-threaded pool keyed by a classification int derived from TParam0; subclasses provide CreatePool(int), Classify(TResource) and Classify(TParam0)",
      "threadSafety": "not thread-safe",
      "methods": [
        {
          "sig": "Acquire(TParam0 param0):TResource",
          "desc": "classifies param0 and returns a resource from the corresponding sub-pool, creating it on first use; not thread-safe; O(1)"
        },
        {
          "sig": "AcquireEx(TParam0 param0):(TResource resource, IPoolSink<TResource> poolSink)",
          "desc": "acquires and returns the resource with its sub-pool as sink; not thread-safe; O(1)"
        },
        {
          "sig": "Release(TResource? resource)",
          "desc": "classifies resource and returns it to its sub-pool; is a silent no-op if the class has no sub-pool; not thread-safe; O(1)"
        }
      ]
    },
    {
      "name": "Pool<TResource>",
      "kind": "class",
      "category": "pool",
      "implements": [
        "IPool<TResource>"
      ],
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "abstract base for single-threaded object pools; subclasses provide the protected Constructor() hook",
      "threadSafety": "not thread-safe",
      "methods": [
        {
          "sig": "Acquire():TResource",
          "desc": "returns a pooled resource or constructs a new one via Constructor(); not thread-safe; O(1)"
        },
        {
          "sig": "Release(TResource? resource)",
          "desc": "returns resource to the pool when the optional deinitializer returns true (or is absent); null is ignored; not thread-safe; O(1)"
        }
      ]
    },
    {
      "name": "PoolableResourceDisposer<TResource>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IDisposable"
      ],
      "genericConstraints": "where TResource : class",
      "isAuxiliary": false,
      "desc": "struct guard returned by the AcquireAsDisposable extensions; invokes an onDispose callback and returns the resource to its pool exactly once",
      "threadSafety": "thread-safe (lock)",
      "constructors": [
        {
          "sig": "ctor(TResource resource, IPoolSink<TResource> sink, Action<TResource> onDispose)",
          "desc": "creates a guard that will invoke onDispose and sink.Release exactly once when disposed"
        }
      ],
      "properties": [
        {
          "sig": "Resource:TResource { get; }",
          "desc": "returns the wrapped resource; throws NullReferenceException if the disposer was already disposed"
        }
      ],
      "methods": [
        {
          "sig": "Dispose()",
          "desc": "atomically invokes onDispose(resource) then sink.Release(resource) and nulls the held reference so a second Dispose is a no-op"
        }
      ]
    },
    {
      "name": "RawArrayConcurrentPool<T>",
      "kind": "class",
      "category": "pool",
      "base": "ConcurrentPool<T[], int>",
      "isAuxiliary": false,
      "desc": "concurrent pool of raw T[] arrays classified by size into buckets; sizes are rounded up to the next power of two",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Acquire returns arrays of NextPow2(size), which is larger than requested when size is not a power of two",
      "constructors": [
        {
          "sig": "ctor(ArrayPoolCapacityDelegate capacityDelegate)",
          "desc": "creates a pool whose per-bucket capacity is computed by capacityDelegate; a capacity of 0 disables pooling for that size",
          "example": "var pool = new RawArrayConcurrentPool<byte>(size => size <= 1024 ? 100 : 0);"
        },
        {
          "sig": "ctor(int bucketCapacity, Func<T[], bool>? deInitializer)",
          "desc": "creates a pool with a fixed bucket capacity for all sizes and an optional deinitializer"
        }
      ],
      "methods": [
        {
          "sig": "Acquire(int param0):T[]",
          "desc": "returns a pooled array of length NextPow2(param0), at least param0; thread-safety depends on the backing map; O(1)",
          "example": "byte[] buf = pool.Acquire(300); // byte[512]"
        },
        {
          "sig": "Release(T[]? resource)",
          "desc": "returns the array to the bucket matching its exact length, or discards it when that size is not pooled; null is ignored; thread-safety depends on the backing map; O(1)"
        }
      ]
    },
    {
      "name": "RawArrayPool<T>",
      "kind": "class",
      "category": "pool",
      "base": "Pool<T[], int>",
      "isAuxiliary": false,
      "desc": "single-threaded pool of raw T[] arrays classified by size into buckets; sizes are rounded up to the next power of two",
      "threadSafety": "not thread-safe",
      "limitations": "Acquire returns arrays of NextPow2(size), which is larger than requested when size is not a power of two",
      "constructors": [
        {
          "sig": "ctor(Func<T[], bool>? deInitializer)",
          "desc": "creates a pool with an optional deinitializer that can veto reuse"
        }
      ],
      "methods": [
        {
          "sig": "Acquire(int param0):T[]",
          "desc": "returns a pooled array of length NextPow2(param0), at least param0; O(1)"
        },
        {
          "sig": "Release(T[]? resource)",
          "desc": "returns the array to the bucket matching its exact length, or discards it when that size is not pooled; null is ignored; O(1)"
        }
      ]
    },
    {
      "name": "RecyclableMultiRefByteArrayFromPool",
      "kind": "class",
      "category": "array",
      "base": "MultiRefByteArrayFromPool",
      "isAuxiliary": true,
      "desc": "revivable MultiRefByteArrayFromPool returned by ByteArrayPool.Acquire/AcquireEx when wrapperCacheCapacity is greater than zero; after release it returns itself to the pool's wrapper store for reuse instead of being dropped"
    },
    {
      "name": "RefCounter",
      "kind": "class",
      "category": "refcount",
      "implements": [
        "IRefCounterView"
      ],
      "isAuxiliary": false,
      "desc": "lock-free reference counter backed by Interlocked; used internally by MultiRefResource; lives in namespace Actuarius.Memory.Internal",
      "threadSafety": "lock-free",
      "limitations": "AddRef is valid only for a caller that already holds a reference; Release does not clamp at zero so repeated releases drive the count negative; integer overflow is not guarded",
      "constructors": [
        {
          "sig": "ctor(bool revivable)",
          "desc": "creates a counter; revivable true starts at 0 and allows Revive, revivable false starts at 1 and makes Revive always fail"
        }
      ],
      "properties": [
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns true while the count is greater than 0; lock-free; O(1)"
        },
        {
          "sig": "RefCount:int { get; }",
          "desc": "returns the current reference count; lock-free; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "AddRef():int",
          "desc": "increments the count via Interlocked and returns the new count; returns 0 and leaves the count unchanged if the count is not greater than 0; lock-free; O(1)"
        },
        {
          "sig": "Release():int",
          "desc": "decrements the count and returns the new value; returns -1 on underflow and the count is not clamped; lock-free; O(1)"
        },
        {
          "sig": "Revive():bool",
          "desc": "resets the count from 0 to 1 and returns true; returns false if the counter is not revivable or the count is not 0; lock-free; O(1)"
        },
        {
          "sig": "ToString():string",
          "desc": "returns 'RefCount:' followed by the current count; O(1)"
        }
      ],
      "events": [
        {
          "sig": "event Action<RefCounterErrorEvent, int> Error",
          "desc": "raised after a failed operation with the failure kind and the count observed at failure"
        },
        {
          "sig": "event Action<int> Updated",
          "desc": "raised after a successful operation with the resulting count"
        }
      ]
    },
    {
      "name": "RefCounterErrorEvent",
      "kind": "enum",
      "category": "refcount",
      "isAuxiliary": true,
      "desc": "FailedToIncrement=0, FailedToDecrement=1, FailedToRevive=2"
    },
    {
      "name": "ReleasableResourceDisposer<TResource>",
      "kind": "struct",
      "category": "adapter",
      "implements": [
        "IDisposable"
      ],
      "genericConstraints": "where TResource : class, IReleasableResource",
      "isAuxiliary": false,
      "desc": "struct guard nested in IReleasableResource_Ext and returned by AsDisposable; calls Resource.Release() exactly once when disposed",
      "threadSafety": "thread-safe (lock)",
      "constructors": [
        {
          "sig": "ctor(TResource resource)",
          "desc": "creates a guard that calls resource.Release() once when disposed"
        }
      ],
      "properties": [
        {
          "sig": "Resource:TResource { get; }",
          "desc": "returns the wrapped resource; throws NullReferenceException if the disposer was already disposed"
        }
      ],
      "methods": [
        {
          "sig": "Dispose()",
          "desc": "atomically calls Release() on the wrapped resource and nulls the held reference so a second Dispose is a no-op"
        }
      ]
    },
    {
      "name": "ResourceUsageErrorType",
      "kind": "enum",
      "category": "refcount",
      "isAuxiliary": true,
      "desc": "AddRefOfReleasedObject=0, ReleaseOfReleasedObject=1, WrongReviveUsage=2, Leak=3, NoUsageAssertionFail=4, Unknown=5"
    },
    {
      "name": "ResourceUsageRecord",
      "kind": "struct",
      "category": "refcount",
      "isAuxiliary": false,
      "desc": "a single recorded reference-count event exported by MultiRefResourceMonitor.ExportHistory",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(string action, int count, bool isError, StackTrace? stack)",
          "desc": "creates a record of the named operation with the observed count, an error flag and an optional captured stack"
        }
      ],
      "properties": [
        {
          "sig": "Action:string { get; }",
          "desc": "returns the inferred operation name, such as AddRef, Release or ReleaseOfReleasedObject"
        },
        {
          "sig": "Count:int { get; }",
          "desc": "returns the reference count observed for the event"
        },
        {
          "sig": "IsError:bool { get; }",
          "desc": "returns true when the event was a failure"
        },
        {
          "sig": "Stack:StackTrace? { get; }",
          "desc": "returns the captured stack trace, or null when stack tracing is disabled"
        }
      ],
      "methods": [
        {
          "sig": "ToString():string",
          "desc": "returns two leading spaces, or '! ' for an error, followed by the action and count; O(1)"
        }
      ]
    },
    {
      "name": "SmallObjectBufferedPool<TObject>",
      "kind": "class",
      "category": "pool",
      "base": "BufferedPool<TObject>",
      "genericConstraints": "where TObject : class",
      "isAuxiliary": false,
      "desc": "BufferedPool pre-tuned for small objects with bucketSize 100 and distributionLevel 10",
      "threadSafety": "lock-free",
      "constructors": [
        {
          "sig": "ctor(Func<TObject> ctor)",
          "desc": "creates a small-object pool using ctor for new objects"
        }
      ]
    },
    {
      "name": "StaticByteArray",
      "kind": "class",
      "category": "array",
      "base": "StaticReadOnlyByteArray",
      "implements": [
        "IMultiRefByteArray"
      ],
      "isAuxiliary": false,
      "desc": "writable wrapper around a fixed byte[] with no reference counting; Release and AddRef are no-ops and IsAlive is always true",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(byte[] array, int offset = 0, int length = -1)",
          "desc": "creates a writable view of array; length defaults to array.Length - offset; throws ArgumentOutOfRangeException if the range is invalid"
        }
      ],
      "properties": [
        {
          "sig": "Array:byte[] { get; }",
          "desc": "returns the mutable backing byte[]; holds reference only"
        }
      ],
      "indexers": [
        {
          "sig": "this[int id]:byte { get; set; }",
          "desc": "gets or sets the byte at logical index id; throws IndexOutOfRangeException if the index leaves the backing array"
        }
      ]
    },
    {
      "name": "StaticReadOnlyByteArray",
      "kind": "class",
      "category": "array",
      "implements": [
        "IMultiRefReadOnlyByteArray"
      ],
      "isAuxiliary": false,
      "desc": "read-only wrapper around a fixed byte[] with no reference counting; Release and AddRef are no-ops and IsAlive is always true",
      "threadSafety": "not thread-safe",
      "limitations": "not suitable when shared ownership must be tracked; use MultiRefByteArray for ref counting",
      "constructors": [
        {
          "sig": "ctor(byte[] array)",
          "desc": "creates a read-only view over the whole array"
        },
        {
          "sig": "ctor(byte[] array, int offset = 0, int length = -1)",
          "desc": "creates a read-only view of array[offset..offset+length); length defaults to array.Length - offset; throws ArgumentOutOfRangeException if the range is invalid"
        }
      ],
      "properties": [
        {
          "sig": "Count:int { get; }",
          "desc": "returns the number of bytes in the view; O(1)"
        },
        {
          "sig": "IsAlive:bool { get; }",
          "desc": "always returns true; there is no reference counting; O(1)"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "always returns true; O(1)"
        },
        {
          "sig": "Offset:int { get; }",
          "desc": "returns the 0-based index in the backing array where the view starts; O(1)"
        },
        {
          "sig": "ReadOnlyArray:byte[] { get; }",
          "desc": "returns the backing byte[] (read-only by convention); holds reference only"
        }
      ],
      "indexers": [
        {
          "sig": "this[int id]:byte { get; }",
          "desc": "returns the byte at logical index id; throws IndexOutOfRangeException if the index leaves the backing array"
        }
      ],
      "methods": [
        {
          "sig": "AddRef()",
          "desc": "is a no-op because there is no reference counting; O(1)"
        },
        {
          "sig": "ConstructMonitor(bool collectHistory = true, bool trackLeaks = true, bool traceEnabled = true):IMultiRefResourceMonitor",
          "desc": "ignores the arguments and returns the shared MultiRefResourceVoidMonitor.Instance; static arrays have no reference counting to diagnose"
        },
        {
          "sig": "CopyTo(byte[] dst, int dstOffset, int srcOffset, int count):bool",
          "desc": "copies count bytes into dst; returns false if the ranges are invalid; O(count)"
        },
        {
          "sig": "Release()",
          "desc": "is a no-op because there is no reference counting; O(1)"
        }
      ]
    },
    {
      "name": "TypeToIntStaticMap",
      "kind": "staticClass",
      "category": "utility",
      "isAuxiliary": false,
      "desc": "assigns a unique, stable int id per closed generic type at runtime",
      "methods": [
        {
          "sig": "GetTypeId<T>():int",
          "desc": "returns the unique id for type T, assigned on first use; thread-safe: lock-free; O(1)"
        }
      ]
    },
    {
      "name": "VoidByteArray",
      "kind": "class",
      "category": "array",
      "base": "StaticByteArray",
      "isAuxiliary": false,
      "desc": "singleton StaticByteArray equivalent to a null byte[]; IsValid is false and the inherited CopyTo always returns false",
      "threadSafety": "lock-free",
      "properties": [
        {
          "sig": "Instance:VoidByteArray { get; }",
          "desc": "returns the shared invalid byte array, typed as VoidByteArray",
          "example": "IMultiRefByteArray invalid = VoidByteArray.Instance;"
        }
      ]
    }
  ],
  "delegates": [
    {
      "name": "ArrayPoolCapacityDelegate",
      "sig": "int ArrayPoolCapacityDelegate(int arraySize)",
      "desc": "returns the bucket capacity for arrays of the given requested size; a value of 0 disables pooling for that size so RawArrayConcurrentPool falls back to allocating a new array on every Acquire; nested public delegate of RawArrayConcurrentPool"
    }
  ],
  "extensions": [
    {
      "for": "IByteArray",
      "members": [
        {
          "sig": "CopyFrom(this IByteArray dst, byte[] src, int srcOffset, int dstOffset, int count):bool",
          "desc": "copies count bytes from src[srcOffset] into dst.Array[dstOffset] and returns true; throws ArgumentException if the ranges are invalid, via Buffer.BlockCopy; O(count)"
        }
      ]
    },
    {
      "for": "IGenericPool",
      "members": [
        {
          "sig": "AcquireAsDisposable<TResource>(this IGenericPool pool, Action<TResource> onDispose):PoolableResourceDisposer<TResource> where TResource : class, new()",
          "desc": "acquires a TResource and wraps it in a disposer that invokes onDispose then returns it to the pool",
          "example": "using (var guard = pools.SmallObjectsPool.AcquireAsDisposable<MyHandler>(h => h.Reset())) { var h = guard.Resource; }"
        }
      ]
    },
    {
      "for": "IMultiRefByteArray",
      "members": [
        {
          "sig": "Span(this IMultiRefByteArray source, int offset, int length):IMultiRefByteArray",
          "desc": "returns a new MultiRefByteArraySpan over source[offset..offset+length) and acquires an extra reference on source; throws IndexOutOfRangeException if the range is invalid",
          "example": "IMultiRefByteArray view = source.Span(2, 5);"
        }
      ]
    },
    {
      "for": "IMultiRefResource",
      "members": [
        {
          "sig": "Acquire<T>(this T element):T where T : class, IMultiRefResource",
          "desc": "calls AddRef() on element and returns the same instance; each call must be balanced with a Release",
          "example": "IMultiRefByteArray another = shared.Acquire();"
        }
      ]
    },
    {
      "for": "IPool<TResource, TParam0>",
      "members": [
        {
          "sig": "AcquireAsDisposable<TResource, TParam0>(this IPool<TResource, TParam0> pool, TParam0 param0, Action<TResource> onDispose):PoolableResourceDisposer<TResource> where TResource : class",
          "desc": "acquires a resource classified by param0 and returns a disposer that calls onDispose then Release"
        }
      ]
    },
    {
      "for": "IPool<TResource>",
      "members": [
        {
          "sig": "AcquireAsDisposable<TResource>(this IPool<TResource> pool, Action<TResource> onDispose):PoolableResourceDisposer<TResource> where TResource : class, new()",
          "desc": "acquires a resource and returns a disposer that calls onDispose then Release"
        }
      ]
    },
    {
      "for": "IReadOnlyByteArray",
      "members": [
        {
          "sig": "CopyTo(this IReadOnlyByteArray src, byte[] dst, int dstOffset, int srcOffset, int count):bool",
          "desc": "copies count bytes from src at srcOffset into dst at dstOffset; returns false if the object is released or any range is invalid; O(count)"
        }
      ]
    },
    {
      "for": "IReadOnlyBytes",
      "members": [
        {
          "sig": "ToArray(this IReadOnlyBytes self, IPool<byte[], int>? pool = null):byte[]?",
          "desc": "copies the valid bytes into a new array, or into one rented from pool; returns null if !IsValid; caller takes ownership of the returned array and should release it back to pool; O(count)",
          "example": "byte[]? copy = bytes.ToArray(pool);"
        }
      ]
    },
    {
      "for": "IReleasableResource",
      "members": [
        {
          "sig": "AsDisposable<TResource>(this TResource resource):ReleasableResourceDisposer<TResource> where TResource : class, IReleasableResource",
          "desc": "wraps resource in a disposer whose Dispose calls Release() on it",
          "example": "using (var guard = resource.AsDisposable()) { var r = guard.Resource; }"
        }
      ]
    }
  ],
  "gotchas": [
    "Acquire(size) on RawArray pools returns an array of length NextPow2(size), which is larger than requested when size is not a power of two.",
    "AddRef on a RefCounter requires the caller to already hold a reference; it is not a way to acquire a released counter.",
    "ByteArrayConcurrentPoolGuard.Release is a no-op: resources acquired from it must be released via IMultiRefByteArray.Release(), not pool.Release().",
    "ByteArrayPool recycling (wrapperCacheCapacity > 0) reuses wrapper object identity, so a stale reference released twice or released after the resource was re-acquired corrupts the current owner's reference count and can return an in-use byte[] to the pool.",
    "ByteArrayPool.Release is a no-op: the wrapped byte[] returns to the raw pool only when the MultiRefByteArrayFromPool reference count reaches zero.",
    "ByteSinkToArray.Put writes at index _position (ignoring the offset), while PutMany writes at _position + _offset; prefer PutMany with a non-zero offset.",
    "ByteSinkToManyArrays.Finish publishes only the current block when it is exactly full; a partially-filled trailing block is silently dropped.",
    "ByteSourceFromStream.TryPop casts Stream.ReadByte to byte, so an early EOF (ReadByte returns -1) yields the value 255 while countToRead is still positive.",
    "CollectablePool supports at most 1000 distinct resource types; the fixed internal constructors array throws IndexOutOfRangeException beyond that.",
    "MultiRefByteArray and StaticByteArray do not validate offset/count in the constructor; out-of-range reads throw IndexOutOfRangeException at access time.",
    "MultiRefByteArrayGuard takes ownership of the single reference of the wrapped resource; releasing the guard releases the inner resource exactly once.",
    "MultiRefByteArraySpan copies source.Array and source.Offset into the base before validating the range, so an invalid range still throws IndexOutOfRangeException after the base fields are already set.",
    "MultiRefResource misuse (double Release, AddRef after Release) does not throw; MultiRefResourceMonitor raises its Error event instead, and it reports nothing until MultiRefResource.ConstructMonitor() is called.",
    "MultiRefResourceVoidMonitor never reports errors; static arrays (StaticReadOnlyByteArray) return it from ConstructMonitor.",
    "NoPool<TResource> and DelegateNoPool<TResource> never reuse objects; Release is a no-op, so pool-based code still runs but allocates every time.",
    "PoolableResourceDisposer.Resource and ReleasableResourceDisposer.Resource throw NullReferenceException after Dispose; capture the resource before disposing.",
    "RefCounter notifications (Updated, Error) are best-effort: handlers run synchronously on the calling thread and are not serialized.",
    "RefCounter.AddRef returns 0 on failure, while RefCounter.Release returns 0 when the last reference is released; the meaning of 0 differs per method.",
    "RefCounter.Release does not clamp at zero, so repeated releases drive the count negative and each reports FailedToDecrement.",
    "StaticByteArray and StaticReadOnlyByteArray implement the IMultiRef* interfaces but Release and AddRef are no-ops and IsAlive is always true.",
    "The ByteArraysPool from MemoryRental.Shared wraps a RawArrayConcurrentPool<byte>, so Acquire returns arrays of size NextPow2(requested).",
    "The MemoryRental.Shared array-pool capacity delegate returns 0 for sizes above 1MiB, so very large arrays fall back to per-Acquire allocation.",
    "Wrappers with a live monitor (MultiRefResource.ConstructMonitor) are never recycled by ByteArrayPool, so a monitored wrapper is always dropped instead of entering the wrapper store."
  ],
  "commonMistakes": [
    "Calling ByteArrayConcurrentPoolGuard.Release expecting the resource to be released; release the IMultiRefByteArray itself instead.",
    "Calling ByteArrayPool.Release and expecting the raw byte[] to be returned; the wrapper reference count must reach zero instead.",
    "Calling GetSpan on a MultiRefByteArray; the sub-range helper is now the extension method Span, called as source.Span(offset, length).",
    "Calling Release twice on a resource acquired from a recycling pool (wrapperCacheCapacity > 0), which corrupts the next owner's reference count and can return an in-use byte[] to the pool; only enable recycling when acquires and releases are balanced.",
    "Forgetting to call Release() on an acquired MultiRefByteArray, leaking the pooled byte[] forever.",
    "Holding a MultiRefByteArray past its Release and reading it, which throws NullReferenceException because the backing array is nulled.",
    "Ignoring the bool returned by Put/PutMany/TryPop/TakeMany; failure means the data was not written or read.",
    "Passing a negative count or out-of-range offset to MultiRefByteArray; validation happens only when the indexer or CopyTo is used.",
    "Trusting the value from ByteSourceFromStream.TryPop at EOF; an early EOF yields 255, not a done signal.",
    "Using ByteSinkToManyArrays.Finish to flush a partially-filled block; only exactly-full blocks are published.",
    "Using a non-concurrent pool (Pool<TResource>, DelegatePool, RawArrayPool) from multiple threads without external locking."
  ],
  "patterns": [
    {
      "goal": "Acquire a byte array, write, and return it via the sink",
      "code": "var pool = MemoryRental.Shared.GetArrayPool<byte>(); var (buf, sink) = pool.AcquireEx(256); buf[0] = 1; sink.Release(buf);"
    },
    {
      "goal": "Acquire a pooled object with automatic disposal",
      "code": "PoolableResourceDisposer<MyHandler> guard = pools.SmallObjectsPool.AcquireAsDisposable<MyHandler>(h => h.Reset()); MyHandler h = guard.Resource; guard.Dispose();"
    },
    {
      "goal": "Acquire a reference-counted byte array from the global pool and release by ref count",
      "code": "var (arr, sink) = MemoryRental.Shared.ByteArraysPool.AcquireEx(512); try { arr[0] = 42; } finally { arr.Release(); }"
    },
    {
      "goal": "Acquire from the shared recycling byte-array pool without per-acquire allocation",
      "code": "var a = MemoryRental.Shared.SharedByteArraysPool.Acquire(512); try { a[0] = 42; } finally { a.Release(); }"
    },
    {
      "goal": "Create a high-performance concurrent object pool with BufferedPool",
      "code": "var pool = new BufferedPool<MyObject>(bucketSize: 100, distributionLevel: 10, () => new MyObject()); MyObject o = pool.Acquire(); pool.Release(o);"
    },
    {
      "goal": "Create a sub-range view of a byte array with the Span extension",
      "code": "var src = new StaticByteArray(new byte[] { 0, 1, 2, 3, 4 }); IMultiRefByteArray view = src.Span(1, 3); byte b = view[0]; view.Release();"
    },
    {
      "goal": "Detect misuse of a byte-array pool in tests with the guard",
      "code": "var guardPool = new ByteArrayConcurrentPoolGuard(realByteArrayPool); IMultiRefByteArray bytes = guardPool.Acquire(16); bytes.Release(); guardPool.AssertNoOutstanding(); bool clean = guardPool.ErrorCount == 0;"
    },
    {
      "goal": "Drain a byte source gracefully until TryPop returns false",
      "code": "var src = new ByteSourceFromArray(new StaticByteArray(new byte[] { 1, 2, 3 })); byte acc = 0; while (src.TryPop(out byte b)) { acc += b; }"
    },
    {
      "goal": "Enable wrapper recycling on a custom concurrent byte-array pool",
      "code": "var pool = new ByteArrayConcurrentPool(MemoryRental.Shared.GetArrayPool<byte>(), wrapperCacheCapacity: 256); var a = pool.Acquire(128); a[0] = 1; a.Release();"
    },
    {
      "goal": "Pool arrays by size with a custom capacity delegate that disables pooling for large sizes",
      "code": "var pool = new RawArrayConcurrentPool<byte>(size => size <= 1024 ? 100 : 0); byte[] buf = pool.Acquire(300); pool.Release(buf);"
    },
    {
      "goal": "Share a MultiRefResource between two owners with Acquire, balancing each Release",
      "code": "var arr = new MultiRefByteArray(new byte[] { 1, 2, 3 }); IMultiRefByteArray owner2 = arr.Acquire(); arr.Release(); owner2.Release();"
    },
    {
      "goal": "Use CollectablePool with a custom collectable resource",
      "code": "class MyResource : MultiRefCollectableResource<MyResource> { protected override void OnCollected() { } protected override void OnRestored() { } } MyResource r = collectablePool.Acquire<MyResource>(); r.Release();"
    },
    {
      "goal": "Write through a byte sink and handle a full buffer gracefully",
      "code": "var sink = new ByteSinkToArray(new byte[2]); if (!sink.Put(1)) Console.WriteLine(\"full\"); bool ok = sink.PutMany(new StaticByteArray(new byte[] { 2, 3 })); if (!ok) Console.WriteLine(\"full\");"
    }
  ],
  "relationships": [
    {
      "from": "BufferedPool<TObject>",
      "to": "Bucket<TObject>",
      "rel": "manages per-thread buckets"
    },
    {
      "from": "BufferedPool<TObject>",
      "to": "BucketSource<TObject>",
      "rel": "uses as the bucket supplier"
    },
    {
      "from": "ByteArrayConcurrentPool",
      "to": "ByteArrayPool",
      "rel": "extends"
    },
    {
      "from": "ByteArrayConcurrentPool",
      "to": "IConcurrentPool<IMultiRefByteArray, int>",
      "rel": "implements"
    },
    {
      "from": "ByteArrayConcurrentPoolGuard",
      "to": "IConcurrentPool<IMultiRefByteArray, int>",
      "rel": "implements"
    },
    {
      "from": "ByteArrayConcurrentPoolGuard",
      "to": "MultiRefByteArrayGuard",
      "rel": "creates in Acquire/AcquireEx"
    },
    {
      "from": "ByteArrayConcurrentPoolGuard",
      "to": "MultiRefResourceMonitor",
      "rel": "creates per guard via ConstructMonitor and aggregates its Error"
    },
    {
      "from": "ByteArrayPool",
      "to": "IPool<IMultiRefByteArray, int>",
      "rel": "implements"
    },
    {
      "from": "ByteArrayPool",
      "to": "MultiRefByteArrayFromPool",
      "rel": "creates in Acquire/AcquireEx wrapping a pooled raw byte[]"
    },
    {
      "from": "ByteArrayPool",
      "to": "RecyclableMultiRefByteArrayFromPool",
      "rel": "creates in Acquire/AcquireEx when wrapperCacheCapacity is greater than zero"
    },
    {
      "from": "ByteSinkToManyArrays<TArray>",
      "to": "IByteArray",
      "rel": "generic constraint"
    },
    {
      "from": "CollectablePool",
      "to": "ICollectableResource",
      "rel": "calls Collected on return and Restored on acquire"
    },
    {
      "from": "ConcurrentDelegatePool<TResource>",
      "to": "ConcurrentPool<TResource>",
      "rel": "extends"
    },
    {
      "from": "ConcurrentPool<TResource>",
      "to": "Pool<TResource>",
      "rel": "extends"
    },
    {
      "from": "DefaultConcurrentPool<TResource>",
      "to": "ConcurrentDelegatePool<TResource>",
      "rel": "extends"
    },
    {
      "from": "DefaultPool<TResource>",
      "to": "DelegatePool<TResource>",
      "rel": "extends"
    },
    {
      "from": "DelegateNoPool<TResource>",
      "to": "IConcurrentPool<TResource>",
      "rel": "implements"
    },
    {
      "from": "DelegatePool<TResource>",
      "to": "Pool<TResource>",
      "rel": "extends"
    },
    {
      "from": "EmptyByteArray",
      "to": "StaticByteArray",
      "rel": "extends"
    },
    {
      "from": "FixedLengthRawArrayConcurrentPool<T>",
      "to": "ConcurrentPool<T[]>",
      "rel": "extends"
    },
    {
      "from": "FixedLengthRawArrayPool<T>",
      "to": "Pool<T[]>",
      "rel": "extends"
    },
    {
      "from": "GenericConcurrentPool",
      "to": "BufferedPool<TResource>",
      "rel": "creates in GetPool"
    },
    {
      "from": "GenericConcurrentPool",
      "to": "TypeToIntStaticMap",
      "rel": "uses GetTypeId for per-type ids"
    },
    {
      "from": "LargeObjectBufferedPool<TObject>",
      "to": "BufferedPool<TObject>",
      "rel": "extends with bucketSize 20 and distributionLevel 3"
    },
    {
      "from": "MemoryRental",
      "to": "ByteArrayConcurrentPool",
      "rel": "creates in ctor as ByteArraysPool and SharedByteArraysPool"
    },
    {
      "from": "MemoryRental",
      "to": "CollectablePool",
      "rel": "creates in ctor"
    },
    {
      "from": "MemoryRental",
      "to": "GenericConcurrentPool",
      "rel": "creates as SmallObjectsPool and BigObjectsPool"
    },
    {
      "from": "MemoryRental",
      "to": "RawArrayConcurrentPool<T>",
      "rel": "creates per type in GetArrayPool"
    },
    {
      "from": "MultiRefByteArray",
      "to": "MultiRefResource",
      "rel": "extends"
    },
    {
      "from": "MultiRefByteArrayFromPool",
      "to": "IPoolSink<byte[]>",
      "rel": "returns the raw byte[] to it in OnReleased"
    },
    {
      "from": "MultiRefByteArrayFromPool",
      "to": "MultiRefByteArray",
      "rel": "extends"
    },
    {
      "from": "MultiRefByteArrayGuard",
      "to": "IMultiRefByteArray",
      "rel": "implements"
    },
    {
      "from": "MultiRefByteArrayGuard",
      "to": "MultiRefResource",
      "rel": "extends"
    },
    {
      "from": "MultiRefByteArraySpan",
      "to": "IMultiRefByteArray",
      "rel": "acquires a reference to source in the ctor"
    },
    {
      "from": "MultiRefByteArraySpan",
      "to": "MultiRefByteArray",
      "rel": "extends"
    },
    {
      "from": "MultiRefByteArraySpan_Ext",
      "to": "IMultiRefByteArray",
      "rel": "Span extension returns a MultiRefByteArraySpan over it"
    },
    {
      "from": "MultiRefCollectableResource<TSelf>",
      "to": "MultiRefResource",
      "rel": "extends"
    },
    {
      "from": "MultiRefResource",
      "to": "IMultiRefResource",
      "rel": "implements"
    },
    {
      "from": "MultiRefResourceMonitor",
      "to": "IMultiRefResourceMonitor",
      "rel": "implements"
    },
    {
      "from": "MultiRefResourceVoidMonitor",
      "to": "IMultiRefResourceMonitor",
      "rel": "implements"
    },
    {
      "from": "NoPool<TResource>",
      "to": "IConcurrentPool<TResource>",
      "rel": "implements"
    },
    {
      "from": "Pool<TResource, TParam0>",
      "to": "IPool<TResource, TParam0>",
      "rel": "implements"
    },
    {
      "from": "Pool<TResource>",
      "to": "IPool<TResource>",
      "rel": "implements"
    },
    {
      "from": "PoolableResourceDisposer<TResource>",
      "to": "IPoolSink<TResource>",
      "rel": "calls Release in Dispose"
    },
    {
      "from": "RawArrayConcurrentPool<T>",
      "to": "ConcurrentPool<T[], int>",
      "rel": "extends"
    },
    {
      "from": "RawArrayConcurrentPool<T>",
      "to": "FixedLengthRawArrayConcurrentPool<T>",
      "rel": "creates per class id when capacity > 0"
    },
    {
      "from": "RawArrayPool<T>",
      "to": "FixedLengthRawArrayPool<T>",
      "rel": "creates per class id"
    },
    {
      "from": "RawArrayPool<T>",
      "to": "Pool<T[], int>",
      "rel": "extends"
    },
    {
      "from": "RecyclableMultiRefByteArrayFromPool",
      "to": "ByteArrayPool",
      "rel": "returns itself via the wrapper sink in OnReleased"
    },
    {
      "from": "RecyclableMultiRefByteArrayFromPool",
      "to": "MultiRefByteArrayFromPool",
      "rel": "extends"
    },
    {
      "from": "RefCounter",
      "to": "IRefCounterView",
      "rel": "implements"
    },
    {
      "from": "RefCounter",
      "to": "MultiRefResource",
      "rel": "backs the reference count"
    },
    {
      "from": "ReleasableResourceDisposer<TResource>",
      "to": "IReleasableResource",
      "rel": "calls Release in Dispose"
    },
    {
      "from": "SmallObjectBufferedPool<TObject>",
      "to": "BufferedPool<TObject>",
      "rel": "extends with bucketSize 100 and distributionLevel 10"
    },
    {
      "from": "StaticByteArray",
      "to": "StaticReadOnlyByteArray",
      "rel": "extends"
    },
    {
      "from": "StaticReadOnlyByteArray",
      "to": "IMultiRefReadOnlyByteArray",
      "rel": "implements with no-op reference counting"
    },
    {
      "from": "StaticReadOnlyByteArray",
      "to": "MultiRefResourceVoidMonitor",
      "rel": "returns from ConstructMonitor"
    },
    {
      "from": "VoidByteArray",
      "to": "StaticByteArray",
      "rel": "extends"
    }
  ]
}
