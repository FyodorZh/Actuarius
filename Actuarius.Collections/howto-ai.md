# Actuarius.Collections

Target: `netstandard2.1`, LangVersion 9, Nullable enabled.  
NuGet: `Actuarius.Collections` v0.1.5-dev.0

All types in `Actuarius.Collections` unless noted.

---

## Namespace: `Actuarius`

### `static class BitMath`
- `HiBit(uint n)` → `int` — position of highest set bit
- `NextPow2(uint n)` → `int` — smallest power of 2 ≥ n

---

## Core Abstractions (Interfaces)

### `ICountable`
- `int Count { get; }`

### `IConsumer<in TData>` — write side
- `bool Put(TData value)` — place element; returns `false` if full

### `IConcurrentConsumer<in TData> : IConsumer<TData>` — marker for thread-safe Put

### `IProducer<TData>` — read side
- `bool TryPop([MaybeNullWhen(false)] out TData value)` — retrieve next element; returns `false` if empty

### `IConcurrentProducer<TData> : IProducer<TData>` — marker for thread-safe TryPop

### `IUnorderedCollection<TData> : IConsumer<TData>, IProducer<TData>` — bag with undefined removal order
### `ISingleReaderWriterConcurrentUnorderedCollection<TData> : IUnorderedCollection<TData>` — marker for 1P/1C
### `IConcurrentUnorderedCollection<TData> : ISingleReaderWriterConcurrentUnorderedCollection<TData>, IConcurrentConsumer<TData>, IConcurrentProducer<TData>` — marker for MPMC

### `IQueue<TData> : IUnorderedCollection<TData>, ICountable` — FIFO, not thread-safe
### `ISingleReaderWriterConcurrentQueue<TData> : IQueue<TData>, ISingleReaderWriterConcurrentUnorderedCollection<TData>`
### `IConcurrentQueue<TData> : ISingleReaderWriterConcurrentQueue<TData>, IConcurrentUnorderedCollection<TData>`

### `IStack<TData> : IUnorderedCollection<TData>, ICountable` — LIFO, not thread-safe
### `ISingleReaderWriterConcurrentStack<TData> : IStack<TData>, ISingleReaderWriterConcurrentUnorderedCollection<TData>`
### `IConcurrentStack<TData> : ISingleReaderWriterConcurrentStack<TData>, IConcurrentUnorderedCollection<TData>`

### `IReadOnlyArray<out TData> : ICountable`
- `TData this[int id] { get; }`

### `IArray<TData> : IReadOnlyArray<TData>`
- `new TData this[int id] { get; set; }`

### `ISet<in TData> : IConsumer<TData>`
- `bool Remove(TData element)`
- `bool Contains(TData element)`

### `IConcurrentSet<in TData> : ISet<TData>, IConcurrentConsumer<TData>`

### `IMap<in TKey, TData>`
- `bool Add(TKey key, TData element)` — returns `false` if key exists or full
- `bool Remove(TKey key)`
- `bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element)`

### `IConcurrentMap<in TKey, TData> : IMap<TKey, TData>`
- `bool AddOrReplace(TKey key, TData newElement, [MaybeNullWhen(false)] out TData oldElement)`
- `bool AddOrGet(TKey key, TData newElement, [MaybeNullWhen(false)] out TData resultElement)`
- `bool GetAndRemove(TKey key, [MaybeNullWhen(false)] out TData resultElement)`

---

## Concrete Implementations

### Queue / Stack (non-concurrent)

| Class | Implements | Notes |
|---|---|---|
| `SystemQueue<TData>` | `IQueue<TData>` | Backed by `System.Collections.Generic.Queue<T>`. Also: `Clear()` |
| `SystemStack<TData>` | `IStack<TData>` | Backed by `System.Collections.Generic.Stack<T>` |
| `CycleQueue<T>` | `IQueue<T>`, `IArray<T>` | Fixed-capacity circular buffer. Capacity is always power of 2. |

