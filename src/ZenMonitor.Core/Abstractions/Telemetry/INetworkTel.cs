// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Placeholder interface for network monitoring.
/// Implementation is pending and currently returns empty stubs.
/// </summary>
public interface INetworkTel
{
    /// <summary>Updates all cached network metrics.</summary>
    void Update();

    /// <summary>
    /// Get the entire snapshot record of <see cref="INetworkTel"/>
    /// </summary>
    /// <returns><see cref="NetworkInfoSnapshot"/> and all its underlying data</returns>
    NetworkInfoSnapshot GetSnapshot();

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
