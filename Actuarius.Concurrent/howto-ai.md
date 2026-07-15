# Actuarius.Concurrent

Target: `netstandard2.1`, LangVersion 9, Nullable enabled.  
NuGet: `Actuarius.Concurrent` v0.1.11-dev.0  
Depends on: `Actuarius.Collections`, `System.Reactive` 6.1.0

All types in namespace `Actuarius.Concurrent`.

---

## Overview

Thread-safe concurrency primitives: observable reactive wrappers (`ObservableField`, `ObservableSet`, `ObservableDictionary`), serialized executor, intent state machine, atomic value box, comparers, and collection valves with open/close gate semantics.

---

## ObservableField

### `interface IObservableField<out T> : IObservable<T>`
- `T Value { get; }` — current value
- `Task<bool> WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default)` — async wait until predicate is satisfied; returns `false` if field completed
- `IDisposable Subscribe(IObserver<T> observer)` — inherit from `IObservable<T>`

### `class ObservableField<T> : IObservableField<T>`
- Ctor: `(T value)`
- `T Value { get; set; }` — getter; setter calls `SetValue(value)`
- `void SetValue(T value, bool checkEquality = true)` — set value; if `checkEquality` and new == current, no notification. Does nothing if completed
- `Task<bool> WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default)` — returns `false` if field completed
- `IDisposable Subscribe(IObserver<T> observer)` — if completed, immediately sends value + `OnCompleted`
- `void Complete()` — terminal; prevents mutations, notifies subscribers, resolves pending WaitFor with `false`
- `static implicit operator T(ObservableField<T> field)` — convert to `T`
- `override string? ToString()`

### Usage
```csharp
var field = new ObservableField<int>(0);

// Read/write
field.Value = 42;
int val = field.Value;

// Implicit conversion
int x = field;   // field is ObservableField<int>

// Subscribe
field.Subscribe(v => Console.WriteLine(v));
field.Subscribe(
    onNext: v => { },
    onCompleted: () => Console.WriteLine("done")
);
var sub = field.Subscribe(Observer.Create<int>(...));
sub.Dispose();

// Async wait
bool satisfied = await field.WaitFor(v => v > 100);
bool cancelled = await field.WaitFor(v => v > 10, cts.Token);

// Complete — terminal
field.Complete();
field.Value = 99;  // silently ignored
await field.WaitFor(v => v > 0);  // returns false immediately
```

---

## ObservableSet

### `interface IObservableSet<TData> : IObservable<ObservableSetPatch<TData>>`
- `IObservableSet<TNew> ConvertTo<TNew>(Func<TData, TNew> converter)` — default interface method; returns `ObservableSet_Converter<TData, TNew>`

### `class ObservableSet<TData> : IObservableSet<TData>, Collections.ISet<TData>` where `TData : notnull`
- Ctor: `(IEqualityComparer<TData>? comparer = null)`
- `IDisposable Subscribe(IObserver<ObservableSetPatch<TData>> observer)` — if completed, immediate `OnCompleted`; if non-empty, initial patch with all elements
- `void Clear()` — emits patch with all elements as removed
- `void Set(IEnumerable<TData> elements)` — full replacement (diff emitted). Throws `InvalidOperationException` on duplicate keys
- `void Append(IEnumerable<TData> elements)` — add elements. Throws `ArgumentNullException` if null
- `void Remove(IEnumerable<TData> elements)` — remove elements. Same throw
- `void Complete()` — terminal, no further patches
- `TData[] ToArray()` — snapshot
- `bool Put(TData value)` — add single; returns `false` if already present or completed; emits patch
- `bool Remove(TData element)` — remove single; returns `false` if absent or completed; emits patch
- `bool Contains(TData element)` — membership check

### `class ObservableSet_Converter<From, To> : IObservableSet<To>`
- Ctor: `(IObservableSet<From> core, Func<From, To> converter)`

### `readonly struct ObservableSetPatch<T>`
- Ctor: `(IReadOnlyList<T> added, IReadOnlyList<T> removed)`
- `IReadOnlyList<T> Added { get; }`
- `IReadOnlyList<T> Removed { get; }`
- `void ApplyTo(ISet<T> set)` — applies patch (remove then add)

### Usage
```csharp
var set = new ObservableSet<string>();

// Subscribe to patches
set.Subscribe(patch => {
    // patch.Added, patch.Removed
});

// Full replacement (with diff)
set.Set(new[] { "A", "B", "C" });
set.Set(new[] { "A", "D" });  // patch: removed=["B","C"], added=["D"]

// Incremental changes
set.Append(new[] { "E", "F" });
set.Remove(new[] { "A" });
set.Put("G");              // returns true
set.Remove("Z");           // returns false, no patch

// No-change optimization — no notification emitted
set.Set(new[] { "A", "B" });
set.Set(new[] { "A", "B" });  // identical — no patch

// Snapshot
string[] snapshot = set.ToArray();

// Terminal
set.Complete();

// Via IObservableSet.ConvertTo
IObservableSet<int> intSet = set.ConvertTo(s => s.Length);
```

---

## ObservableDictionary

