# Actuarius.Memory

Target: `netstandard2.1`, LangVersion 9, Nullable enabled.  
NuGet: `Actuarius.Memory` v0.1.5-dev.0  
Depends on: `Actuarius.Collections`

All types in namespace `Actuarius.Memory` unless noted.

---

## Overview

Object pooling, byte array management (reference-counted, pooled, and static), byte sinks/sources, and collectable resources. Central entry point is `MemoryRental.Shared`.

---

## Entry Point

### `interface IMemoryRental`
- `IConcurrentPool<IMultiRefByteArray, int> ByteArraysPool { get; }`
- `ICollectablePool CollectablePool { get; }`
- `IGenericConcurrentPool SmallObjectsPool { get; }`
- `IGenericConcurrentPool BigObjectsPool { get; }`
- `IConcurrentPool<T[], int> GetArrayPool<T>()`

### `class MemoryRental : IMemoryRental`
- `static readonly IMemoryRental Shared` — global singleton
- `MemoryRental()` — constructor wires all pools:
  - `ByteArraysPool = new ByteArrayConcurrentPool(GetArrayPool<byte>())`
  - `CollectablePool = new CollectablePool(() => new LimitedConcurrentQueue<ICollectableResource>(100))`
  - `SmallObjectsPool = new GenericConcurrentPool(…, 100, 10)`
  - `BigObjectsPool = new GenericConcurrentPool(…, 10, 2)`

---

## Pool Abstractions

### `interface IPoolRef` — marker base

### `interface IPoolSink<in TResource> : IPoolRef`
- `void Release(TResource? resource)` — return object to pool

### `interface IConcurrentPoolSink<in TResource> : IPoolSink<TResource>` — marker

### `interface IPool<TResource> : IPoolSink<TResource>` — acquire without params
- `TResource Acquire()`

### `interface IConcurrentPool<TResource> : IPool<TResource>, IConcurrentPoolSink<TResource>`

### `interface IGenericPool`
- `IPool<TResource> GetPool<TResource>() where TResource : class, new()`
- `(TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>() where TResource : class, new()`

### `interface IGenericConcurrentPool : IGenericPool`
- `new IConcurrentPool<TResource> GetPool<TResource>() where TResource : class, new()`

### `interface IPool<TResource, in TParam0> : IPoolSink<TResource>` — acquire with 1 param
- `TResource Acquire(TParam0 param0)`
- `(TResource resource, IPoolSink<TResource> poolSink) AcquireEx(TParam0 param0)`

### `interface IConcurrentPool<TResource, in TParam0> : IPool<TResource, TParam0>, IConcurrentPoolSink<TResource>`

### `interface IGenericPool<in TParam>`
- `IPool<TResource, TParam> ShowTypedPool<TResource>()`
- `(TResource resource, IPoolSink<TResource> poolSink) Acquire<TResource>(TParam param)`

### `interface IGenericConcurrentPool<in TParam> : IGenericPool<TParam>`
- `new IConcurrentPool<TResource, TParam> ShowTypedPool<TResource>()`

### `struct PoolableResourceDisposer<TResource> : IDisposable` where `TResource : class`
- `TResource Resource { get; }` — throws NRE if already disposed
- `PoolableResourceDisposer(TResource resource, IPoolSink<TResource> sink, Action<TResource> onDispose)`
- `void Dispose()` — calls `onDispose` then `sink.Release`

---

## Pool Implementations (0-param)

