// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Abstractions.Telemetry;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="ISystemTelemetry"/> that exposes all Null sub-services.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SystemTelemetry(
    ICpuTel cpuTel,
    IDriveTel driveTel,
    IGpuTel gpuTel,
    IMemoryTel memoryTel,
    INetworkTel networkTel,
    IProcessTel processTel,
    ISystemTel systemTel) : ISystemTelemetry
{
    /// <inheritdoc />
    public void UpdateAll()
    {
        CpuTel.Update();
        DriveTel.Update();
        GpuTel.Update();
        MemoryTel.Update();
        NetworkTel.Update();
        ProcessTel.Update();
        SystemTel.Update();
    }

    /// <inheritdoc />
    public ICpuTel CpuTel { get; } = cpuTel;

    /// <inheritdoc />
    public IDriveTel DriveTel { get; } = driveTel;

    /// <inheritdoc />
    public IGpuTel GpuTel { get; } = gpuTel;

    /// <inheritdoc />
    public IMemoryTel MemoryTel { get; } = memoryTel;

    /// <inheritdoc />
    public INetworkTel NetworkTel { get; } = networkTel;

    /// <inheritdoc />
    public IProcessTel ProcessTel { get; } = processTel;

    /// <inheritdoc />
    public ISystemTel SystemTel { get; } = systemTel;
}
