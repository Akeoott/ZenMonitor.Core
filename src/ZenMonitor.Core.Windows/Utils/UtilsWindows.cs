// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Management;

using Microsoft.Win32;

using ZenMonitor.Core.Models;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Windows.Utils;

/// <summary>
/// Windows implementation of <see cref="IUtilsWindows"/> using native Win32 API calls
/// (P/Invoke) and WMI for hardware metrics.
/// </summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("windows")]
public partial class UtilsWindows(UtilsWindows.RawCpuTelemetry rawCpu) : IUtilsWindows
{
    /// <inheritdoc />
    public IRawCpuTelemetry RawCpu { get; } = rawCpu;

    #region RawCpuTelemetry
    /// <summary>
    /// Implementation of <see cref="IRawCpuTelemetry"/> that gets CPU metrics
    /// </summary>
    public class RawCpuTelemetry : IRawCpuTelemetry
    {
        /// <inheritdoc />
        public string GetProcessorName()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return (string?)key?.GetValue("ProcessorNameString") ?? "Unknown CPU";
            }
            catch
            {
                return "Unknown CPU";
            }
        }

        /// <inheritdoc />
        public int GetProcessorCount()
        {
            GetSystemInfo(out var info);
            return (int)info.dwNumberOfProcessors;
        }

        /// <inheritdoc />
        public int GetCpuFrequencyMHz()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var value = key?.GetValue("~MHz");
                return value is int mhz ? mhz : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <inheritdoc />
        public CpuTickInfo GetSystemTimes()
        {
            var result = UtilsWindows.GetSystemTimes(out var idle, out var kernel, out var user);
            return result == 0
                ? new CpuTickInfo(0, 0, 0)
                : new CpuTickInfo(idle, kernel, user);
        }

        /// <inheritdoc />
        public CpuTickInfo[] GetPerCoreTimes()
        {
            var coreCount = GetProcessorCount();
            var structSize = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
            var bufferSize = structSize * coreCount;

            var buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                var ret = NtQuerySystemInformation(
                    SystemProcessorPerformanceInformation,
                    buffer,
                    bufferSize,
                    out _);

                if (ret != 0)
                    return [.. Enumerable.Repeat(new CpuTickInfo(0, 0, 0), coreCount)];

                var results = new CpuTickInfo[coreCount];
                for (var i = 0; i < coreCount; i++)
                {
                    var ptr = buffer + (i * structSize);
                    var perf = Marshal.PtrToStructure<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(ptr);
                    results[i] = new CpuTickInfo(perf.IdleTime, perf.KernelTime, perf.UserTime);
                }

                return results;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <inheritdoc />
        public int GetTemperature()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\WMI",
                    "SELECT * FROM MSAcpi_ThermalZoneTemperature");

                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    var tempKelvin = (uint)(obj["CurrentTemperature"] ?? 0);
                    if (tempKelvin > 0)
                        return (int)((tempKelvin - 2732) / 10.0);
                }
            }
            catch
            {
                // WMI may not be available or thermal zone may not exist
            }

            return 0;
        }

        /// <inheritdoc />
        public double GetPowerDraw()
        {
            // TODO: Add Power draw fetching
            return 0.0;
        }
    }
    #endregion
}
