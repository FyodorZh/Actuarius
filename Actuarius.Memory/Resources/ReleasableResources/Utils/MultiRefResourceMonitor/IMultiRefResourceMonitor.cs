using System;
using System.Collections.Generic;

namespace Actuarius.Memory
{
    public interface IMultiRefResourceMonitor
    {
        event Action<string, ResourceUsageErrorType>? Error;
        IReadOnlyList<ResourceUsageRecord> ExportHistory();
    }
}