using System;
using System.Threading.Tasks;

namespace Actuarius.Concurrent.Tests
{
    [TestFixture]
    public class ObservableFieldTests_Copilot
    {
        [Test]
        public void Value_ReturnsInitialValue()
        {
            var field = new ObservableField<int>(42);
            Assert.That(field.Value, Is.EqualTo(42));
        }

        [Test]
        public void SetValue_UpdatesValueAndNotifiesObservers()
        {
            var field = new ObservableField<string>("Initial");
            string? observedValue = null;

            field.Subscribe(value => observedValue = value);
            field.SetValue("Updated");

            Assert.That(field.Value, Is.EqualTo("Updated"));
            Assert.That(observedValue, Is.EqualTo("Updated"));
        }

        [Test]
        public void SetValue_DoesNotNotifyIfValueUnchanged()
        {
            var field = new ObservableField<int>(10);
            int notificationCount = 0;

            field.Subscribe(_ => notificationCount++);
            field.SetValue(10);

            Assert.That(notificationCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WaitFor_CompletesWhenPredicateIsSatisfied()
        {
            var field = new ObservableField<int>(0);
            var task = field.WaitFor(value => value > 5);

            field.SetValue(10);

            var result = await task;
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task WaitFor_ReturnsFalseIfCompleted()
        {
            var field = new ObservableField<int>(0);
            var task = field.WaitFor(value => value > 5);

            field.Complete();

            var result = await task;
            Assert.That(result, Is.False);
        }

        [Test]
        public void Complete_PreventsFurtherUpdates()
        {
            var field = new ObservableField<int>(0);
            field.Complete();

            field.SetValue(10);

            Assert.That(field.Value, Is.EqualTo(0));
        }

        [Test]
        public void Subscribe_ReceivesCompletionNotification()
        {
            var field = new ObservableField<int>(0);
            bool completed = false;

            field.Subscribe(
                _ => { },
                () => completed = true
            );

            field.Complete();

            Assert.That(completed, Is.True);
        }

        [Test]
        public void ToString_ReturnsStringRepresentationOfValue()
        {
            var field = new ObservableField<int>(123);
            Assert.That(field.ToString(), Is.EqualTo("123"));
        }

        [Test]
        public void ImplicitConversion_ReturnsValue()
        {
            var field = new ObservableField<int>(99);
            int value = field;

            Assert.That(value, Is.EqualTo(99));
        }
    }
}
