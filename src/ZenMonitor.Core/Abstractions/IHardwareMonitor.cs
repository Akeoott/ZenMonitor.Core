// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Abstractions;

/// <summary>
/// Aggregates all hardware monitoring interfaces into a single entry point.
/// Each sub-interface is exposed as a property so consumers (and DI) can
/// access individual monitors or the whole system at once.
/// </summary>
public interface IHardwareMonitor
{
    /// <summary>
    /// Runs Update() for every interface in IHardwareMonitor in alphabetical order.
    /// </summary>
    void UpdateAll();

    /// <summary>Gets the CPU monitoring service.</summary>
    ICpu Cpu { get; }

    /// <summary>Gets the Drive monitoring service.</summary>
    IDrive Drive { get; }

    /// <summary>Gets the GPU monitoring service.</summary>
    IGpu Gpu { get; }

    /// <summary>Gets the Memory monitoring service.</summary>
    IMemory Memory { get; }

    /// <summary>Gets the Network monitoring service.</summary>
    INetwork Network { get; }

    /// <summary>Gets the Process monitoring service.</summary>
    IProcess Process { get; }

    /// <summary>Gets the System monitoring service.</summary>
    ISystem System { get; }
}
