// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.IO.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IDrive"/> that reads mounted filesystem
/// information from <c>df</c> and disk I/O stats from <c>/proc/diskstats</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class Drive(ILogger<Drive>? logger, IFileSystem fileSystem, IUtilsLinux helper) : IDrive
{
    private readonly ILogger<Drive> _logger = logger ?? NullLogger<Drive>.Instance;
    private DriveInfoSnapshot _snapshot = new([]);

    private readonly Dictionary<string, (long ioTime, DateTime time)> _previousDiskStats = [];

    /// <inheritdoc />
    public void Update() => _snapshot = FetchDriveInfo();

    /// <inheritdoc />
    public DriveMountInfo[] GetMountInfos() => _snapshot.MountInfos;

    private DriveInfoSnapshot FetchDriveInfo()
    {
        _logger.LogTrace("Fetching all Drive info...");

        var mountInfos = ReadMountInfos();

        return new DriveInfoSnapshot(mountInfos);
    }

    private DriveMountInfo[] ReadMountInfos()
    {
        var ioUsages = ReadIoUsages();
        var dfOutput = RunDf("-T -B1");

        try
        {
            if (string.IsNullOrEmpty(dfOutput))
            {
                throw new InvalidOperationException(
                    "dfOutput is empty or null. Failed getting valid values to format DriveMountInfo[]");
            }

            var lines = dfOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
            var mountInfos = new List<DriveMountInfo>();
            var index = 0;

            HashSet<string> pseudoFileSystems = [
                "tmpfs", "proc", "sysfs", "devtmpfs", "devpts",
            "fusectl", "securityfs", "cgroup", "cgroup2", "pstore",
            "debugfs", "hugetlbfs", "mqueue", "configfs", "bpf", "tracefs"
            ];

            foreach (var line in lines)
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;

                var deviceName = parts[0];
                var fileSystems = parts[1];
                if (pseudoFileSystems.Contains(fileSystems)) continue;

                var totalBytes = long.Parse(parts[2]);
                var usedBytes = long.Parse(parts[3]);
                var availableBytes = long.Parse(parts[4]);
                var mountPoint = parts[6];

                var shortName = deviceName.StartsWith("/dev/") ? deviceName[5..] : deviceName;
                var ioUsage = ioUsages.GetValueOrDefault(shortName, 0.0);

                mountInfos.Add(new DriveMountInfo(
                    index++,
                    mountPoint,
                    deviceName,
                    fileSystems,
                    totalBytes,
                    availableBytes,
                    usedBytes,
                    ioUsage
                ));
            }

            return [.. mountInfos];
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            _logger.LogError(ex, "Failed to get values for DriveMountInfo[]. Returning empty array...");
            return [];
        }
    }

    private Dictionary<string, double> ReadIoUsages()
    {
        var ioUsages = new Dictionary<string, double>();
        try
        {
            var lines = fileSystem.File.ReadAllLines("/proc/diskstats");

            foreach (var line in lines)
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 14) continue;

                var name = parts[2];
                var ioTime = long.Parse(parts[12]);

                if (_previousDiskStats.TryGetValue(name, out var prev))
                {
                    var deltaTime = (helper.UtcNow - prev.time).TotalMilliseconds;
                    double deltaIo = ioTime - prev.ioTime;
                    var usage = deltaTime > 0 ? deltaIo / deltaTime * 100 : 0;
                    ioUsages[name] = usage;
                }
                else
                {
                    ioUsages[name] = 0;
                }

                _previousDiskStats[name] = (ioTime, helper.UtcNow);
            }
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Could not find /proc/diskstats");
            return [];
        }

        return ioUsages;
    }

    private string RunDf(string arguments)
    {
        var result = helper.RunProcess("df", arguments);
        try
        {
            return result.ExitCode != 0
                ? throw new InvalidOperationException($"df error: {result.StandardError}")
                : result.StandardOutput;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "df failed: {Error}", result.StandardError);
            return "";
        }
    }
}
