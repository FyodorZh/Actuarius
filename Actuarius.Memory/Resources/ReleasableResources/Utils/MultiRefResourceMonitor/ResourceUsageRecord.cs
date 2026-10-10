using System.Diagnostics;

namespace Actuarius.Memory
{
    /// <summary>
    /// A single recorded reference-count event.
    /// </summary>
    public readonly struct ResourceUsageRecord
    {
        /// <summary>Gets the inferred operation name.</summary>
        public readonly string Action;

        /// <summary>Gets the reference count observed for the event.</summary>
        public readonly int Count;

        /// <summary>Gets a value indicating whether the event was a failure.</summary>
        public readonly bool IsError;

        /// <summary>Gets the captured stack, or null when stack tracing is disabled.</summary>
        public readonly StackTrace? Stack;

        /// <summary>
        /// Initializes a new history record.
        /// </summary>
        public ResourceUsageRecord(string action, int count, bool isError, StackTrace? stack)
        {
            Action = action;
            Count = count;
            IsError = isError;
            Stack = stack;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return (IsError ? "! " : "  ") + Action + " -> " + Count;
        }
    }
}