**CycleQueue specifics:**
- `CycleQueue()` — capacity 16, canGrow=true
- `CycleQueue(int capacity, bool canGrow = true)` — capacity rounded up to next power of 2
- `int Capacity { get; }` — current capacity
- `T Head { get; }` — peek head (throws if empty)
- `T Tail { get; }` — peek tail (throws if empty)
- `T this[int id] { get; set; }` — 0 = head, Count-1 = tail (throws on out-of-range)
- `void Clear()`
- `bool Put(T value)` — enqueue; returns false if at capacity and can't grow
- `bool PutToHead(T value)` — prepend; returns false if at capacity
- `bool TryPop(out T value)` — dequeue; returns false if empty
- `EnumerableWithOrder Enumerate(QueueEnumerationOrder order)` — iterate HeadToTail or TailToHead

**CycleQueue nested types:**
```csharp
public readonly struct EnumerableWithOrder : IEnumerable<T> {
    public Enumerator GetEnumerator();
}
public struct Enumerator : IEnumerator<T> {
    public T Current { get; }
    public bool MoveNext();
    public void Reset();
    public void Dispose();
}
```

### `enum QueueEnumerationOrder` — `HeadToTail` = 0, `TailToHead` = 1

---

### `PriorityQueue<Key, Data> : IQueue<KeyValuePair<Key, Data>>` where `Key : IComparable<Key>`

Min-heap priority queue.

- `PriorityQueue()` — empty
- `int Count { get; }`
- `void Clear()`
- `void Enqueue(Key key, Data data)` — add with priority
- `Key TopKey()` — smallest key (throws if empty)
- `Data Peek()` — data w/ smallest key (throws if empty)
- `Data Dequeue()` — remove & return data w/ smallest key (throws if empty)
- `bool Put(KeyValuePair<Key, Data> value)` — equivalent to Enqueue; always returns true
- `bool TryPop(out KeyValuePair<Key, Data> value)` — returns false if empty
- `DataEnumerable Values { get; }` — raw data list enumerable
- `Enumerable Enumerate(QueueEnumerationOrder order)` — iterate over `DataRef`

**PriorityQueue nested types:**
```csharp
public struct DataRef {
    public DataRef(PriorityQueue<Key, Data> owner, int id);
    public bool IsValid { get; }
    public Data ShowData();
    public Key ShowKey();
    public bool Update(Key newKey);   // invalidates this ref
    public bool Remove();             // invalidates this ref
}
public struct Enumerator : IEnumerator<DataRef> { ... }
public readonly struct Enumerable { ... }
public readonly struct DataEnumerable { ... }
```

### `PriorityQueueEx<Key, Data> : PriorityQueue<Key, Data>` where `Key : IComparable<Key>`
- `DataRef FindData(Func<Data, bool> finder)` — returns invalid DataRef if not found

---

### Concurrent Queue

| Class | Implements | Notes |
|---|---|---|
| `SystemConcurrentQueue<TData>` | `IConcurrentQueue<TData>` | Wraps `ConcurrentQueue<T>` |
| `LimitedConcurrentQueue<T>` | `IConcurrentQueue<T>` | Bounded, lock-free (CAS). Ctor: `(int capacity)` |
| `SingleReaderWriterConcurrentQueue<TData>` | `ISingleReaderWriterConcurrentQueue<TData>` | 1 producer / 1 consumer. Ctor: `(int initialCapacity = 10)`. Two internal buffers swapped via `Interlocked.Exchange` |
| `SynchronizedByLockConcurrentQueue<TData>` | `IConcurrentQueue<TData>` | Wraps any `IQueue<TData>` with `lock`. Ctor: `(IQueue<TData> queue, int maxCapacity = -1)` |
| `SynchronizedBySpinLockConcurrentQueue<TData>` | `IConcurrentQueue<TData>` | Wraps any `IQueue<TData>` with `SpinLock`. Ctor: `(IQueue<TData> queue, int maxCapacity = -1)` |

### Concurrent Stack

