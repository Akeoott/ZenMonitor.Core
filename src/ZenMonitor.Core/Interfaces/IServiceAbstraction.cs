// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

namespace ZenMonitor.Core.Interfaces;

/// <summary>
/// Abstraction for system-level operations that need to be mockable in tests.
/// Aggregates platform-specific helper interfaces for Linux and Windows.
/// </summary>
public interface IServiceAbstraction
{
    /// <summary>Gets the Linux-specific helper operations.</summary>
    [SupportedOSPlatform("linux")]
    ILinux Linux { get; }

    /// <summary>Gets the Windows-specific helper operations.</summary>
    [SupportedOSPlatform("windows")]
    IWindows Windows { get; }
}
