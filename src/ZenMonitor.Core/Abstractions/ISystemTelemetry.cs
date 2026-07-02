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
    ICpuTel CpuTel { get; }

    /// <summary>Gets the Drive telemetry service.</summary>
    IDriveTel DriveTel { get; }

    /// <summary>Gets the GPU telemetry service.</summary>
    IGpuTel GpuTel { get; }

    /// <summary>Gets the Memory telemetry service.</summary>
    IMemoryTel MemoryTel { get; }

    /// <summary>Gets the Network telemetry service.</summary>
    INetworkTel NetworkTel { get; }

    /// <summary>Gets the Process telemetry service.</summary>
    IProcessTel ProcessTel { get; }

    /// <summary>Gets the System telemetry service.</summary>
    ISystemTel SystemTel { get; }
}
