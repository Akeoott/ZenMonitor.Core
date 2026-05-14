// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IHardwareMonitor"/> that exposes all Null sub-services.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class NullHardwareMonitor : IHardwareMonitor
{
    /// <summary>Gets the Null CPU monitoring service.</summary>
    public ICpu Cpu { get; } = new NullCpu();

    /// <summary>Gets the Null Drive monitoring service.</summary>
    public IDrive Drive { get; } = new NullDrive();

    /// <summary>Gets the Null GPU monitoring service.</summary>
    public IGpu Gpu { get; } = new NullGpu();

    /// <summary>Gets the Null Memory monitoring service.</summary>
    public IMemory Memory { get; } = new NullMemory();

    /// <summary>Gets the Null Network monitoring service.</summary>
    public INetwork Network { get; } = new NullNetwork();

    /// <summary>Gets the Null System monitoring service.</summary>
    public ISystem System { get; } = new NullSystem();
}