| Class | Base | Notes |
|---|---|---|
| `Pool<TResource> : IPool<TResource>` where `TResource : class` | abstract | Uses internal `IUnorderedCollection<TResource>` + optional deinitializer `Func<TResource,bool>?`. Ctors: `(Func<TResource,bool>?)`, `(IUnorderedCollection<TResource>, Func<TResource,bool>?)`. Override: `abstract Constructor()` |
| `ConcurrentPool<TResource> : Pool<TResource>, IConcurrentPool<TResource>` where `TResource : class` | abstract | Uses `IConcurrentUnorderedCollection<TResource>`. Ctor: `(IConcurrentUnorderedCollection<TResource>, Func<TResource,bool>?)` |
| `DelegatePool<TResource> : Pool<TResource>` where `TResource : class` | concrete | Ctor: `(Func<TResource> resourceCtor, Func<TResource,bool>? deInitializer)` or with custom collection |
| `DefaultPool<TResource> : DelegatePool<TResource>` where `TResource : class, new()` | concrete | Ctor: `()` — uses `new TResource()` |
| `ConcurrentDelegatePool<TResource> : ConcurrentPool<TResource>` where `TResource : class` | concrete | Ctor: `(Func<TResource>, IConcurrentUnorderedCollection<TResource>, Func<TResource,bool>?)` |
| `DefaultConcurrentPool<TResource> : ConcurrentDelegatePool<TResource>` where `TResource : class, new()` | concrete | Ctor: `(IConcurrentUnorderedCollection<TResource>)` |
| `NoPool<TResource> : IConcurrentPool<TResource>` where `TResource : class, new()` | concrete | Never pools — always new, release does nothing. `static readonly NoPool<TResource> Instance` |
| `DelegateNoPool<TResource> : IConcurrentPool<TResource>` | concrete | Ctor: `(Func<TResource> resourceFactory)` — never pools |
| `BufferedPool<TObject> : IConcurrentPool<TObject>` where `TObject : class` | concrete | High-perf per-thread bucket pool. Ctors: `(int bucketSize, int distributionLevel, Func<TObject> acquire)`, or with custom `Func<IConcurrentUnorderedCollection<Bucket<TObject>>>` |
| `SmallObjectBufferedPool<TObject> : BufferedPool<TObject>` where `TObject : class` | concrete | Ctor: `(Func<TObject>)` — bucketSize=100, distLevel=10 |
| `LargeObjectBufferedPool<TObject> : BufferedPool<TObject>` where `TObject : class` | concrete | Ctor: `(Func<TObject>)` — bucketSize=20, distLevel=3 |

### `class GenericConcurrentPool : IGenericConcurrentPool`
- Ctor: `(IConcurrentMap<int, object> typeIdMap, int bucketSize, int distributionLevel)`

---

## Pool Implementations (1-param)

| Class | Base | Notes |
|---|---|---|
| `Pool<TResource, TParam0> : IPool<TResource, TParam0>` where `TResource : class` | abstract | Parameterized by classification. Override: `CreatePool(int)`, `Classify(TResource)`, `Classify(TParam0)`. Ctor: `(IMap<int, IPool<TResource>>)` |
| `DelegatePool<TResource, TParam1> : Pool<TResource, TParam1>` where `TResource : class` | concrete | Ctor: `(IMap<int, IPool<TResource>>, Func<int, IPool<TResource>>, Func<TResource, int>, Func<TParam1, int>)` |
| `ConcurrentPool<TResource, TParam> : IConcurrentPool<TResource, TParam>` where `TResource : class` | abstract | Concurrent version. Override: `CreatePool(int)`, `Classify(TResource)`, `Classify(TParam)`. Ctor: `(IConcurrentMap<int, IConcurrentPool<TResource>>)` |
| `ConcurrentDelegatePool<TResource, TParam> : ConcurrentPool<TResource, TParam>` | concrete | Ctor: `(IConcurrentMap<int, IConcurrentPool<TResource>>, Func<int, IConcurrentPool<TResource>>, Func<TResource, int>, Func<TParam, int>)` |

### Array Pools

