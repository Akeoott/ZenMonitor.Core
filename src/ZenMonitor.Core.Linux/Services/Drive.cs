// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IDrive"/> that reads mounted filesystem
/// information from <c>df</c> and disk I/O stats from <c>/proc/diskstats</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class Drive(ILogger<Drive> logger, IFileSystem fileSystem, IServiceAbstraction helper) : IDrive
{
    private readonly ILogger<Drive> _logger = logger;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IServiceAbstraction _helper = helper;
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
        var ioUsages = ReadIOUsages();
        string dfOutput = RunDf("-T -B1");

        try
        {
            if (string.IsNullOrEmpty(dfOutput))
            {
                throw new InvalidOperationException(
                    $"dfOutput is empty or null. Failed getting valid values to format DriveMountInfo[]");
            }

            var lines = dfOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
            var mountInfos = new List<DriveMountInfo>();
            int index = 0;

            HashSet<string> pseudoFileSystems = [
                "tmpfs", "proc", "sysfs", "devtmpfs", "devpts",
            "fusectl", "securityfs", "cgroup", "cgroup2", "pstore",
            "debugfs", "hugetlbfs", "mqueue", "configfs", "bpf", "tracefs"
            ];

            foreach (var line in lines)
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;

                string deviceName = parts[0];
                string fileSystem = parts[1];
                if (pseudoFileSystems.Contains(fileSystem)) continue;

                long totalBytes = long.Parse(parts[2]);
                long usedBytes = long.Parse(parts[3]);
                long availableBytes = long.Parse(parts[4]);
                string mountPoint = parts[6];

                string shortName = deviceName.StartsWith("/dev/") ? deviceName[5..] : deviceName;
                double ioUsage = ioUsages.GetValueOrDefault(shortName, 0.0);

                mountInfos.Add(new DriveMountInfo(
                    index++,
                    mountPoint,
                    deviceName,
                    fileSystem,
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

    private Dictionary<string, double> ReadIOUsages()
    {
        var ioUsages = new Dictionary<string, double>();
        try
        {
            var lines = _fileSystem.File.ReadAllLines("/proc/diskstats");

            foreach (var line in lines)
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 14) continue;

                string name = parts[2];
                long ioTime = long.Parse(parts[12]);

                if (_previousDiskStats.TryGetValue(name, out var prev))
                {
                    double deltaTime = (_helper.Linux.UtcNow - prev.time).TotalMilliseconds;
                    double deltaIo = ioTime - prev.ioTime;
                    double usage = deltaTime > 0 ? deltaIo / deltaTime * 100 : 0;
                    ioUsages[name] = usage;
                }
                else
                {
                    ioUsages[name] = 0;
                }

                _previousDiskStats[name] = (ioTime, _helper.Linux.UtcNow);
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
        var result = _helper.Linux.RunProcess("df", arguments);
        try
        {
            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"df error: {result.StandardError}");
            }
            else
            {
                return result.StandardOutput;
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "df failed: {Error}", result.StandardError);
            return "";
        }
    }
}
