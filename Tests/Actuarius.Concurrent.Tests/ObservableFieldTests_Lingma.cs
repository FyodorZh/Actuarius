using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Actuarius.Concurrent.Tests;

[TestFixture]
public class ObservableFieldTests_Lingma
{
    [Test]
    public void Constructor_InitializesWithValue()
    {
        var field = new ObservableField<int>(42);
        
        Assert.That(field.Value, Is.EqualTo(42));
    }

    [Test]
    public void Constructor_NullValue_AllowedForReferenceTypes()
    {
        var field = new ObservableField<string?>(null);
        
        Assert.That(field.Value, Is.Null);
    }

    [Test]
    public void SetValue_UpdatesValue()
    {
        var field = new ObservableField<int>(10);
        
        field.Value = 20;
        
        Assert.That(field.Value, Is.EqualTo(20));
    }

    [Test]
    public void SetValue_SameValue_DoesNotNotifyObservers()
    {
        var field = new ObservableField<int>(10);
        var notificationCount = 0;
        
        field.Subscribe(_ => notificationCount++);
        field.Value = 10; // Same value
        
        Assert.That(notificationCount, Is.EqualTo(1));
    }

    [Test]
    public void SetValue_DifferentValue_NotifiesObservers()
    {
        var field = new ObservableField<int>(10);
        var receivedValues = new List<int>();
        
        field.Subscribe(receivedValues.Add);
        field.Value = 20;
        
        Assert.That(receivedValues, Has.Count.EqualTo(2));
        Assert.That(receivedValues[0], Is.EqualTo(10));
        Assert.That(receivedValues[1], Is.EqualTo(20));
    }

    [Test]
    public void SetValue_WithCheckEqualityFalse_AlwaysNotifies()
    {
        var field = new ObservableField<int>(10);
        var notificationCount = 0;
        
        field.Subscribe(_ => notificationCount++);
        field.SetValue(10, checkEquality: false);
        
        Assert.That(notificationCount, Is.EqualTo(2));
    }

    [Test]
    public void Subscribe_ImmediatelyReceivesCurrentValue()
    {
        var field = new ObservableField<string>("initial");
        var receivedValues = new List<string>();
        
        field.Subscribe(receivedValues.Add);
        
        Assert.That(receivedValues, Has.Count.EqualTo(1));
        Assert.That(receivedValues[0], Is.EqualTo("initial"));
    }

    [Test]
    public void Subscribe_MultipleSubscribers_AllReceiveNotifications()
    {
        var field = new ObservableField<int>(0);
        var subscriber1Values = new List<int>();
        var subscriber2Values = new List<int>();
        
        field.Subscribe(subscriber1Values.Add);
        field.Subscribe(subscriber2Values.Add);
        
        field.Value = 5;
        field.Value = 10;
        
        Assert.That(subscriber1Values, Is.EquivalentTo(new[] { 0, 5, 10 }));
        Assert.That(subscriber2Values, Is.EquivalentTo(new[] { 0, 5, 10 }));
    }

    [Test]
    public void Subscribe_Unsubscribe_StopsNotifications()
    {
        var field = new ObservableField<int>(0);
        var receivedValues = new List<int>();
        
        var subscription = field.Subscribe(receivedValues.Add);
        subscription.Dispose();
        
        field.Value = 5;
        
        Assert.That(receivedValues, Has.Count.EqualTo(1)); // Only initial value
        Assert.That(receivedValues[0], Is.EqualTo(0));
    }

