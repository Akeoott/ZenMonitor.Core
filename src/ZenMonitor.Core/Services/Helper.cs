// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;

namespace ZenMonitor.Core.Services;

/// <summary>
///
/// </summary>
public sealed class Helper(ILinux linux, IWindows windows) : IServiceAbstraction
{
    /// <summary> </summary>
    public ILinux Linux { get; } = linux;

    /// <summary> </summary>
    public IWindows Windows { get; } = windows;
}
