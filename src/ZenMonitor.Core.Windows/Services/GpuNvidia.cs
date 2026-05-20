// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="IGpu"/> for NVIDIA GPUs.
/// Reads metrics via the <c>nvidia-smi</c> CLI tool.
/// </summary>
[SupportedOSPlatform("windows")]
public class GpuNvidia(ILogger<GpuNvidia> logger, IHelper helper) : IGpu
{
    private readonly ILogger<GpuNvidia> _logger = logger;
    private readonly IHelper _helper = helper;
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

    private GpuInfoSnapshot FetchGpuInfo()
    {
        try
        {
            _logger.LogTrace("Fetching all Gpu info...");
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Gpu info");
            return new GpuInfoSnapshot("", 0, 0, 0, 0, 0, "", 0);
        }
    }
}
