// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="IMemory"/> that reads memory metrics
/// from <c>/proc/meminfo</c>.
/// </summary>
[SupportedOSPlatform("windows")]
public class Memory(ILogger<Memory> logger, IFileSystem fileSystem) : IMemory
{
    private readonly ILogger<Memory> _logger = logger;
    private readonly IFileSystem _fileSystem = fileSystem;
    private MemoryInfoSnapshot _snapshot = new(0, 0, 0, 0, 0, 0, 0);

    /// <summary>Updates all cached memory metrics by reading from system files.</summary>
    public void Update() => _snapshot = FetchMemoryInfo();

    /// <summary>Returns total physical memory in GiB.</summary>
    public double GetMemTotal() => _snapshot.MemTotal;

    /// <summary>Returns free physical memory in GiB.</summary>
    public double GetMemFree() => _snapshot.MemFree;

    /// <summary>Returns available physical memory in GiB.</summary>
    public double GetMemAvailable() => _snapshot.MemAvailable;

    /// <summary>Returns used physical memory in GiB.</summary>
    public double GetMemUsed() => _snapshot.MemUsed;

    /// <summary>Returns cached memory in GiB.</summary>
    public double GetCached() => _snapshot.Cached;

    /// <summary>Returns total swap space in GiB.</summary>
    public double GetSwapTotal() => _snapshot.SwapTotal;

    /// <summary>Returns free swap space in GiB.</summary>
    public double GetSwapFree() => _snapshot.SwapFree;

    private MemoryInfoSnapshot FetchMemoryInfo()
    {
        try
        {
            _logger.LogTrace("Fetching all Memory info...");
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Memory info");
            return new MemoryInfoSnapshot(0, 0, 0, 0, 0, 0, 0);
        }
    }
}
