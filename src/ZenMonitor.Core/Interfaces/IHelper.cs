// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

namespace ZenMonitor.Core.Interfaces;

/// <summary>
/// Abstraction for system-level operations that need to be mockable in tests.
/// </summary>
public interface IHelper
{
    [SupportedOSPlatform("linux")]
    ILinux Linux { get; }

    [SupportedOSPlatform("windows")]
    IWindows Windows { get; }
}
