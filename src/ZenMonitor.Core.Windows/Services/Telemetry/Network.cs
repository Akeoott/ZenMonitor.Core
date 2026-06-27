// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Windows.Services.Telemetry;

/// <summary>
/// Windows implementation of <see cref="INetwork"/> that gets all network related telemetry.
/// </summary>
[SupportedOSPlatform("windows")]
public class Network(ILogger<Network> logger) : INetwork
{
    private NetworkInfoSnapshot _snapshot = new(0, 0, []);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchNetworkInfo();

    /// <inheritdoc />
    public NetworkInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public long GetDownloadSpeed() => _snapshot.DownloadSpeed;

    /// <inheritdoc />
    public long GetUploadSpeed() => _snapshot.UploadSpeed;

    /// <inheritdoc />
    public ConnectedNetworks[] GetNetworks() => _snapshot.Networks;

    private NetworkInfoSnapshot FetchNetworkInfo()
    {
        logger.LogTrace("Fetching all Network info...");
        logger.LogInformation("Network has not been implemented yet for windows.");

        return new NetworkInfoSnapshot(0, 0, []);
    }
}
