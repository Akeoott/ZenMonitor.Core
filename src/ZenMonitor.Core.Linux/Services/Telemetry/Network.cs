// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Services.Telemetry;

/// <summary>
/// Linux implementation of <see cref="INetwork"/> that reads network interface metrics
/// from <c>/proc/net/dev</c> and <c>/sys/class/net/*/operstate</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class Network(ILogger<Network>? logger, IFileSystem fileSystem, IUtilsLinux utils) : INetwork
{
    private readonly ILogger<Network> _logger = logger ?? NullLogger<Network>.Instance;
    private NetworkInfoSnapshot _snapshot = new(0, 0, []);

    private readonly Dictionary<string, (long rx, long tx, DateTime time)> _previousNetStats = [];

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
        try
        {
            _logger.LogTrace("Fetching all Network info...");

            var networks = ReadNetworkInterfaces();
            long totalDownloadSpeed = 0;
            long totalUploadSpeed = 0;

            foreach (var net in networks)
            {
                totalDownloadSpeed += net.DownloadSpeed;
                totalUploadSpeed += net.UploadSpeed;
            }

            return new NetworkInfoSnapshot(totalDownloadSpeed, totalUploadSpeed, networks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch network info");
            return new NetworkInfoSnapshot(0, 0, []);
        }
    }

    private ConnectedNetworks[] ReadNetworkInterfaces()
    {
        var networks = new List<ConnectedNetworks>();
        string[] lines;

        try
        {
            lines = [.. fileSystem.File.ReadLines("/proc/net/dev")];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read /proc/net/dev");
            return [];
        }

        // Skip the two header lines
        for (var i = 2; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var parts = line.Split(':', 2);
            if (parts.Length != 2)
                continue;

            var interfaceName = parts[0].Trim();

            // Skip loopback
            if (interfaceName == "lo")
                continue;

            var fields = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 10)
                continue;

            if (!long.TryParse(fields[0], out var rxBytes) ||
                !long.TryParse(fields[8], out var txBytes))
            {
                continue;
            }

            var isUp = IsInterfaceUp(interfaceName);
            long downloadSpeed = 0;
            long uploadSpeed = 0;

            if (_previousNetStats.TryGetValue(interfaceName, out var prev))
            {
                var deltaSec = (utils.UtcNow - prev.time).TotalSeconds;
                if (deltaSec > 0)
                {
                    var deltaRx = rxBytes - prev.rx;
                    var deltaTx = txBytes - prev.tx;

                    if (deltaRx < 0) deltaRx = 0;
                    if (deltaTx < 0) deltaTx = 0;

                    downloadSpeed = (long)Math.Round(deltaRx / deltaSec);
                    uploadSpeed = (long)Math.Round(deltaTx / deltaSec);
                }
            }

            _previousNetStats[interfaceName] = (rxBytes, txBytes, utils.UtcNow);

            networks.Add(new ConnectedNetworks(
                interfaceName,
                downloadSpeed,
                uploadSpeed,
                rxBytes,
                txBytes,
                isUp
            ));
        }

        return [.. networks];
    }

    private bool IsInterfaceUp(string interfaceName)
    {
        try
        {
            var operStatePath = $"/sys/class/net/{interfaceName}/operstate";
            if (!fileSystem.File.Exists(operStatePath))
                return false;

            var state = fileSystem.File.ReadAllText(operStatePath).Trim();
            return string.Equals(state, "up", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read operstate for {Interface}", interfaceName);
            return false;
        }
    }
}
