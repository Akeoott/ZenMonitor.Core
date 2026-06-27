// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides CPU hardware monitoring capabilities, including temperature,
/// usage, frequency, and power draw metrics per-core and overall.
/// </summary>
public interface ICpu
{
    /// <summary>Updates all cached CPU metrics by reading from system files.</summary>
    void Update();

    /// <summary>
    /// Get the entire snapshot record of <see cref="ICpu"/>
    /// </summary>
    /// <returns><see cref="CpuInfoSnapshot"/> and all its underlying data</returns>
    CpuInfoSnapshot GetSnapshot();

    /// <summary>Returns the CPU model name (e.g. "Intel Core i7-13700K").</summary>
    string GetCpuName();

    /// <summary>Returns the overall CPU frequency in MHz.</summary>
    double GetCpuSpeed();

    /// <summary>Returns the overall CPU usage percentage (0-100).</summary>
    int GetCpuUsage();

    /// <summary>Returns the overall CPU temperature in degrees Celsius.</summary>
    int GetCpuTemp();

    /// <summary>Returns the current CPU package power draw in watts.</summary>
    double GetPowerDraw();

    /// <summary>Returns per-core frequency measurements.</summary>
    CpuCoreSpeed[] GetCoreSpeeds();

    /// <summary>Returns per-core usage percentages (0-100).</summary>
    CpuCoreUsage[] GetCoreUsages();

    /// <summary>Returns per-core temperature readings in degrees Celsius.</summary>
    CpuCoreTemp[] GetCoreTemps();
}
