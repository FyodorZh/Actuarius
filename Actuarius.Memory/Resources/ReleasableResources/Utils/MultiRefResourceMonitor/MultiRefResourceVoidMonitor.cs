using System;
using System.Collections.Generic;

namespace Actuarius.Memory
{
    public class MultiRefResourceVoidMonitor : IMultiRefResourceMonitor
    {
#pragma warning disable CS0067
        public event Action<string, ResourceUsageErrorType>? Error;
#pragma warning restore CS0067
        
        public static readonly MultiRefResourceVoidMonitor Instance = new MultiRefResourceVoidMonitor();
        
        public IReadOnlyList<ResourceUsageRecord> ExportHistory()
        {
            return Array.Empty<ResourceUsageRecord>();
        }
    }
}