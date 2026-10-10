namespace Actuarius.Memory
{
    /// <summary>
    /// Describes why a <see cref="MultiRefResource"/> was used incorrectly.
    /// </summary>
    public enum ResourceUsageErrorType
    {
        /// <summary>An owner was added to an already released resource.</summary>
        AddRefOfReleasedObject,

        /// <summary>A resource with no remaining owners was released again.</summary>
        ReleaseOfReleasedObject,

        /// <summary>A non-revivable resource was revived, or the count was not exactly zero.</summary>
        WrongReviveUsage,

        /// <summary>A resource was garbage collected while it still had owners.</summary>
        Leak,

        /// <summary>The resource was asserted to be idle while it still had owners.</summary>
        NoUsageAssertionFail,

        /// <summary>An unrecognized failure reported by the reference counter.</summary>
        Unknown,
    }
}