| Class | Implements | Notes |
|---|---|---|
| `SystemConcurrentStack<TData>` | `IConcurrentQueue<TData>` | Wraps `ConcurrentStack<T>`. NOTE: implements queue not stack interface |
| `SynchronizedByLockConcurrentStack<TData>` | `IConcurrentStack<TData>` | Backed by `Stack<T>` + `lock`. Ctor: `(int maxCapacity = -1)` |
| `SynchronizedBySpinLockConcurrentStack<TData>` | `IConcurrentStack<TData>` | Backed by `Stack<T>` + `SpinLock`. Ctor: `(int maxCapacity = -1)` |

### Concurrent Unordered Collection
- `SystemConcurrentUnorderedCollection<TData> : IConcurrentUnorderedCollection<TData>` — wraps `ConcurrentBag<T>`

### Dictionary / Map

| Class | Implements | Notes |
|---|---|---|
| `SystemDictionary<TKey, TData>` | `IMap<TKey, TData>` where `TKey : IEquatable<TKey>` | Wraps `Dictionary<TKey,TData>` |
| `SystemConcurrentDictionary<TKey, TData>` | `IConcurrentMap<TKey, TData>` | Wraps `ConcurrentDictionary<TKey,TData>` |
| `SynchronizedConcurrentDictionary<TKey, TData>` | `IConcurrentMap<TKey, TData>` | Wraps `Dictionary<TKey,TData>` + `ReaderWriterLockSlim`. Also: `ReplaceOrAdd(TKey, TData, out TData) → bool` (always returns true, always sets oldElement) |

### Set
- `SynchronizedConcurrentSet<TData> : IConcurrentSet<TData>` — wraps `HashSet<T>` + `ReaderWriterLockSlim`

### Delegate Adapters (struct wrappers)

| Struct | Implements | Ctor |
|---|---|---|
| `ConsumerDelegate<TData>` | `IConsumer<TData>` | `(Func<TData, bool> action)` |
| `ConcurrentConsumerDelegate<TData>` | `IConcurrentConsumer<TData>` | `(Func<TData, bool> action)` |
| `ProducerDelegate<TData>` | `IProducer<TData>` | `(Func<TData?> producerFunc)` — returns true if not null |
| `ConcurrentProducerDelegate<TData>` | `IConcurrentProducer<TData>` | `(Func<TData?> producerFunc)` |

---

## Extension Methods / Helpers

### `static class IArray_Ext`
- `bool EqualByContent<T>(this IReadOnlyArray<T>? self, IReadOnlyArray<T>? other) where T : IEquatable<T>` — content equality, handles nulls

### `static class IReadOnlyArray_Enumerators`
- `AsEnumerable<TData> Enumerate<TData>(this IReadOnlyArray<TData> list)` — enables `foreach` on any `IReadOnlyArray<TData>`

**Nested:**
```csharp
public readonly struct AsEnumerable<TData> : IEnumerable<TData> {
    public AsEnumerable(IReadOnlyArray<TData> array);
    public Enumerator<TData> GetEnumerator();
}
public struct Enumerator<TData> : IEnumerator<TData> { ... }
```

---

## Usage Patterns & Code Snippets

### Basic Queue/Stack
```csharp
// Non-concurrent queue (backed by System.Queue<T>)
var queue = new SystemQueue<int>();
queue.Put(1);           // → true
queue.Put(2);
queue.TryPop(out var v); // v = 1, true
queue.Clear();

// Stack
var stack = new SystemStack<string>();
stack.Put("a");
stack.TryPop(out var s); // s = "a"
```

### CycleQueue (circular buffer with indexer)
```csharp
var cq = new CycleQueue<int>(8);  // capacity = 8
cq.Put(10); cq.Put(20); cq.Put(30);
int head = cq.Head;   // 10, peek without remove
int tail = cq.Tail;   // 30
cq[1] = 25;           // set logical index 1

cq.PutToHead(5);      // prepend → [5,10,25,30]

// Enumerate in reverse
foreach (var x in cq.Enumerate(QueueEnumerationOrder.TailToHead)) { }

// Indexer range: 0 = head, Count-1 = tail
for (int i = 0; i < cq.Count; i++) Console.WriteLine(cq[i]);
```

