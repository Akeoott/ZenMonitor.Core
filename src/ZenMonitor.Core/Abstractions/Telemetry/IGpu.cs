// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides GPU hardware monitoring capabilities, including temperature,
/// utilization, memory usage, and power draw.
/// </summary>
public interface IGpu
{
    /// <summary>Updates all cached GPU metrics.</summary>
    void Update();

    /// <summary>Returns the GPU model name.</summary>
    string GetGpuName();

    /// <summary>Returns the GPU core utilization percentage (0-100).</summary>
    int GetUsageGpu();

    /// <summary>Returns the GPU memory utilization percentage (0-100).</summary>
    int GetUsageMemory();

    /// <summary>Returns the GPU memory used in megabytes.</summary>
    double GetMemoryUsed();

    /// <summary>Returns the total GPU memory in megabytes.</summary>
    double GetMemoryTotal();

    /// <summary>Returns the GPU temperature in degrees Celsius.</summary>
    int GetTemperatureGpu();

    /// <summary>Returns the current GPU power state (e.g. P0, P2, P8).</summary>
    string GetPowerState();

    /// <summary>Returns the current GPU power draw in watts.</summary>
    double GetPowerDraw();
}
