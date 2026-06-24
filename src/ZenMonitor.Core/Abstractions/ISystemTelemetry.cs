// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions.Telemetry;

namespace ZenMonitor.Core.Abstractions;

/// <summary>
/// Aggregates all system telemetry interfaces into a single entry point.
/// Each sub-interface is exposed as a property so consumers (and DI) can
/// access individual monitors or the whole system at once.
/// </summary>
public interface ISystemTelemetry
{
    /// <summary>
    /// Runs Update() for every interface in ISystemTelemetry in alphabetical order.
    /// </summary>
    void UpdateAll();

    /// <summary>Gets the CPU telemetry service.</summary>
    ICpu Cpu { get; }

    /// <summary>Gets the Drive telemetry service.</summary>
    IDrive Drive { get; }

    /// <summary>Gets the GPU telemetry service.</summary>
    IGpu Gpu { get; }

    /// <summary>Gets the Memory telemetry service.</summary>
    IMemory Memory { get; }

    /// <summary>Gets the Network telemetry service.</summary>
    INetwork Network { get; }

    /// <summary>Gets the Process telemetry service.</summary>
    IProcess Process { get; }

    /// <summary>Gets the System telemetry service.</summary>
    ISystem System { get; }
}
