// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ZenMonitor.Core.Windows.ServiceAbstraction;

[SupportedOSPlatform("windows")]
public partial class Windows
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_INFO
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
    internal struct SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION
    {
        public long IdleTime;
        public long KernelTime;
        public long UserTime;
        public long Reserved1;
        public long Reserved2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_POWER_INFORMATION
    {
        public ulong MaxIdlenessAllowed;
        public ulong Idleness;
        public ulong TimeRemaining;
        public byte CoolingMode;
    }

    internal const int SystemProcessorPerformanceInformation = 8;
    internal const int SystemPowerInformation = 5;
}
