// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IGpuTel"/> implementation that returns all-zero / empty defaults.
/// Used as a fallback when the GPU vendor is unknown or detection fails.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class NullGpuTel : IGpuTel
{
    private readonly GpuInfoSnapshot _emptySnapshot = new("", 0, 0, 0.0, 0.0, 0, "", 0.0);

    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <inheritdoc />
    public GpuInfoSnapshot GetSnapshot() => _emptySnapshot;

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
