// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IGpu"/> for AMD GPUs.
/// Currently not implemented — returns default zeros.
/// </summary>
[SupportedOSPlatform("linux")]
public class GpuAmd(ILogger<GpuAmd>? logger) : IGpu
{
    private readonly ILogger<GpuAmd> _logger = logger ?? NullLogger<GpuAmd>.Instance;
    private GpuInfoSnapshot _snapshot = new("", 0, 0, 0.0, 0.0, 0, "", 0.0);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchGpuInfo();

    /// <inheritdoc />
    public string GetGpuName() => _snapshot.GpuName;

    /// <inheritdoc />
    public int GetUsageGpu() => _snapshot.UsageGpu;

    /// <inheritdoc />
    public int GetUsageMemory() => _snapshot.UsageMemory;

    /// <inheritdoc />
    public double GetMemoryUsed() => _snapshot.MemoryUsed;

    /// <inheritdoc />
    public double GetMemoryTotal() => _snapshot.MemoryTotal;

    /// <inheritdoc />
    public int GetTemperatureGpu() => _snapshot.TemperatureGpu;

    /// <inheritdoc />
    public string GetPowerState() => _snapshot.PowerState;

    /// <inheritdoc />
    public double GetPowerDraw() => _snapshot.PowerDraw;

    /// <summary>
    /// AMD GPU implementation is pending. Currently returns a zeroed snapshot.
    /// TODO: Implement reading metrics from /sys/class/drm/card*/device/hwmon.
    /// </summary>
    private GpuInfoSnapshot FetchGpuInfo()
    {
        _logger.LogTrace("Fetching all GpuAmd info...");
        _logger.LogWarning("AMD GPUs are currently not supported!");

        return new GpuInfoSnapshot(
            "", 0, 0, 0.0, 0.0, 0, "", 0.0);
    }
}
