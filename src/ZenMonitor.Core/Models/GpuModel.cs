// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

/// <summary>
/// A snapshot of all GPU metrics collected at a single point in time.
/// </summary>
/// <param name="GpuName">GPU model name.</param>
/// <param name="UsageGpu">GPU core utilization percentage (0-100).</param>
/// <param name="UsageMemory">GPU memory utilization percentage (0-100).</param>
/// <param name="MemoryUsed">GPU memory used in megabytes.</param>
/// <param name="MemoryTotal">Total GPU memory in megabytes.</param>
/// <param name="TemperatureGpu">GPU temperature in degrees Celsius.</param>
/// <param name="PowerState">GPU power state (e.g. P0, P2, P8).</param>
/// <param name="PowerDraw">Current GPU power draw in watts.</param>
public record GpuInfoSnapshot(
    string GpuName,
    int UsageGpu,
    int UsageMemory,
    double MemoryUsed,
    double MemoryTotal,
    int TemperatureGpu,
    string PowerState,
    double PowerDraw
);
