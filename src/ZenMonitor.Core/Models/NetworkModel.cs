// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

/// <summary>
/// Represents a connected network with its current transfer rates
/// and cumulative byte counters along other details.
/// </summary>
/// <param name="Name">The detected network name (e.g. eth0, wlan0).</param>
/// <param name="DownloadSpeed">Current download speed in bytes per second.</param>
/// <param name="UploadSpeed">Current upload speed in bytes per second.</param>
/// <param name="TotalBytesDownloaded">Cumulative bytes received since boot.</param>
/// <param name="TotalBytesUploaded">Cumulative bytes transmitted since boot.</param>
/// <param name="IsUp"><c>true</c> if the detected network is administratively up.</param>
public record ConnectedNetworks(
    string Name,
    long DownloadSpeed,
    long UploadSpeed,
    long TotalBytesDownloaded,
    long TotalBytesUploaded,
    bool IsUp
);

/// <summary>
/// Snapshot of all network interface metrics at a point in time.
/// </summary>
/// <param name="DownloadSpeed">Aggregate download speed across all connected networks.</param>
/// <param name="UploadSpeed">Aggregate upload speed across all connected networks.</param>
/// <param name="Networks">Per-interface network metrics.</param>
public record NetworkInfoSnapshot(
    long DownloadSpeed,
    long UploadSpeed,
    ConnectedNetworks[] Networks
);
