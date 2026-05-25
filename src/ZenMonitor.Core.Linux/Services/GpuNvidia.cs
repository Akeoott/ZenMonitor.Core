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
public class GpuNvidia(ILogger<GpuNvidia> logger, IServiceAbstraction helper) : IGpu
{
    private readonly ILogger<GpuNvidia> _logger = logger;
    private readonly IServiceAbstraction _helper = helper;
    private GpuInfoSnapshot _snapshot = new(
        "", 0, 0, 0.0, 0.0, 0, "", 0.0);

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

    private GpuInfoSnapshot FetchGpuInfo()
    {
        _logger.LogTrace("Fetching all GpuNvidia info...");

        string csv = "";
        try
        {
            csv = RunNvidiaSmi(
           "--query-gpu=name,utilization.gpu,utilization.memory,memory.used,memory.total,temperature.gpu,pstate,power.draw" +
           "--format=csv,noheader,nounits");

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
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "{exceptionMessage}", ex.Message);
            return new("", 0, 0, 0.0, 0.0, 0, "", 0.0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FetchGpuInfo failed unexpectedly.");
            return new("", 0, 0, 0.0, 0.0, 0, "", 0.0);
        }
    }

    private string RunNvidiaSmi(string arguments)
    {
        ProcessResult result = _helper.Linux.RunProcess("nvidia-smi", arguments);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"nvidia-smi error code {result.ExitCode}: {result.StandardError}");
        }

        return result.StandardOutput.Trim();
    }
}
