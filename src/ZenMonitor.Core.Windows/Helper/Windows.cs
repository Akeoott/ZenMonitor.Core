// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using Microsoft.Win32;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Helper;

/// <summary>
/// Windows implementation of <see cref="IWindows"/> using native Win32 API calls
/// (P/Invoke) and WMI for hardware metrics.
/// </summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("windows")]
public partial class Windows : IWindows
{
    // Win32 API Declarations

    [LibraryImport("kernel32.dll")]
    private static partial void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    [LibraryImport("kernel32.dll")]
    private static partial int GetSystemTimes(
        out long lpIdleTime,
        out long lpKernelTime,
        out long lpUserTime);

    [LibraryImport("ntdll.dll")]
    private static partial int NtQuerySystemInformation(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);

    [LibraryImport("powrprof.dll")]
    private static partial int CallNtPowerInformation(
        int InformationLevel,
        IntPtr lpInputBuffer,
        int nInputBufferSize,
        out SYSTEM_POWER_INFORMATION lpOutputBuffer,
        int nOutputBufferSize);

    // Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION
    {
        public long IdleTime;
        public long KernelTime;
        public long UserTime;
        public long Reserved1;
        public long Reserved2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_INFORMATION
    {
        public ulong MaxIdlenessAllowed;
        public ulong Idleness;
        public ulong TimeRemaining;
        public byte CoolingMode;
    }

    private const int SystemProcessorPerformanceInformation = 8;
    private const int SystemPowerInformation = 5;

    // IWindows Implementation

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
        GetSystemInfo(out SYSTEM_INFO info);
        return (int)info.dwNumberOfProcessors;
    }

    /// <inheritdoc />
    public int GetProcessorBaseFrequencyMHz()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            object? value = key?.GetValue("~MHz");
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
        long idle = 0, kernel = 0, user = 0;
        int result = GetSystemTimes(out idle, out kernel, out user);
        if (result == 0)
            return new CpuTickInfo(0, 0, 0);

        return new CpuTickInfo(idle, kernel, user);
    }

    /// <inheritdoc />
    public CpuTickInfo[] GetPerCoreTimes()
    {
        int coreCount = GetProcessorCount();
        int structSize = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
        int bufferSize = structSize * coreCount;

        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            int ret = NtQuerySystemInformation(
                SystemProcessorPerformanceInformation,
                buffer,
                bufferSize,
                out int retLen);

            if (ret != 0)
                return [.. Enumerable.Repeat(new CpuTickInfo(0, 0, 0), coreCount)];

            var results = new CpuTickInfo[coreCount];
            for (int i = 0; i < coreCount; i++)
            {
                IntPtr ptr = buffer + (i * structSize);
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
    public int GetCpuTemperature()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI",
                "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                uint tempKelvin = (uint)(obj["CurrentTemperature"] ?? 0);
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
    public double GetCpuPowerDraw()
    {
        try
        {
            int result = CallNtPowerInformation(
                SystemPowerInformation,
                IntPtr.Zero,
                0,
                out SYSTEM_POWER_INFORMATION spi,
                Marshal.SizeOf<SYSTEM_POWER_INFORMATION>());

            if (result != 0)
                return 0.0;

            // This is an approximation. The Idleness field represents overall
            // system idleness (0-100). Invert to get a rough "active" percentage,
            // but this is NOT actual wattage.
            // True power draw requires MSR access or external tools.
            // Returning 0 as a safer fallback.
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }
}
