// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models.Telemetry;

/// <summary>
/// Represents the measured clock speed of a single CPU core.
/// </summary>
/// <param name="Index">Zero-based core index.</param>
/// <param name="Speed">Core frequency in MHz.</param>
public record CpuCoreSpeed(int Index, double Speed);

/// <summary>
/// Represents the CPU usage percentage of a single core.
/// </summary>
/// <param name="Index">Zero-based core index.</param>
/// <param name="Usage">Core usage percentage (0-100).</param>
public record CpuCoreUsage(int Index, double Usage);

/// <summary>
/// Represents the temperature reading of a single CPU core.
/// </summary>
/// <param name="Index">Zero-based core index (or CCD index on AMD).</param>
/// <param name="Temp">Core temperature in degrees Celsius.</param>
public record CpuCoreTemp(int Index, int Temp);

/// <summary>
/// A snapshot of all CPU metrics collected at a single point in time.
/// </summary>
/// <param name="CpuName">CPU model name string.</param>
/// <param name="CpuSpeed">Overall CPU frequency in MHz.</param>
/// <param name="CpuUsage">Overall CPU usage percentage.</param>
/// <param name="CpuTemp">Overall CPU temperature in degrees Celsius.</param>
/// <param name="PowerDraw">Current CPU package power draw in watts.</param>
/// <param name="CoreSpeeds">Per-core frequency measurements.</param>
/// <param name="CoreUsages">Per-core usage percentages.</param>
/// <param name="CoreTemps">Per-core or per-CCD temperature readings.</param>
public record CpuInfoSnapshot(
    string CpuName,
    double CpuSpeed,
    int CpuUsage,
    int CpuTemp,
    double PowerDraw,
    CpuCoreSpeed[] CoreSpeeds,
    CpuCoreUsage[] CoreUsages,
    CpuCoreTemp[] CoreTemps
);
