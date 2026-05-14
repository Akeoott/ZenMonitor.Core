// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IGpu"/> for AMD GPUs.
/// Currently not implemented — returns default zeros.
/// </summary>
[SupportedOSPlatform("linux")]
public class GpuAmd(ILogger<GpuAmd> logger) : IGpu
{
    private readonly ILogger<GpuAmd> _logger = logger;
    private GpuInfoSnapshot _snapshot = new(
        "", 0, 0, 0.0, 0.0, 0, "", 0.0);

    /// <summary>Updates all cached GPU metrics.</summary>
    public void Update() => _snapshot = FetchGpuInfo();

    /// <summary>Returns the GPU model name.</summary>
    public string GetGpuName() => _snapshot.GpuName;

    /// <summary>Returns the GPU core utilization percentage (0-100).</summary>
    public int GetUsageGpu() => _snapshot.UsageGpu;

    /// <summary>Returns the GPU memory utilization percentage (0-100).</summary>
    public int GetUsageMemory() => _snapshot.UsageMemory;

    /// <summary>Returns the GPU memory used in megabytes.</summary>
    public double GetMemoryUsed() => _snapshot.MemoryUsed;

    /// <summary>Returns the total GPU memory in megabytes.</summary>
    public double GetMemoryTotal() => _snapshot.MemoryTotal;

    /// <summary>Returns the GPU temperature in degrees Celsius.</summary>
    public int GetTemperatureGpu() => _snapshot.TemperatureGpu;

    /// <summary>Returns the current GPU power state.</summary>
    public string GetPowerState() => _snapshot.PowerState;

    /// <summary>Returns the current GPU power draw in watts.</summary>
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