    [Test]
    public async Task WaitFor_PredicateAlreadySatisfied_CompletesImmediately()
    {
        var field = new ObservableField<int>(10);
        
        var result = await field.WaitFor(x => x > 5);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task WaitFor_PredicateNotSatisfied_WaitsForChange()
    {
        var field = new ObservableField<int>(5);
        var waitForTask = field.WaitFor(x => x > 10);
        
        Assert.That(waitForTask.IsCompleted, Is.False);
        
        field.Value = 15;
        
        var result = await waitForTask;
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task WaitFor_MultiplePredicates_EachSatisfiedIndependently()
    {
        var field = new ObservableField<int>(0);
        
        var task1 = field.WaitFor(x => x >= 5);
        var task2 = field.WaitFor(x => x >= 10);
        
        field.Value = 5;
        var result1 = await task1;
        Assert.That(result1, Is.True);
        Assert.That(task2.IsCompleted, Is.False);
        
        field.Value = 10;
        var result2 = await task2;
        Assert.That(result2, Is.True);
    }

    [Test]
    public async Task WaitFor_CancellationToken_CancelledBeforePredicateSatisfied()
    {
        var field = new ObservableField<int>(0);
        var cts = new CancellationTokenSource();
        
        var waitForTask = field.WaitFor(x => x > 10, cts.Token);
        cts.Cancel();
        
        try
        {
            await waitForTask;
            Assert.Fail("Expected TaskCanceledException");
        }
        catch (TaskCanceledException)
        {
            // Expected
        }
    }

    [Test]
    public async Task WaitFor_CancellationToken_CancelledAfterWaiting()
    {
        var field = new ObservableField<int>(0);
        var cts = new CancellationTokenSource();
        
        var waitForTask = field.WaitFor(x => x > 10, cts.Token);
        
        await Task.Delay(50);
        cts.Cancel();
        
        try
        {
            await waitForTask;
            Assert.Fail("Expected TaskCanceledException");
        }
        catch (TaskCanceledException)
        {
            // Expected
        }
    }

    [Test]
    public void Complete_PreventsFurtherUpdates()
    {
        var field = new ObservableField<int>(10);
        var notificationCount = 0;
        
        field.Subscribe(_ => notificationCount++);
        field.Complete();
        
        field.Value = 20;
        
        Assert.That(notificationCount, Is.EqualTo(1)); // Only initial subscription
        Assert.That(field.Value, Is.EqualTo(10)); // Value unchanged
    }

    [Test]
    public void Complete_SubscriberReceivesCompletion()
    {
        var field = new ObservableField<int>(42);
        var onNextCalled = false;
        var onCompletedCalled = false;
        
        var observer = Observer.Create<int>(
            onNext: _ => onNextCalled = true,
            onCompleted: () => onCompletedCalled = true
        );
        
        field.Subscribe(observer);
        field.Complete();
        
        Assert.That(onNextCalled, Is.True);
        Assert.That(onCompletedCalled, Is.True);
    }

    [Test]
    public void Complete_AfterCompletion_WaitForReturnsFalse()
    {
        var field = new ObservableField<int>(5);
        field.Complete();
        
        var result = field.WaitFor(x => x > 10).Result;
        
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Complete_PendingWaitFor_ReturnsFalse()
    {
        var field = new ObservableField<int>(5);
        var waitForTask = field.WaitFor(x => x > 10);
        
        await Task.Delay(50);
        field.Complete();
        
        var result = await waitForTask;
        Assert.That(result, Is.False);
    }

    [Test]
    public void Complete_MultipleTimes_NoError()
    {
        var field = new ObservableField<int>(10);
        
        Assert.DoesNotThrow(() =>
        {
            field.Complete();
            field.Complete();
            field.Complete();
        });
    }

    [Test]
    public void Complete_UnsubscribesFromSubject()
    {
        var field = new ObservableField<int>(0);
        var receivedValues = new List<int>();
        
        field.Subscribe(receivedValues.Add);
        field.Complete();
        
        field.SetValue(5, checkEquality: false);
        
        Assert.That(receivedValues, Has.Count.EqualTo(1));
        Assert.That(receivedValues[0], Is.EqualTo(0));
    }

    [Test]
    public void ImplicitConversion_ObservableFieldToValue()
    {
        var field = new ObservableField<double>(3.14);
        double value = field;
        
        Assert.That(value, Is.EqualTo(3.14));
    }

    [Test]
    public void ToString_ReturnsValueString()
    {
        var field = new ObservableField<int>(123);
        
        var result = field.ToString();
        
        Assert.That(result, Is.EqualTo("123"));
    }

    [Test]
    public void ToString_NullValue_ReturnsNull()
    {
        var field = new ObservableField<string?>(null);
        
        var result = field.ToString();
        
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Subscribe_AfterComplete_ReceivesValueAndCompletesImmediately()
    {
        var field = new ObservableField<string>("test");
        field.Complete();
        
        var receivedValues = new List<string>();
        var completed = false;
        
        field.Subscribe(
            onNext: receivedValues.Add,
            onCompleted: () => completed = true
        );
        
        Assert.That(receivedValues, Has.Count.EqualTo(1));
        Assert.That(receivedValues[0], Is.EqualTo("test"));
        Assert.That(completed, Is.True);
    }

    [Test]
    public async Task WaitFor_PredicateWithComplexLogic()
    {
        var field = new ObservableField<List<int>>(new List<int>());
        
        var waitForTask = field.WaitFor(list => list.Count >= 3 && list.Sum(x => x) > 10);
        
        field.Value = new List<int> { 1, 2 };
        Assert.That(waitForTask.IsCompleted, Is.False);
        
        field.Value = new List<int> { 1, 2, 3 };
        Assert.That(waitForTask.IsCompleted, Is.False); // Sum is 6, not > 10
        
        field.Value = new List<int> { 4, 5, 6 };
        var result = await waitForTask;
        Assert.That(result, Is.True);
    }

    [Test]
    public void ThreadSafety_ConcurrentSetValue()
    {
        var field = new ObservableField<int>(0);
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();
        
        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        field.Value = threadId * 1000 + j;
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        Assert.That(exceptions, Is.Empty);
    }

    [Test]
    public void ThreadSafety_ConcurrentWaitFor()
    {
        var field = new ObservableField<int>(0);
        var tasks = new List<Task<bool>>();
        
        for (int i = 0; i < 10; i++)
        {
            int threshold = i * 100;
            tasks.Add(field.WaitFor(x => x >= threshold));
        }
        
        Task.Run(async () =>
        {
            for (int i = 0; i < 1000; i += 10)
            {
                field.Value = i;
                await Task.Delay(10);
            }
        });
        
        var results = Task.WhenAll(tasks).Result;
        
        Assert.That(results, Has.All.True);
    }

    [Test]
    public void IObservableField_InterfaceImplementation()
    {
        IObservableField<int> interfaceField = new ObservableField<int>(42);
        
        Assert.That(interfaceField.Value, Is.EqualTo(42));
        
        var waitTask = interfaceField.WaitFor(x => x > 40);
        Assert.That(waitTask.Result, Is.True);
    }

    [Test]
    public void SetValue_ReferenceType_ValueTypes()
    {
        var field = new ObservableField<string>("hello");
        var notifications = new List<string>();
        
        field.Subscribe(notifications.Add);
        
        field.Value = "world";
        field.Value = "world"; // Same value
        field.Value = "hello";
        
        Assert.That(notifications, Is.EquivalentTo(new[] { "hello", "world", "hello" }));
    }

    [Test]
    public async Task Subscribe_MultipleTimes_ReturnsDifferentDisposables()
    {
        var field = new ObservableField<int>(0);
        
        var subscription1 = field.Subscribe(_ => { });
        var subscription2 = field.Subscribe(_ => { });
        
        Assert.That(subscription1, Is.Not.SameAs(subscription2));
        
        subscription1.Dispose();
        field.Value = 1;
        
        // Subscription2 should still work
        Assert.That(subscription2, Is.Not.Null);
    }
}
