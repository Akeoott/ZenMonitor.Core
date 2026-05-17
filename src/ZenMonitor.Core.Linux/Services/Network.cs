// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="INetwork"/>.
/// Currently a placeholder — network metrics are not yet implemented.
/// </summary>
[SupportedOSPlatform("linux")]
public class Network(ILogger<Network> logger) : INetwork
{
    private readonly ILogger<Network> _logger = logger;
    private NetworkInfoSnapshot _snapshot = new(0, 0, []);

    /// <summary>Updates all cached network metrics.</summary>
    public void Update() => _snapshot = FetchNetworkInfo();

    /// <summary>Returns the aggregate download speed across all interfaces in bytes per second.</summary>
    public long GetDownloadSpeed() => _snapshot.DownloadSpeed;

    /// <summary>Returns the aggregate upload speed across all interfaces in bytes per second.</summary>
    public long GetUploadSpeed() => _snapshot.UploadSpeed;

    /// <summary>Returns metric information for each active network interface.</summary>
    public NetworkInterfaces[] GetNetworks() => _snapshot.Networks;

    private NetworkInfoSnapshot FetchNetworkInfo()
    {
        _logger.LogTrace("Fetching all Network info...");
        _logger.LogInformation("Network has not been implemented yet for linux.");

        return new NetworkInfoSnapshot(0, 0, []);
    }

}
