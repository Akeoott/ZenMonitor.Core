// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Windows.Utils;

// ReSharper disable InconsistentNaming
[SupportedOSPlatform("windows")]
public partial class UtilsWindows
{
    #region RawCpuTel
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
}
