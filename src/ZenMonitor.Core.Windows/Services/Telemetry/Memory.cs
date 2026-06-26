// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Windows.Services.Telemetry;

/// <summary>
/// Windows implementation of <see cref="IMemory"/>
/// </summary>
[SupportedOSPlatform("windows")]
public class Memory(ILogger<Memory> logger) : IMemory
{
    private MemoryInfoSnapshot _snapshot = new(0, 0, 0, 0, 0, 0, 0);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchMemoryInfo();

    /// <inheritdoc />
    public double GetMemTotal() => _snapshot.MemTotal;

    /// <inheritdoc />
    public double GetMemFree() => _snapshot.MemFree;

    /// <inheritdoc />
    public double GetMemAvailable() => _snapshot.MemAvailable;

    /// <inheritdoc />
    public double GetMemUsed() => _snapshot.MemUsed;

    /// <inheritdoc />
    public double GetCached() => _snapshot.Cached;

    /// <inheritdoc />
    public double GetSwapTotal() => _snapshot.SwapTotal;

    /// <inheritdoc />
    public double GetSwapFree() => _snapshot.SwapFree;

    private MemoryInfoSnapshot FetchMemoryInfo()
    {
        try
        {
            logger.LogTrace("Fetching all Memory info...");
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Memory info");
            return new MemoryInfoSnapshot(0, 0, 0, 0, 0, 0, 0);
        }
    }
}