### `interface IObservableDictionary<TKey, TData> : IObservable<ObservableDictionaryPatch<TKey, TData>>` where `TKey : notnull`
- `IObservableDictionary<TKey, TNewData> ConvertDataTo<TNewData>(Func<TData, TNewData> converter)` — default interface method
- `IObservableDictionary<TNewKey, TNewData> ConvertTo<TNewKey, TNewData>(Func<TKey, TNewKey> keyConverter, Func<TData, TNewData> dataConverter) where TNewKey : notnull` — default interface method

### `class ObservableDictionary<TKey, TData> : IObservableDictionary<TKey, TData>, IMap<TKey, TData>` where `TKey : notnull`
- Ctor: `(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TData>? dataComparer = null)`
- `IDisposable Subscribe(IObserver<ObservableDictionaryPatch<TKey, TData>> observer)` — if completed, immediate `OnCompleted`; if non-empty, initial patch with all entries
- `void Clear()` — emits patch with all keys as removed
- `void Set(IEnumerable<KeyValuePair<TKey, TData>> elements)` — full replacement (diff with additions, updates, removals). Throws `InvalidOperationException` on duplicate key in input
- `void AddOrUpdate(IEnumerable<KeyValuePair<TKey, TData>> elements)` — upsert (never removes)
- `void RemoveMany(IEnumerable<TKey> keys)` — batch remove
- `void Complete()` — terminal
- `KeyValuePair<TKey, TData>[] ToArray()` — snapshot

**IMap<TKey,TData> implementation:**
- `bool Add(TKey key, TData element)` — adds or updates; emits update if key exists
- `bool Remove(TKey key)` — removes if present; emits patch
- `bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TData element)`

### `class ObservableDictionary_Converter<TKey, TData, TKeyFrom, TDataFrom> : IObservableDictionary<TKey, TData>` where both key types are `notnull`
- Ctor: `(IObservableDictionary<TKeyFrom, TDataFrom> core, Func<TKeyFrom, TKey> keyConverter, Func<TDataFrom, TData> dataConverter)`

### `class ObservableDictionary_Converter_Data<TKey, TFromData, TData> : IObservableDictionary<TKey, TData>` where `TKey : notnull`
- Ctor: `(IObservableDictionary<TKey, TFromData> core, Func<TFromData, TData> converter)` — data-only conversion

### `readonly struct ObservableDictionaryPatch<TKey, TValue>`
- Ctor: `(IReadOnlyList<KeyValuePair<TKey, TValue>> added, IReadOnlyList<KeyValuePair<TKey, TValue>> updated, IReadOnlyList<TKey> removed)`
- `IReadOnlyList<KeyValuePair<TKey, TValue>> Added { get; }`
- `IReadOnlyList<KeyValuePair<TKey, TValue>> Updated { get; }`
- `IReadOnlyList<TKey> Removed { get; }`
- `void ApplyTo(IDictionary<TKey, TValue> dictionary)` — removes, then adds, then updates

### Usage
```csharp
var dict = new ObservableDictionary<string, int>();

// Subscribe to patches
dict.Subscribe(patch => {
    // patch.Added, patch.Updated, patch.Removed
});

// Full replacement
dict.Set(new[] {
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("B", 2)
});
dict.Set(new[] {
    new KeyValuePair<string, int>("B", 20),  // update
    new KeyValuePair<string, int>("C", 3)    // add
});  // patch: removed=["A"], updated=[("B",20)], added=[("C",3)]

// Incremental
dict.AddOrUpdate(new[] {
    new KeyValuePair<string, int>("C", 30),  // update
    new KeyValuePair<string, int>("D", 4)    // add
});
dict.RemoveMany(new[] { "B" });
dict.Clear();

// Via IMap interface
dict.Add("key", 10);      // add or update
dict.TryGetValue("key", out var val);
dict.Remove("key");

// Conversion
IObservableDictionary<string, string> strDict = dict.ConvertDataTo(d => d.ToString());
IObservableDictionary<string, string> conv = dict.ConvertTo(
    k => k,
    d => d.ToString()
);
```

---

## Valve Classes (Gate Pattern)

### `enum ValveEnqueueResult` — `Ok = 0`, `Overflown = 1`, `Rejected = 2`

### `class SingleReaderWriterConcurrentUnorderedCollectionValve<TData> : ISingleReaderWriterConcurrentUnorderedCollection<TData>`
- Ctors:
  - `(ISingleReaderWriterConcurrentUnorderedCollection<TData> collection, Action<TData> processor)` — same action for unfinished + rejected
  - `(collection, Action<TData> unfinishedProcessor, Action<TData> rejectedProcessor)`
- `void CloseValve()` — triggers drain callback after all pending sections exit
- `ValveEnqueueResult EnqueueEx(TData value)` — returns `Ok`, `Overflown`, or `Rejected`
- `bool Put(TData value)` — returns `false` if overflown (rejected returns `true`)
- `bool TryPop([MaybeNullWhen(false)] out TData value)` — returns `false` if closed or empty

