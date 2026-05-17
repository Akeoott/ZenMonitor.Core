// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

public record ConnectedNetworks(
    string Name,
    long DownloadSpeed,
    long UploadSpeed,
    long TotalBytesDownloaded,
    long TotalBytesUploaded,
    bool IsUp
);

/// <summary>
/// Placeholder snapshot for network metrics.
/// This will be redesigned with proper network interface statistics.
/// </summary>
public record NetworkInfoSnapshot(
    long DownloadSpeed,
    long UploadSpeed,
    ConnectedNetworks[] Networks
);
