// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IGpu"/> for NVIDIA GPUs.
/// Reads metrics via the <c>nvidia-smi</c> CLI tool.
/// </summary>
[SupportedOSPlatform("linux")]
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
        _logger.LogTrace("Fetching all GpuNvidia info...");

        string csv = RunNvidiaSmi(
            "--query-gpu=name,utilization.gpu,utilization.memory,memory.used,memory.total,temperature.gpu,pstate,power.draw --format=csv,noheader,nounits");

        string[] part = [.. csv.Split(',').Select(p => p.Trim())];

        return new GpuInfoSnapshot(
            part[0],
            int.TryParse(part[1], out var usageGpu) ? usageGpu : 0,
            int.TryParse(part[2], out var usageMemory) ? usageMemory : 0,
            double.TryParse(part[3], out var memoryUsed) ? memoryUsed : 0.0,
            double.TryParse(part[4], out var memoryTotal) ? memoryTotal : 0.0,
            int.TryParse(part[5], out var temperatureGpu) ? temperatureGpu : 0,
            part[6],
            double.TryParse(part[7], out var powerDraw) ? powerDraw : 0.0);
    }

    private string RunNvidiaSmi(string arguments)
    {
        ProcessResult result = _helper.Linux.RunProcess("nvidia-smi", arguments);

        if (result.ExitCode != 0)
        {
            _logger.LogError("Running nvidia-smi failed with exit code {ExitCode}: {Error}", result.ExitCode, result.StandardError);
            throw new InvalidOperationException($"nvidia-smi error: {result.StandardError}");
        }

        return result.StandardOutput.Trim();
    }
}
