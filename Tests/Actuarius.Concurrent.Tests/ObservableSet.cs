using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actuarius.Concurrent.Tests;

[TestFixture]
public class ObservableSetTests
{
    [Test]
    public void CheckForInit()
    {
        var observableSet = new ObservableSet<string>();

        observableSet.Set(new[] { "X", "Y" });
        
        List<ObservableSetPatch<string>> patches = new();
        observableSet.Subscribe(patch => patches.Add(patch));

        observableSet.Set(new[] { "X", "Z" });

        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].Added, Is.EquivalentTo(new[] { "X", "Y" }));
        Assert.That(patches[0].Removed, Is.EquivalentTo(Array.Empty<string>()));
        Assert.That(patches[1].Added, Is.EquivalentTo(new[] { "Z" }));
        Assert.That(patches[1].Removed, Is.EquivalentTo(new[] { "Y" }));
    }
    
    [Test]
    public void Update_AddsNewElements()
    {
        var observableSet = new ObservableSet<int>();
        var patchReceived = false;
        ObservableSetPatch<int>? lastPatch = null;

        observableSet.Subscribe(patch =>
        {
            patchReceived = true;
            lastPatch = patch;
        });

        observableSet.Set(new[] { 1, 2, 3 });

        Assert.That(patchReceived, Is.True);
        Assert.That(lastPatch!.Value.Added, Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(lastPatch.Value.Removed, Is.Empty);
    }

    [Test]
    public void Update_RemovesMissingElements()
    {
        var observableSet = new ObservableSet<string>();
        var patches = new List<ObservableSetPatch<string>>();

        observableSet.Subscribe(patch => patches.Add(patch));

        observableSet.Set(new[] { "A", "B", "C" });
        patches.Clear();

        observableSet.Set(new[] { "B" });

        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Is.Empty);
        Assert.That(patches[0].Removed, Is.EquivalentTo(new[] { "A", "C" }));
    }

    [Test]
    public void Update_PreservesExistingElements()
    {
        var observableSet = new ObservableSet<int>();
        var patches = new List<ObservableSetPatch<int>>();

        observableSet.Subscribe(patch => patches.Add(patch));

        observableSet.Set(new[] { 1, 2, 3 });
        patches.Clear();

        observableSet.Set(new[] { 2, 3, 4 });

        Assert.That(patches, Has.Count.EqualTo(1));
        Assert.That(patches[0].Added, Is.EquivalentTo(new[] { 4 }));
        Assert.That(patches[0].Removed, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Update_NoChanges_DoesNotNotify()
    {
        var observableSet = new ObservableSet<string>();
        var notificationCount = 0;

        observableSet.Subscribe(_ => notificationCount++);

        observableSet.Set(new[] { "A", "B" });
        var initialCount = notificationCount;

        observableSet.Set(new[] { "A", "B" });

        Assert.That(notificationCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void Update_EmptyInput_ClearsSet()
    {
        var observableSet = new ObservableSet<int>();
        var patchReceived = false;
        ObservableSetPatch<int>? lastPatch = null;

        observableSet.Subscribe(patch =>
        {
            patchReceived = true;
            lastPatch = patch;
        });

        observableSet.Set(new[] { 1, 2, 3 });
        patchReceived = false;

        observableSet.Set(Array.Empty<int>());

        Assert.That(patchReceived, Is.True);
        Assert.That(lastPatch!.Value.Removed, Has.Count.EqualTo(3));
    }

    [Test]
    public void PatchStream_NotifiesSubscribersOfChanges()
    {
        var observableSet = new ObservableSet<int>();
        var addedList = new List<IReadOnlyList<int>>();
        var removedList = new List<IReadOnlyList<int>>();

        observableSet.Subscribe(patch =>
        {
            addedList.Add(patch.Added.ToArray());
            removedList.Add(patch.Removed.ToArray());
        });

        observableSet.Set(new[] { 1, 2 });
        observableSet.Set(new[] { 2, 3 });
        observableSet.Set(new[] { 3 });

        Assert.That(addedList, Has.Count.EqualTo(3));
        Assert.That(removedList, Has.Count.EqualTo(3));

        Assert.That(addedList[0], Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(addedList[1], Is.EquivalentTo(new[] { 3 }));
        Assert.That(addedList[2], Is.Empty);

        Assert.That(removedList[0], Is.Empty);
        Assert.That(removedList[1], Is.EquivalentTo(new[] { 1 }));
        Assert.That(removedList[2], Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void Update_WithDuplicateElements_ThrowsException()
    {
        var observableSet = new ObservableSet<string>();

        observableSet.Set(new[] { "A" });

        Assert.Throws<InvalidOperationException>(() =>
        {
            observableSet.Set(new[] { "A", "A" });
        });
    }

    [Test]
    public void Update_MultipleSubscribers_AllReceiveNotifications()
    {
        var observableSet = new ObservableSet<int>();
        var subscriber1Patches = new List<ObservableSetPatch<int>>();
        var subscriber2Patches = new List<ObservableSetPatch<int>>();

        observableSet.Subscribe(patch => subscriber1Patches.Add(patch));
        observableSet.Subscribe(patch => subscriber2Patches.Add(patch));

        observableSet.Set(new[] { 1, 2, 3 });

        Assert.That(subscriber1Patches, Has.Count.EqualTo(1));
        Assert.That(subscriber2Patches, Has.Count.EqualTo(1));
        Assert.That(subscriber1Patches[0].Added, Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(subscriber2Patches[0].Added, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Update_ThreadSafety_ConcurrentUpdates()
    {
        var observableSet = new ObservableSet<int>();
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
                        observableSet.Set(new[] { startValue + j });
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
    public void ObservableSetPatch_StructProperties()
    {
        var added = new[] { 1, 2, 3 };
        var removed = new[] { 4, 5 };

        var patch = new ObservableSetPatch<int>(added, removed);

        Assert.That(patch.Added, Is.EquivalentTo(added));
        Assert.That(patch.Removed, Is.EquivalentTo(removed));
    }

    [Test]
    public void Update_ComplexScenario_AddRemoveAndPreserve()
    {
        var observableSet = new ObservableSet<string>();
        var allPatches = new List<ObservableSetPatch<string>>();

        observableSet.Subscribe(patch => allPatches.Add(patch));

        // Initial add
        observableSet.Set(new[] { "A", "B", "C", "D" });

        // Remove some, add new ones, preserve others
        observableSet.Set(new[] { "B", "C", "E", "F" });

        Assert.That(allPatches, Has.Count.EqualTo(2));
        Assert.That(allPatches[1].Removed, Is.EquivalentTo(new[] { "A", "D" }));
        Assert.That(allPatches[1].Added, Is.EquivalentTo(new[] { "E", "F" }));

        // Complete replacement
        observableSet.Set(new[] { "X", "Y" });

        Assert.That(allPatches, Has.Count.EqualTo(3));
        Assert.That(allPatches[2].Removed, Has.Count.EqualTo(4));
        Assert.That(allPatches[2].Added, Is.EquivalentTo(new[] { "X", "Y" }));
    }

    [Test]
    public void Update_NullEnumerable_ThrowsArgumentNullException()
    {
        var observableSet = new ObservableSet<int>();

        Assert.Throws<ArgumentNullException>(() =>
        {
            observableSet.Set(null!);
        });
    }

    [Test]
    public void Update_EmptySetWithEmptyInput_DoesNotNotify()
    {
        var observableSet = new ObservableSet<int>();
        var notificationCount = 0;

        observableSet.Subscribe(_ => notificationCount++);

        observableSet.Set(Array.Empty<int>());

        Assert.That(notificationCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_AddAndRemoveSameElementInSingleUpdate()
    {
        var observableSet = new ObservableSet<string>();
        var patches = new List<ObservableSetPatch<string>>();

        observableSet.Subscribe(patch => patches.Add(patch));

        observableSet.Set(new[] { "A" });
        observableSet.Set(new[] { "A", "B" });
        observableSet.Set(new[] { "B" });

        Assert.That(patches, Has.Count.EqualTo(3));
        Assert.That(patches[2].Added, Is.Empty);
        Assert.That(patches[2].Removed, Is.EquivalentTo(new[] { "A" }));
    }
}
