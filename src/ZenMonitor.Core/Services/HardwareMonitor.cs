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
    IProcess process,
    ISystem system) : IHardwareMonitor
{
    /// <inheritdoc />
    public void UpdateAll()
    {
        Cpu.Update();
        Drive.Update();
        Gpu.Update();
        Memory.Update();
        Network.Update();
        Process.Update();
        System.Update();
    }

    /// <inheritdoc />
    public ICpu Cpu { get; } = cpu;

    /// <inheritdoc />
    public IDrive Drive { get; } = drive;

    /// <inheritdoc />
    public IGpu Gpu { get; } = gpu;

    /// <inheritdoc />
    public IMemory Memory { get; } = memory;

    /// <inheritdoc />
    public INetwork Network { get; } = network;

    /// <inheritdoc />
    public IProcess Process { get; } = process;

    /// <inheritdoc />
    public ISystem System { get; } = system;
}