| Class | Base | Notes |
|---|---|---|
| `RawArrayPool<T> : Pool<T[], int>` | concrete | Classifies by `NextPow2(size)`. Ctor: `(Func<T[],bool>? deInitializer)` |
| `RawArrayConcurrentPool<T> : ConcurrentPool<T[], int>` | concrete | Concurrent. Ctors: `(int bucketCapacity, Func<T[],bool>?)`, `(ArrayPoolCapacityDelegate)` |
| `FixedLengthRawArrayPool<T> : Pool<T[]>` | concrete | Fixed-length arrays. Ctor: `(int length, Func<T[],bool>?)` |
| `FixedLengthRawArrayConcurrentPool<T> : ConcurrentPool<T[]>` | concrete | Ctor: `(int length, int capacity, Func<T[],bool>?)` |

### `delegate int ArrayPoolCapacityDelegate(int arraySize)` — for `RawArrayConcurrentPool`

---

## Byte Array Types

### Reference-Counted (MultiRefResource)

```
IReleasableResource
  └── IMultiRefResource : IReleasableResource
        ├── bool IsAlive { get; }
        └── void AddRef()
             └── IMultiRefReadOnlyBytes : IMultiRefResource, IReadOnlyBytes
                   └── IMultiRefReadOnlyByteArray : IMultiRefReadOnlyBytes, IReadOnlyByteArray
                         └── IMultiRefByteArray : IMultiRefReadOnlyByteArray, IByteArray
```

### `class MultiRefByteArray : MultiRefResource, IMultiRefByteArray`
- Ctor: `(byte[] array, int offset = 0, int count = -1)`
- `int Count { get; }`, `int Offset { get; }`, `bool IsValid { get; }`
- `byte this[int id] { get; set; }`
- `bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)`
- `byte[] ReadOnlyArray { get; }`, `byte[] Array { get; }`
- `override OnReleased()` — custom release logic

### `class MultiRefByteArraySpan : MultiRefByteArray`
- Ctor: `(IMultiRefByteArray source, int offset, int count)` — acquires source; releases on own release
- Throws `IndexOutOfRangeException` if range invalid

### `class MultiRefByteArrayFromPool : MultiRefByteArray`
- Ctor: `(IPoolSink<byte[]> pool, byte[] array, int offset, int count)`
- `OnReleased()` returns underlying `byte[]` to pool

### `class VoidByteArray : MultiRefByteArray` — singleton `Instance`; `CopyTo` always returns false
### `class EmptyByteArray : MultiRefByteArray` — singleton `Instance`; zero-length

### `class StaticReadOnlyByteArray : IMultiRefReadOnlyByteArray` — wraps fixed `byte[]`, no ref-counting
- Ctors: `(byte[])`, `(byte[], int offset, int length)`
- `Release()` no-op, `IsAlive => true`, `AddRef()` no-op
- `this[int] { get; }`, `Count`, `Offset`, `ReadOnlyArray`, `CopyTo`

### `class StaticByteArray : StaticReadOnlyByteArray, IMultiRefByteArray`
- Ctor: `(byte[] array, int offset = 0, int length = -1)`
- `new byte this[int id] { get; set; }`, `byte[] Array { get; }`

### Extension: `static IMultiRefByteArray GetSpan(this IMultiRefByteArray source, int offset, int length)` — creates `MultiRefByteArraySpan`

---

## Collectable Resources

### `interface ICollectableResource`
- `bool Collected()` — called when object is returned to pool; return true to allow reuse

### `interface ICollectableResource<out TSelf> : ICollectableResource`
- `void Restored(IPoolSink<TSelf> pool)` — called when object is acquired from pool

### `interface ICollectablePool`
- `TResource Acquire<TResource>() where TResource : class, ICollectableResource<TResource>, new()`

### `class CollectablePool : ICollectablePool`
- Ctor: `(Func<IConcurrentUnorderedCollection<ICollectableResource>> singleTypePool)`

### `abstract class MultiRefCollectableResource<TSelf> : MultiRefResource, ICollectableResource<TSelf>` where `TSelf : MultiRefCollectableResource<TSelf>`
- Protected ctor: `MultiRefCollectableResource()` — passes `true` (noInit)
- `abstract OnCollected()` — called before return to pool
- `abstract OnRestored()` — called after acquire from pool
- Inherits `AddRef()`, `Release()`, `IsAlive`, `Revive()` from `MultiRefResource`