### `class ConcurrentUnorderedCollectionValve<TData> : SingleReaderWriterConcurrentUnorderedCollectionValve<TData>, IConcurrentUnorderedCollection<TData>`
- Ctors: `(IConcurrentUnorderedCollection<TData> collection, Action<TData> processor)` or `(collection, Action<TData> unfinished, Action<TData> rejected)`

### `class SingleReaderWriterConcurrentQueueValve<TData> : SingleReaderWriterConcurrentUnorderedCollectionValve<TData>, ISingleReaderWriterConcurrentQueue<TData>`
- `int Count { get; }` — delegates to underlying collection
- Ctors: `(ISingleReaderWriterConcurrentQueue<TData> collection, Action<TData> processor)` or `(collection, Action<TData> unfinished, Action<TData> rejected)`

### `class ConcurrentQueueValve<TData> : ConcurrentUnorderedCollectionValve<TData>, IConcurrentQueue<TData>`
- `int Count { get; }`
- Ctors: `(IConcurrentQueue<TData> collection, Action<TData> processor)` or `(collection, Action<TData> unfinished, Action<TData> rejected)`

### Usage
```csharp
var inner = new LimitedConcurrentQueue<int>(100);
var valve = new ConcurrentQueueValve<int>(inner, data => Process(data));

ValveEnqueueResult r = valve.EnqueueEx(42);  // Ok, Overflown, or Rejected
bool ok = valve.Put(43);                      // false if overflown
valve.TryPop(out int val);                    // through the gate

valve.CloseValve();  // drains remaining items through processor
```

---

## Concurent Primitives

### `class ConcurrentSerializedExecutor`
- Ctor: `(Action action)`
- `void ScheduleOneInvokation()` — serialized execution. Only one call runs at a time; concurrent callers queue. All pending invocations complete before new ones. Exceptions collected into `AggregateException`.

```csharp
var executor = new ConcurrentSerializedExecutor(() => {
    // this action is never called concurrently with itself
});
executor.ScheduleOneInvokation();
```

### `sealed class Intention`
- Lock-free 3-state machine: `NoIntention` → `HasIntention` → `HadIntention`
- `State CurState { get; }` — `Intention.State` enum: `NoIntention`, `HasIntention`, `HadIntention`
- `bool Set()` — `NoIntention`→`HasIntention`; returns `true` if set
- `bool TryToRealize()` — `HasIntention`→`HadIntention`; returns `true` if realized
- `bool SetAndRealize()` — `Set()` + `TryToRealize()`; returns `true` if realized

```csharp
var intent = new Intention();
bool registered = intent.Set();          // true
bool realized = intent.TryToRealize();   // true
bool done = intent.SetAndRealize();      // one-shot: Set+TryToRealize
```

### `class AtomicBox<T>` where `T : struct`
- `T Value { get; set; }` — thread-safe (uses `ReaderWriterLockSlim`)
- `void Update(Func<T, T> update)` — atomic read-modify-write under write lock

```csharp
var box = new AtomicBox<int>();
box.Value = 5;
box.Update(x => x + 1);  // atomic increment
int val = box.Value;     // 6
```

### `class ConcurrentManualLockGate`
- Ctor: `(Action onClose)` — gate starts open; immediately enters initial critical section
- `bool Enter()` — enter session; returns `false` if gate closed
- `void Exit()` — exit session; triggers `onClose` when count reaches 0
- `void TryClose()` — atomically marks closed and calls `Exit()`

### `class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>` where `TKey : notnull`
- Ctor: `(Func<T, TKey> keyExtractor)`
- `virtual bool Equals(T? x, T? y)` — compares extracted keys
- `int GetHashCode(T obj)` — hash of extracted key

### `class StrictKeyEqualityComparer<T, TKey> : KeyEqualityComparer<T, TKey>` where `TKey : IEquatable<TKey>`
- Ctor: `(Func<T, TKey> keyExtractor)`
- `override bool Equals(T? x, T? y)` — uses `IEquatable<TKey>.Equals`

### `abstract class InvariantChecker<TState>` where `TState : struct`
- Protected ctor: `(TState state, Action<string> onFail)`
- `TState State { get; }` — current state
- `abstract TState ToState(int state)` / `abstract int FromState(TState state)`
- `TState SetState(TState newState)` — `Interlocked.Exchange`
- `void CheckState(TState expected)` — fail if mismatch
- `void ChangeState(TState expected, TState newState)` — fail if wrong expected
- `void BeginCriticalSection(ref int sectionId)` / `void EndCriticalSection(ref int sectionId)` — nesting check
- `void StateFail(TState expected, TState received)` — formatted failure
- `void Fail(string error = "")` — invariant violation

### Usage: KeyEqualityComparer
```csharp
var comparer = new KeyEqualityComparer<Person, int>(p => p.Id);
var dict = new Dictionary<Person, string>(comparer);
```

### Usage: AtomicBox
```csharp
var box = new AtomicBox<int>();
box.Value = 10;
box.Update(v => v * 2);  // atomically set to 20
```

### Usage: ConcurrentSerializedExecutor
```csharp
var exec = new ConcurrentSerializedExecutor(() => {
    Thread.Sleep(10);  // never runs concurrently
});
exec.ScheduleOneInvokation();
```