### Priority Queue
```csharp
var pq = new PriorityQueue<int, string>();
pq.Enqueue(3, "three");
pq.Enqueue(1, "one");
pq.Enqueue(2, "two");

string min = pq.Peek();     // "one"  (key=1)
string top = pq.Dequeue();  // "one"
int topKey = pq.TopKey();   // 2

// IQueue<KeyValuePair<int,string>> interface
pq.Put(new KeyValuePair<int, string>(0, "zero"));
pq.TryPop(out var kv);      // kv = {0, "zero"}

// Find + update/remove via DataRef (with PriorityQueueEx)
var pqEx = new PriorityQueueEx<int, string>();
pqEx.Enqueue(1, "hello");
var ref = pqEx.FindData(s => s == "hello");
if (ref.IsValid) {
    ref.Update(10);   // change priority, ref becomes invalid
}
```

### Bounded concurrent queue (lock-free, CAS-based)
```csharp
var lcq = new LimitedConcurrentQueue<int>(100); // capacity 100
bool ok = lcq.Put(42);          // false if full
bool ok2 = lcq.TryPop(out var v); // false if empty
```

### Single-reader single-writer concurrent queue
```csharp
var srwq = new SingleReaderWriterConcurrentQueue<int>();
// Writer thread:
srwq.Put(1);
// Reader thread:
srwq.TryPop(out var val);
```

### Synchronized wrappers (decorate any IQueue with a lock)
```csharp
var inner = new SystemQueue<int>();
var locked = new SynchronizedByLockConcurrentQueue<int>(inner, maxCapacity: 1000);
// or with SpinLock:
var spinLocked = new SynchronizedBySpinLockConcurrentQueue<int>(inner);
```

### Concurrent Stack
```csharp
// NOTE: SystemConcurrentStack implements IConcurrentQueue<TData>, not IConcurrentStack<TData>
var stack = new SystemConcurrentStack<int>();
stack.Put(1);
stack.TryPop(out var v);

// True stack implementations:
var lockStack = new SynchronizedByLockConcurrentStack<int>();
var spinStack = new SynchronizedBySpinLockConcurrentStack<int>();
```

### Concurrent Dictionary
```csharp
// Wraps ConcurrentDictionary
var dict = new SystemConcurrentDictionary<string, int>();
dict.Add("a", 1);
dict.AddOrReplace("a", 2, out var old);  // old = 1
dict.AddOrGet("b", 10, out var val);     // val = 10
dict.GetAndRemove("a", out var removed); // removed = 2

// ReaderWriterLockSlim-based:
var syncDict = new SynchronizedConcurrentDictionary<string, int>();
syncDict.ReplaceOrAdd("x", 100, out var prev); // always succeeds
```

### Concurrent Set
```csharp
var set = new SynchronizedConcurrentSet<int>();
set.Put(1);
set.Contains(1);  // true
set.Remove(1);    // true
```

### Delegate wrappers (use any function as IConsumer/IProducer)
```csharp
// Use a lambda as a consumer
IConsumer<int> consumer = new ConsumerDelegate<int>(x => x > 0);
consumer.Put(5); // invokes lambda, returns true

// Use a factory as a producer
IProducer<int> producer = new ProducerDelegate<int>(() => GenerateValue());
producer.TryPop(out var val);
```

### Extension: compare IReadOnlyArray by content
```csharp
IReadOnlyArray<int> a = new CycleQueue<int>(8) { /* ... */ };
IReadOnlyArray<int> b = new CycleQueue<int>(8) { /* ... */ };
bool equal = a.EqualByContent(b);
```

### Extension: foreach over IReadOnlyArray
```csharp
IReadOnlyArray<int> arr = new CycleQueue<int>(8);
foreach (var x in arr.Enumerate()) { }
```