---

## Releasable Resources

### `interface IReleasableResource`
- `void Release()` — inform object ownership ended

### `static class IReleasableResource_Ext`
- `ReleasableResourceDisposer<TResource> AsDisposable<TResource>(this TResource resource)` where `TResource : class, IReleasableResource` — wraps as `IDisposable`

### `struct ReleasableResourceDisposer<TResource> : IDisposable` where `TResource : class, IReleasableResource`
- `TResource Resource { get; }` — throws NRE if already disposed
- `void Dispose()` — calls `Resource.Release()`

### `interface IMultiRefResource : IReleasableResource`
- `bool IsAlive { get; }`
- `void AddRef()` — increment owner count

### `abstract class MultiRefResource : IMultiRefResource`
- `protected MultiRefResource(bool noInit)`
- `protected abstract void OnReleased()` — called when ref count reaches 0
- `protected virtual bool TraceEnabled => true`
- `public bool IsAlive { get; }`
- `public void AddRef()`
- `public void Release()` — decrements; calls `OnReleased()` at 0; may throw on underflow
- `protected bool Revive()` — transition from released back to alive (for pooling)
- `protected void AssertNoUsage()` — asserts no outstanding references
- `protected enum ResourceUsageErrorType { AddRefOfReleasedObject, ReleaseOfReleasedObject, WrongReviveUsage, Leak, NoUsageAssertionFail }`
- `protected virtual void OnRefCountError(ResourceUsageErrorType, ActionHistoryTracer?)`

### `static class IMultiRefResource_Ext`
- `T Acquire<T>(this T element) where T : class, IMultiRefResource` — calls `AddRef()`, returns same reference

---

## Byte Sinks & Sources

### `interface IByteSink : IConsumer<byte>`
- `bool PutMany<TBytes>(TBytes bytes) where TBytes : IReadOnlyBytes` — bulk insert

### `struct ByteSink : IByteSink`
- Ctor: `(IByteArray array)`
- `int Position { get; set; }`
- `bool Put(byte value)`
- `bool PutMany<TBytes>(TBytes bytes)`

### `struct ByteSinkToArray : IByteSink`
- Ctor: `(byte[] array, int offset = 0, int count = -1)` — writes into raw `byte[]`
- `bool Put(byte value)`
- `bool PutMany<TBytes>(TBytes bytes)`

### `struct ByteSinkToManyArrays<TArray> : IByteSink` where `TArray : IByteArray`
- Ctor: `(IProducer<TArray> emptyArraysProducer, IConsumer<TArray> filledArraysConsumer)` — accumulates across multiple `IByteArray` blocks
- `void Finish()` — publish remaining partial block

### `interface IByteSource : IProducer<byte>`
- `bool TakeMany(IMultiRefByteArray dst)` — bulk read

### `struct ByteSourceFromArray : IByteSource`
- Ctor: `(IReadOnlyByteArray array)`
- `int Position { get; set; }`
- `bool TryPop(out byte value)`
- `bool TakeMany(IMultiRefByteArray dst)`

### `struct ByteSourceFromStream : IByteSource`
- Ctor: `(Stream stream, int countToRead = -1)` — reads from `System.IO.Stream`
- `bool TryPop(out byte value)`
- `bool TakeMany(IMultiRefByteArray dst)`

---

## Byte Array Abstractions

### `interface IReadOnlyBytes : ICountable`
- `bool IsValid { get; }`
- `bool CopyTo(byte[] dst, int dstOffset, int srcOffset, int count)` — false on failure

### `static class IReadOnlyBytes_Ext`
- `byte[]? ToArray(this IReadOnlyBytes self, IPool<byte[], int>? pool = null)` — copy to new/existing array

### `interface IReadOnlyByteArray : IReadOnlyBytes, IReadOnlyArray<byte>`
- `byte[] ReadOnlyArray { get; }`
- `int Offset { get; }`

