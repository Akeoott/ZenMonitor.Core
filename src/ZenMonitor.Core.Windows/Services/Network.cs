// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="INetwork"/> that gets all network related telemetry.
/// </summary>
[SupportedOSPlatform("windows")]
public class Network(ILogger<Network> logger) : INetwork
{
    private readonly ILogger<Network> _logger = logger;
    private NetworkInfoSnapshot _snapshot = new(0, 0, []);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchNetworkInfo();

    /// <inheritdoc />
    public long GetDownloadSpeed() => _snapshot.DownloadSpeed;

    /// <inheritdoc />
    public long GetUploadSpeed() => _snapshot.UploadSpeed;

    /// <inheritdoc />
    public ConnectedNetworks[] GetNetworks() => _snapshot.Networks;

    private NetworkInfoSnapshot FetchNetworkInfo()
    {
        _logger.LogTrace("Fetching all Network info...");
        _logger.LogInformation("Network has not been implemented yet for windows.");

        return new NetworkInfoSnapshot(0, 0, []);
    }
}
