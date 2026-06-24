// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions.Telemetry;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IGpu"/> implementation that returns all-zero / empty defaults.
/// Used as a fallback when the GPU vendor is unknown or detection fails.
/// </summary>
public sealed class NullGpu : IGpu
{
    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <summary>Returns an empty string.</summary>
    public string GetGpuName() => "";

    /// <summary>Returns 0.</summary>
    public int GetUsageGpu() => 0;

    /// <summary>Returns 0.</summary>
    public int GetUsageMemory() => 0;

    /// <summary>Returns 0.</summary>
    public double GetMemoryUsed() => 0;

    /// <summary>Returns 0.</summary>
    public double GetMemoryTotal() => 0;

    /// <summary>Returns 0.</summary>
    public int GetTemperatureGpu() => 0;

    /// <summary>Returns an empty string.</summary>
    public string GetPowerState() => "";

    /// <summary>Returns 0.</summary>
    public double GetPowerDraw() => 0;
}
