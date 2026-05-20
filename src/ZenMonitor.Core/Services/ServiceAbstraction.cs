// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Interfaces;

namespace ZenMonitor.Core.Services;

/// <summary>
/// Implements getters for IServiceAbstraction. Namely ILinux and IWindows.
/// </summary>
/// <param name="linux">Interface for linux specific service helper abstractions</param>
/// <param name="windows">Interface for windows specific service helper abstractions</param>
public sealed class ServiceAbstraction(ILinux linux, IWindows windows) : IServiceAbstraction
{
    /// <inheritdoc />
    public ILinux Linux { get; } = linux;

    /// <inheritdoc />
    public IWindows Windows { get; } = windows;
}
