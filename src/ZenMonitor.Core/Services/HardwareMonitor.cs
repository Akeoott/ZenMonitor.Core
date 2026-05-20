// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IHardwareMonitor"/> that exposes all Null sub-services.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class HardwareMonitor(
    ICpu cpu,
    IDrive drive,
    IGpu gpu,
    IMemory memory,
    INetwork network,
    ISystem system) : IHardwareMonitor
{
    /// <summary>Gets the Null CPU monitoring service.</summary>
    public ICpu Cpu { get; } = cpu;

    /// <summary>Gets the Null Drive monitoring service.</summary>
    public IDrive Drive { get; } = drive;

    /// <summary>Gets the Null GPU monitoring service.</summary>
    public IGpu Gpu { get; } = gpu;

    /// <summary>Gets the Null Memory monitoring service.</summary>
    public IMemory Memory { get; } = memory;

    /// <summary>Gets the Null Network monitoring service.</summary>
    public INetwork Network { get; } = network;

    /// <summary>Gets the Null System monitoring service.</summary>
    public ISystem System { get; } = system;
}
