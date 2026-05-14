// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IHardwareMonitor"/> that aggregates all
/// platform-specific hardware monitoring services.
/// </summary>
public sealed class LinuxHardwareMonitor(
    ICpu cpu,
    IDrive drive,
    IGpu gpu,
    IMemory memory,
    INetwork network,
    ISystem system) : IHardwareMonitor
{
    /// <summary>Gets the CPU monitoring service.</summary>
    public ICpu Cpu { get; } = cpu;

    /// <summary>Gets the Drive monitoring service.</summary>
    public IDrive Drive { get; } = drive;

    /// <summary>Gets the GPU monitoring service.</summary>
    public IGpu Gpu { get; } = gpu;

    /// <summary>Gets the Memory monitoring service.</summary>
    public IMemory Memory { get; } = memory;

    /// <summary>Gets the Network monitoring service.</summary>
    public INetwork Network { get; } = network;

    /// <summary>Gets the System monitoring service.</summary>
    public ISystem System { get; } = system;
}
