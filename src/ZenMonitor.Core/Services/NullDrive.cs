// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IDrive"/> implementation that returns an empty array.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class NullDrive : IDrive
{
    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <summary>Returns an empty array.</summary>
    public DriveMountInfo[] GetMountInfos() => [];
}
