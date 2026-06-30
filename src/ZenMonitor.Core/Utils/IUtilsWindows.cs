// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Utils;

/// <summary>
/// Abstractions for Windows-native system operations via Win32 API that need
/// to be mockable in tests.
/// </summary>
[SupportedOSPlatform("windows")]
public interface IUtilsWindows
{
    /// <summary>Gets the CPU telemetry service.</summary>
    IRawCpuTelemetry RawCpu { get; }
}

/// <summary>
/// Provides raw CPU telemetry values for the windows implementation of <see cref="ICpu"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public interface IRawCpuTelemetry
{
    /// <summary>Returns the CPU processor model name.</summary>
    string GetProcessorName();

    /// <summary>Returns the number of logical processors.</summary>
    int GetProcessorCount();

    /// <summary>Returns the base processor frequency in MHz.</summary>
    int GetBaseFrequencyMHz();

    /// <summary>
    /// Returns the current total system idle/kernel/user tick counts.
    /// Kernel includes idle — subtract idle to get actual kernel time.
    /// </summary>
    CpuTickInfo GetSystemTimes();

    /// <summary>
    /// Returns per-core idle/kernel/user tick counts for each logical processor.
    /// Kernel includes idle — subtract idle to get actual kernel time.
    /// </summary>
    CpuTickInfo[] GetPerCoreTimes();

    /// <summary>Returns the current CPU temperature in degrees Celsius.</summary>
    int GetTemperature();

    /// <summary>Returns the current CPU package power draw in watts.</summary>
    double GetPowerDraw();
}
