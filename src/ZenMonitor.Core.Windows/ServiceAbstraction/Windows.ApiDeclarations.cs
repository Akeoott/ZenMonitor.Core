// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ZenMonitor.Core.Windows.ServiceAbstraction;

[SupportedOSPlatform("windows")]
public partial class Windows
{
    #region Cpu
    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemInfo")]
    internal static partial void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemTimes")]
    internal static partial int GetSystemTimes(
        out long lpIdleTime,
        out long lpKernelTime,
        out long lpUserTime);

    [LibraryImport("ntdll.dll", EntryPoint = "NtQuerySystemInformation")]
    internal static partial int NtQuerySystemInformation(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);

    [LibraryImport("powrprof.dll", EntryPoint = "CallNtPowerInformation")]
    internal static partial int CallNtPowerInformation(
        int InformationLevel,
        IntPtr lpInputBuffer,
        int nInputBufferSize,
        out SYSTEM_POWER_INFORMATION lpOutputBuffer,
        int nOutputBufferSize);
    #endregion

    #region Memory
    [LibraryImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [LibraryImport("psapi.dll", EntryPoint = "GetPerformanceInfo")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);
    #endregion
}
