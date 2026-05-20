// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ZenMonitor.Core.Windows.ServiceAbstraction;

[SupportedOSPlatform("windows")]
public partial class Windows
{
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
}
