// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Abstractions;

/// <summary>
/// Placeholder interface for network monitoring.
/// Implementation is pending and currently returns empty stubs.
/// </summary>
public interface INetwork
{
    /// <summary>Updates all cached network metrics.</summary>
    void Update();

    /// <summary>Returns the current total download speed in bytes per second.</summary>
    long GetDownloadSpeed();

    /// <summary>Returns the current total upload speed in bytes per second.</summary>
    long GetUploadSpeed();

    /// <summary>
    /// Returns information of all detected networks as an array.
    /// See the record `ConnectedNetworks`.
    /// </summary>
    ConnectedNetworks[] GetNetworks();
}
