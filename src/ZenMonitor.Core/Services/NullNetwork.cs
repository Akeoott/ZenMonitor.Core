// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="INetwork"/> implementation.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class NullNetwork : INetwork
{
    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <summary>Returns 0.</summary>
    public long GetDownloadSpeed() => 0;

    /// <summary>Returns 0.</summary>
    public long GetUploadSpeed() => 0;

    /// <summary>Returns an empty array.</summary>
    public NetworkInterfaces[] GetNetworks() => [];
}