### `interface IByteArray : IReadOnlyByteArray, IArray<byte>`
- `byte[] Array { get; }`

### `static class IByteArray_Ext`
- `bool CopyFrom(this IByteArray dst, byte[] src, int srcOffset, int dstOffset, int count)`

---

## Utility Types (Namespace: `Actuarius.Memory.Internal`)

### `class RefCounter` — thread-safe ref counter (used by `MultiRefResource`)
- `void Init()`, `bool IsValid`, `int AddRef()`, `int Release()`, `bool Revive()`

### `class ActionHistoryTracer` — debug tracing for ref-count issues
- `void RecordEvent(string name, int skipFrames = 1)`, `void Clear()`

## Utility: `static class TypeToIntStaticMap`
- `int GetTypeId<T>()` — unique int per type T at runtime

---

## Usage Patterns & Code Snippets

### MemoryRental global entry
```csharp
// Global singleton
var pools = MemoryRental.Shared;

// Typed array pool (byte[])
var bytePool = pools.GetArrayPool<byte>();
byte[] buf = bytePool.Acquire(256);
bytePool.Release(buf);

// Byte array pool (IMultiRefByteArray by size)
IMultiRefByteArray arr = pools.ByteArraysPool.Acquire(size: 1024);
// ... use arr ...
// Release via AcquireEx:
var (arr2, sink) = pools.ByteArraysPool.AcquireEx(512);
sink.Release(arr2);   // returns raw byte[] to pool

// Generic small/big object pools
IConcurrentPool<MyHandler> hPool = pools.SmallObjectsPool.GetPool<MyHandler>();
MyHandler h = hPool.Acquire();
hPool.Release(h);

// Or with auto-dispose
using var disposable = pools.SmallObjectsPool.AcquireAsDisposable<MyHandler>(onDispose: h => h.Reset());
var handler = disposable.Resource;

// Collectable pool
var resource = pools.CollectablePool.Acquire<MyCollectable>();
```

### GenericConcurrentPool (standalone)
```csharp
var typeIdMap = new SynchronizedConcurrentDictionary<int, object>();
var genericPool = new GenericConcurrentPool(typeIdMap, bucketSize: 100, distributionLevel: 10);

IConcurrentPool<MyObj> pool = genericPool.GetPool<MyObj>();
MyObj obj = pool.Acquire();
pool.Release(obj);

// Acquire with automatic return:
var (r, sink) = genericPool.Acquire<MyObj>();
sink.Release(r);
```

### BufferedPool (standalone)
```csharp
var pool = new BufferedPool<MyObj>(
    bucketSize: 100,
    distributionLevel: 10,
    () => new MyObj()
);
MyObj obj = pool.Acquire();
pool.Release(obj);

// Pre-configured variants:
var smallPool = new SmallObjectBufferedPool<MyObj>(() => new MyObj());
var largePool = new LargeObjectBufferedPool<MyObj>(() => new MyObj());
```

### RawArrayConcurrentPool (pool arrays by size)
```csharp
// Fixed capacity for all sizes
var pool = new RawArrayConcurrentPool<byte>(bucketCapacity: 100, deInitializer: null);

// Dynamic capacity based on size
var dynPool = new RawArrayConcurrentPool<byte>(size => {
    if (size <= 1024) return 1000;
    if (size <= 10240) return 100;
    return 0;  // fallback to NoPool (no pooling for large arrays)
});

byte[] buffer = pool.Acquire(300);  // returns byte[512] (next power of 2)
pool.Release(buffer);
```

### ByteArrayPool (wrap raw byte[] pool with IMultiRefByteArray)
```csharp
var rawPool = new RawArrayConcurrentPool<byte>(capacity: 100);
var byteArrPool = new ByteArrayPool(rawPool);
var concurrentByteArrPool = new ByteArrayConcurrentPool(rawPool);

IMultiRefByteArray arr = byteArrPool.Acquire(256);
var (arr2, sink) = byteArrPool.AcquireEx(256);
```

