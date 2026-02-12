using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Trader.Utils
{
    /// <summary>
    /// Represents an observable interface that provides the ability to monitor and react to changes
    /// in the underlying value. It supports subscriptions, value retrieval, and waiting for specific
    /// conditions to be met.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value stored in the observable field.
    /// </typeparam>
    public interface IObservableField<out T> : IObservable<T>
    {
        /// <summary>
        /// Gets the current value stored in the <see cref="ObservableField{T}"/>.
        /// Accessing this property provides a thread-safe mechanism to retrieve the value.
        /// When set, the value change may trigger notifications to observers if the
        /// <see cref="SetValue"/> method indicates a change has occurred.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Asynchronously waits for the specified predicate to be satisfied by the current value
        /// of the observable field. If the predicate is already satisfied, the task completes
        /// immediately. If the observable field is completed, the task completes with a result
        /// of <c>false</c>.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate to evaluate against the current value of the observable field.
        ///     The task completes with a result of <c>true</c> when this predicate returns <c>true</c>.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task completes with <c>true</c>
        /// if the predicate is satisfied, or with <c>false</c> if the observable field has been completed.
        /// </returns>
        Task<bool> WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The ObservableField class represents a reactive wrapper around a value,
    /// providing observable updates when the value is changed. It supports subscriptions,
    /// value change notifications, and waiting for specific predicate conditions to be satisfied.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value stored in the observable field.
    /// </typeparam>
    public class ObservableField<T> : IObservableField<T>
    {
        /// <summary>
        /// A private field of type <see cref="BehaviorSubject{T}"/> that serves as the underlying subject
        /// for the observable pattern implementation within the <see cref="ObservableField{T}"/> class.
        /// </summary>
        private readonly BehaviorSubject<T> _subject;   
        private T _value;
        private bool _completed;

        private readonly object _locker = new object();

        /// <summary>
        /// A collection of predicates waiting to be satisfied by the current value of the ObservableField.
        /// Each entry pairs a <see cref="TaskCompletionSource{TResult}"/> with a predicate that determines
        /// whether the current value meets a specific condition. When a predicate is satisfied, the associated
        /// task is completed, and the predicate is removed from the collection.
        /// </summary>
        /// <remarks>
        /// The predicates in this list are evaluated whenever the value of the ObservableField changes.
        /// If the ObservableField has been marked as completed, all tasks associated with the predicates
        /// will immediately complete with a result of <c>false</c>.
        /// </remarks>
        private List<(TaskCompletionSource<bool> tcs, Predicate<T> predicate, CancellationToken cToken)>? _waitingPredicates;
        
        public T Value
        {
            get
            {
                lock (_locker)
                {
                    return _value;
                }
            }
            set => SetValue(value);
        }

        public void SetValue(T value, bool checkEquality = true)
        {
            lock (_locker)
            {
                if (checkEquality && Equals(_value, value))
                {
                    return;
                }
                _value = value;
                if (_completed)
                {
                    return;
                }

                if (_waitingPredicates != null)
                {
                    for (int i = _waitingPredicates.Count - 1; i >= 0; i--)
                    {
                        if (_waitingPredicates[i].cToken.IsCancellationRequested)
                        {
                            _waitingPredicates[i] = _waitingPredicates[_waitingPredicates.Count - 1];
                            _waitingPredicates.RemoveAt(_waitingPredicates.Count - 1);
                        }
                        else if (_waitingPredicates[i].predicate(_value))
                        {
                            var tcs = _waitingPredicates[i].tcs;
                            Task.Run(() => tcs.SetResult(true));
                            
                            _waitingPredicates[i] = _waitingPredicates[_waitingPredicates.Count - 1];
                            _waitingPredicates.RemoveAt(_waitingPredicates.Count - 1);
                        }
                    }
                }
            }
            _subject.OnNext(value);
        }

        /// <summary>
        /// Asynchronously waits for the specified predicate to be satisfied by the current value
        /// of the observable field. If the predicate is already satisfied, the task completes
        /// immediately. If the observable field is completed, the task completes with a result
        /// of <c>false</c>.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate to evaluate against the current value of the observable field.
        ///     The task completes with a result of <c>true</c> when this predicate returns <c>true</c>.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task completes with <c>true</c>
        /// if the predicate is satisfied, or with <c>false</c> if the observable field has been completed.
        /// </returns>
        public Task<bool> WaitFor(Predicate<T> predicate, CancellationToken cancellationToken = default)
        {
            lock (_locker)
            {            
                if (_completed)
                {
                    return Task.FromResult(false);
                }
                
                if (predicate(_value))
                {
                    return Task.FromResult(true);
                }

                _waitingPredicates ??= new();
                
                TaskCompletionSource<bool> tcs = new();
                _waitingPredicates.Add((tcs, predicate, cancellationToken));
                return tcs.Task;
            }
        }

        /// Subscribes the provided observer to the observable field. The observer will receive notifications
        /// of updates to the field's value. If the observable field has already been completed, the current
        /// value will be sent to the observer immediately, followed by a completion notification.
        /// <param name="observer">The observer that will receive updates from the observable field.</param>
        /// <return>Returns a disposable object that can be used to unsubscribe the observer from the notifications.</return>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            bool completed;
            lock (_locker)
            {
                completed = _completed;
            }

            if (completed)
            {
                observer.OnNext(_value);
                observer.OnCompleted();
                return Disposable.Empty;
            }
            
            return _subject.Subscribe(observer);
        }

        public ObservableField(T value)
        {
            _value = value;
            _subject = new BehaviorSubject<T>(value);
        }

        /// <summary>
        /// Marks the observable field as completed, preventing any further value updates or subscriptions.
        /// Upon completion, the field notifies all subscribers and terminates any pending predicate-based waits with a result of <c>false</c>.
        /// </summary>
        public void Complete()
        {
            bool complete = false;
            lock (_locker)
            {
                if (!_completed)
                {
                    _completed = true;
                    complete = true;
                    
                    if (_waitingPredicates != null)
                    {
                        foreach (var (tcs, _, cToken) in _waitingPredicates)
                        {
                            if (!cToken.IsCancellationRequested)
                            {
                                Task.Run(() => tcs.SetResult(false));
                            }
                        }
                        _waitingPredicates.Clear();
                    }
                }
            }

            if (complete)
            {
                _subject.OnCompleted();
            }
        }

        /// <summary>
        /// Defines an implicit conversion operator for converting an
        /// <see cref="ObservableField{T}"/> instance to its underlying value of type <c>T</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value held by the <see cref="ObservableField{T}"/>.
        /// </typeparam>
        /// <param name="field">
        /// The <see cref="ObservableField{T}"/> instance to convert to its value.
        /// </param>
        /// <returns>
        /// The current value of the <paramref name="field"/> instance.
        /// </returns>
        public static implicit operator T(ObservableField<T> field)
        {
            return field.Value;
        }

        /// Converts the current value of the `ObservableField` to its string representation.
        /// <return>
        /// Returns the string representation of the current value held by the `ObservableField`,
        /// or null if the current value is null.
        /// </return>
        public override string? ToString()
        {
            return Value?.ToString();
        }
    }
}