// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ZenMonitor.Core.Windows.ServiceAbstraction;

[SupportedOSPlatform("windows")]
public partial class AbstractionsWindows
{
    #region Cpu
    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemInfo")]
    private static partial void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemTimes")]
    private static partial int GetSystemTimes(
        out long lpIdleTime,
        out long lpKernelTime,
        out long lpUserTime);

    [LibraryImport("ntdll.dll", EntryPoint = "NtQuerySystemInformation")]
    private static partial int NtQuerySystemInformation(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);
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