### MultiRefByteArray (reference-counted byte array)
```csharp
byte[] raw = new byte[] { 1, 2, 3, 4, 5 };
var arr = new MultiRefByteArray(raw, offset: 1, count: 3);  // views [2,3,4]
arr.AddRef();               // +1 reference
byte b = arr[0];            // → 2
arr[0] = 99;
arr.Release();              // -1 ref
arr.Release();              // calls OnReleased() — ref count is 0
```

### MultiRefByteArraySpan (sub-range view)
```csharp
var source = new StaticByteArray(new byte[] { 0,1,2,3,4,5,6,7,8 });
var span = new MultiRefByteArraySpan(source, offset: 2, count: 5);
// span now views [2,3,4,5,6]
span.Release();  // releases reference on source

// Extension method equivalent:
var span2 = source.GetSpan(2, 5);
span2.Release();
```

### StaticByteArray (no reference counting)
```csharp
var arr = new StaticByteArray(new byte[] { 10, 20, 30 });
byte b = arr[0];           // 10
arr[0] = 99;               // write (implements IMultiRefByteArray)
// No need to Release() — no-op
```

### VoidByteArray / EmptyByteArray (singletons)
```csharp
MultiRefByteArray invalid = VoidByteArray.Instance;    // CopyTo always false
MultiRefByteArray empty = EmptyByteArray.Instance;     // Count = 0
```

### Byte Sink (write bytes)
```csharp
// Sink to raw byte[]
byte[] buffer = new byte[10];
var sink = new ByteSinkToArray(buffer, offset: 0);
sink.Put(1);
sink.PutMany(new StaticByteArray(new byte[] { 2, 3, 4 }));
// buffer = [1,2,3,4,0,0,0,0,0,0]

// Sink to IByteArray
var byteArr = new StaticByteArray(new byte[10]);
var sink2 = new ByteSink(byteArr);
sink2.Put(5);
sink2.PutMany(new StaticByteArray(new byte[] { 6, 7 }));

// Sink across multiple arrays
var emptyProvider = new SystemQueue<StaticByteArray>();
var filledConsumer = new SystemQueue<StaticByteArray>();
var multiSink = new ByteSinkToManyArrays<StaticByteArray>(emptyProvider, filledConsumer);
multiSink.Put(1); multiSink.Put(2);
multiSink.Finish();  // publish remaining partial block
```

### Byte Source (read bytes)
```csharp
var arr = new StaticByteArray(new byte[] { 10, 20, 30, 40 });
var source = new ByteSourceFromArray(arr);
source.TryPop(out byte b1);      // 10
source.TryPop(out byte b2);      // 20

// Bulk read
var dst = new StaticByteArray(new byte[10]);
source.TakeMany(dst);            // reads remaining [30,40] into dst

// From Stream
using var stream = new MemoryStream(new byte[] { 1,2,3,4,5 });
var streamSource = new ByteSourceFromStream(stream, countToRead: 3);
streamSource.TryPop(out byte sb); // 1
```

### Collectable Pool
```csharp
class MyResource : MultiRefCollectableResource<MyResource> {
    protected override void OnCollected() { /* unload heavy data */ }
    protected override void OnRestored() { /* rehydrate */ }
}

// Acquire — returned from pool if available, or created new
var resource = collectablePool.Acquire<MyResource>();
```

### ReleasableResource / IMultiRefResource patterns
```csharp
// Direct release
resource.Release();

// As IDisposable (using block)
using (var guard = resource.AsDisposable()) {
    var r = guard.Resource;  // the resource
    // ... use r ...
}  // calls r.Release() automatically

// IMultiRefResource: acquire (add ref) when passing a reference
var shared = new MultiRefByteArray(new byte[] { 1, 2, 3 });
var anotherRef = shared.Acquire();   // calls AddRef(), returns same instance
anotherRef.Release();  // must balance each Acquire
shared.Release();
```
