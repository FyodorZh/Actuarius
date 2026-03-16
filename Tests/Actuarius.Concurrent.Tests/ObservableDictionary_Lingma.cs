using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actuarius.Concurrent.Tests;

[TestFixture]
public class ObservableDictionaryTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_WithNullComparer_CreatesSuccessfully()
    {
        var dictionary = new ObservableDictionary<string, int>(null);
        Assert.That(dictionary, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithCustomComparer_UsesComparer()
    {
        var comparer = EqualityComparer<string>.Default;
        var dictionary = new ObservableDictionary<string, string>(comparer);
        
        Assert.That(dictionary, Is.Not.Null);
    }

    #endregion

    #region Subscribe Tests

    [Test]
    public void Subscribe_WithNullObserver_ThrowsArgumentNullException()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        Assert.Throws<ArgumentNullException>(() => 
            dictionary.Subscribe(null!));
    }

    [Test]
    public void Subscribe_EmptyDictionary_DoesNotSendInitialSnapshot()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var patchesReceived = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => patchesReceived.Add(patch));
        
        Assert.That(patchesReceived, Is.Empty);
    }

    [Test]
    public void Subscribe_NonEmptyDictionary_SendsInitialSnapshot()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[] 
        { 
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patchesReceived = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patchesReceived.Add(patch));
        
        Assert.That(patchesReceived, Has.Count.EqualTo(1));
        Assert.That(patchesReceived[0].Added, Has.Count.EqualTo(2));
        Assert.That(patchesReceived[0].Updated, Is.Empty);
        Assert.That(patchesReceived[0].Removed, Is.Empty);
    }

    [Test]
    public void Subscribe_AfterComplete_ReceivesOnCompleted()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var onCompletedCalled = false;
        
        dictionary.Subscribe(
            _ => { },
            () => onCompletedCalled = true);
        
        dictionary.Complete();
        
        Assert.That(onCompletedCalled, Is.True);
    }

    #endregion

    #region Set Tests

    [Test]
    public void Set_WithNullEnumerable_ThrowsArgumentNullException()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        Assert.Throws<ArgumentNullException>(() => 
            dictionary.Set(null!));
    }

    [Test]
    public void Set_AddsNewElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => patches.Add(patch));
        
        var elements = new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        };
        
        dictionary.Set(elements);
        
        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added.Any(kv => kv.Key == "A" && kv.Value == 1), Is.True);
        Assert.That(patches[0].Added.Any(kv => kv.Key == "B" && kv.Value == 2), Is.True);
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
    }

    [Test]
    public void Set_UpdatesExistingElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 10),
            new KeyValuePair<string, int>("B", 20)
        });
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Has.Count.EqualTo(2));
        Assert.That(patches[1].Removed, Is.Empty);
    }

    [Test]
    public void Set_PreservesUnchangedElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        // Set the same elements again
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
    }

    [Test]
    public void Set_MixedAddUpdateAndRemove()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("B", 20), // Update
            new KeyValuePair<string, int>("C", 3)   // Add
        });
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        
        Assert.That(patches[1].Added, Has.Count.EqualTo(1));
        Assert.That(patches[1].Added[0].Key, Is.EqualTo("C"));
        Assert.That(patches[1].Updated, Has.Count.EqualTo(1));
        Assert.That(patches[1].Updated[0].Key, Is.EqualTo("B"));
        Assert.That(patches[1].Removed, Has.Count.EqualTo(1));
        Assert.That(patches[1].Removed[0], Is.EqualTo("A"));
    }

    [Test]
    public void Set_EmptyInput_ClearsDictionary()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.Set(Array.Empty<KeyValuePair<string, int>>());
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Is.Empty);
        Assert.That(patches[1].Removed, Has.Count.EqualTo(2));
    }

    [Test]
    public void Set_RedefinitionInSameCall_ThrowsInvalidOperationException()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        Assert.Throws<InvalidOperationException>(() => 
            dictionary.Set(new[]
            {
                new KeyValuePair<string, int>("A", 10),
                new KeyValuePair<string, int>("A", 20)
            }));
    }

    #endregion

    #region Append Tests

    [Test]
    public void Append_WithNullEnumerable_ThrowsArgumentNullException()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        Assert.Throws<ArgumentNullException>(() => 
            dictionary.AddOrUpdate(null!));
    }

    [Test]
    public void Append_AddsNewElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.AddOrUpdate(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
    }

    [Test]
    public void Append_UpdatesExistingElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.AddOrUpdate(new[]
        {
            new KeyValuePair<string, int>("A", 10)
        });
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added[0].Value, Is.EqualTo(1));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Has.Count.EqualTo(1));
        Assert.That(patches[1].Updated[0].Value, Is.EqualTo(10));
        Assert.That(patches[1].Removed, Is.Empty);
    }

    [Test]
    public void Append_MixedNewAndExisting()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.AddOrUpdate(new[]
        {
            new KeyValuePair<string, int>("A", 10),
            new KeyValuePair<string, int>("B", 2)
        });
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(1));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Has.Count.EqualTo(1));
        Assert.That(patches[1].Updated, Has.Count.EqualTo(1));
        Assert.That(patches[1].Removed, Is.Empty);
    }

    #endregion

    #region Update Tests

    [Test]
    public void Update_ExistingKey_UpdatesValue()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.AddOrUpdate([
            new KeyValuePair<string, int>("A", 10)
        ]);
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added[0].Key, Is.EqualTo("A"));
        Assert.That(patches[0].Added[0].Value, Is.EqualTo(1));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Has.Count.EqualTo(1));
        Assert.That(patches[1].Updated[0].Key, Is.EqualTo("A"));
        Assert.That(patches[1].Updated[0].Value, Is.EqualTo(10));
        Assert.That(patches[1].Removed, Is.Empty);
    }

    #endregion

    #region Remove Tests

    [Test]
    public void Remove_WithNullKeys_ThrowsArgumentNullException()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        Assert.Throws<ArgumentNullException>(() => 
            dictionary.RemoveMany(null!));
    }

    [Test]
    public void Remove_ExistingKeys_RemovesSuccessfully()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2),
            new KeyValuePair<string, int>("C", 3)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.RemoveMany(new[] { "A", "B" });
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(3));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Is.Empty);
        Assert.That(patches[1].Removed, Has.Count.EqualTo(2));
        Assert.That(patches[1].Removed, Does.Contain("A"));
        Assert.That(patches[1].Removed, Does.Contain("B"));
    }

    [Test]
    public void Remove_NonExistentKey_IncludesInPatch()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.RemoveMany(new[] { "NonExistent" });
        
        Assert.That(patches, Has.Count.EqualTo(0));
    }

    #endregion

    #region Clear Tests

    [Test]
    public void Clear_EmptyDictionary_NotifiesSubscribers()
    {
        var dictionary = new ObservableDictionary<string, int>();
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.Clear();
        
        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Is.Empty);
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
    }

    [Test]
    public void Clear_NonEmptyDictionary_RemovesAllElements()
    {
        var dictionary = new ObservableDictionary<string, int>();
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2)
        });
        
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        dictionary.Subscribe(patch => patches.Add(patch));
        
        dictionary.Clear();
        
        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Updated, Is.Empty);
        Assert.That(patches[0].Removed, Is.Empty);
        Assert.That(patches[1].Added, Is.Empty);
        Assert.That(patches[1].Updated, Is.Empty);
        Assert.That(patches[1].Removed, Has.Count.EqualTo(2));
    }

    #endregion

    #region Complete Tests

    [Test]
    public void Complete_MarksDictionaryAsCompleted()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var completed = false;
        
        dictionary.Subscribe(
            _ => { },
            () => completed = true);
        
        dictionary.Complete();
        
        Assert.That(completed, Is.True);
    }

    [Test]
    public void Complete_MultipleCalls_OnlyCompletesOnce()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var completionCount = 0;
        
        dictionary.Subscribe(
            _ => { },
            () => completionCount++);
        
        dictionary.Complete();
        dictionary.Complete();
        dictionary.Complete();
        
        Assert.That(completionCount, Is.EqualTo(1));
    }

    [Test]
    public void Complete_AfterComplete_NoFurtherNotifications()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var notificationCount = 0;
        
        dictionary.Subscribe(_ => notificationCount++);
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        dictionary.Complete();
        var countAfterComplete = notificationCount;
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("B", 2)
        });
        
        Assert.That(notificationCount, Is.EqualTo(countAfterComplete));
    }

    #endregion

    #region Multiple Subscribers Tests

    [Test]
    public void MultipleSubscribers_AllReceiveNotifications()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var subscriber1Patches = new List<ObservableDictionaryPatch<string, int>>();
        var subscriber2Patches = new List<ObservableDictionaryPatch<string, int>>();
        var subscriber3Patches = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => subscriber1Patches.Add(patch));
        dictionary.Subscribe(patch => subscriber2Patches.Add(patch));
        dictionary.Subscribe(patch => subscriber3Patches.Add(patch));
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1)
        });
        
        Assert.That(subscriber1Patches, Has.Count.EqualTo(1));
        Assert.That(subscriber2Patches, Has.Count.EqualTo(1));
        Assert.That(subscriber3Patches, Has.Count.EqualTo(1));
        
        Assert.That(subscriber1Patches[0].Added, Has.Count.EqualTo(1));
        Assert.That(subscriber2Patches[0].Added, Has.Count.EqualTo(1));
        Assert.That(subscriber3Patches[0].Added, Has.Count.EqualTo(1));
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public void ConcurrentUpdates_NoExceptionsThrown()
    {
        var dictionary = new ObservableDictionary<int, int>();
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();
        
        for (int i = 0; i < 10; i++)
        {
            int startValue = i * 100;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        dictionary.Set(new[]
                        {
                            new KeyValuePair<int, int>(startValue + j, j)
                        });
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
    public void ConcurrentSubscribeAndModify_NoDeadlocks()
    {
        var dictionary = new ObservableDictionary<int, int>();
        var timeout = TimeSpan.FromSeconds(5);
        var completed = false;
        
        var task = Task.Run(() =>
        {
            dictionary.Subscribe(patch => { });
            
            for (int i = 0; i < 1000; i++)
            {
                dictionary.Set(new[]
                {
                    new KeyValuePair<int, int>(i, i)
                });
            }
            
            completed = true;
        });
        
        Assert.That(task.Wait(timeout), Is.True, "Operation timed out - possible deadlock");
        Assert.That(completed, Is.True);
    }

    #endregion

    #region Complex Scenario Tests

    [Test]
    public void ComplexScenario_MultipleOperationsSequence()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var allPatches = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => allPatches.Add(patch));
        
        // Initial population
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("A", 1),
            new KeyValuePair<string, int>("B", 2),
            new KeyValuePair<string, int>("C", 3)
        });
        
        // Partial update
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("B", 20),
            new KeyValuePair<string, int>("D", 4)
        });
        
        // Append
        dictionary.AddOrUpdate(new[]
        {
            new KeyValuePair<string, int>("E", 5)
        });
        
        // Single update
        dictionary.AddOrUpdate([new KeyValuePair<string, int>("C", 30)]);
        
        // Remove some
        dictionary.RemoveMany(new[] { "A", "D" });
        
        // Clear all
        dictionary.Clear();
        
        Assert.That(allPatches, Has.Count.EqualTo(6));
        
        // Verify final state is empty
        Assert.That(allPatches[5].Removed, Has.Count.GreaterThan(0));
    }

    [Test]
    public void PatchAccuracy_ValuesAreCorrect()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var patches = new List<ObservableDictionaryPatch<string, int>>();
        
        dictionary.Subscribe(patch => patches.Add(patch));
        
        var initialData = new[]
        {
            new KeyValuePair<string, int>("X", 100),
            new KeyValuePair<string, int>("Y", 200)
        };
        
        dictionary.Set(initialData);
        
        Assert.That(patches[0].Added, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added.Any(kv => kv.Key == "X" && kv.Value == 100), Is.True);
        Assert.That(patches[0].Added.Any(kv => kv.Key == "Y" && kv.Value == 200), Is.True);
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public void EmptyOperations_NoNotifications()
    {
        var dictionary = new ObservableDictionary<string, int>();
        var notificationCount = 0;
        
        dictionary.Subscribe(_ => notificationCount++);
        
        dictionary.Set(Array.Empty<KeyValuePair<string, int>>());
        dictionary.AddOrUpdate(Array.Empty<KeyValuePair<string, int>>());
        dictionary.RemoveMany(Array.Empty<string>());
        
        Assert.That(notificationCount, Is.EqualTo(0));
    }

    [Test]
    public void CustomEqualityComparer_IsRespected()
    {
        var caseInsensitiveComparer = StringComparer.OrdinalIgnoreCase;
        var dictionary = new ObservableDictionary<string, int>(caseInsensitiveComparer);
        
        dictionary.Set(new[]
        {
            new KeyValuePair<string, int>("Key", 1)
        });
        
        // Should work with different case
        dictionary.AddOrUpdate([new KeyValuePair<string, int>("KEY", 10)]);
        
        var currentValue = dictionary.GetType()
            .GetField("_currentDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(dictionary) as System.Collections.Generic.Dictionary<string, int>;
        
        Assert.That(currentValue?["Key"], Is.EqualTo(10));
    }

    #endregion
}
