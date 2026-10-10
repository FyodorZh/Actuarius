namespace Actuarius.Memory.Tests
{
    internal sealed class TestResource : MultiRefResource
    {
        public TestResource(bool revivable = false)
            : base(revivable)
        {
        }

        public int ReleasedCount { get; private set; }

        protected override void OnReleased()
        {
            ReleasedCount++;
        }

        public bool ReviveEx() => Revive();

        public MultiRefResourceMonitor? MonitorEx => Monitor;
    }
}
