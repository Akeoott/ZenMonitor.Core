// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Linux.Services.Telemetry;

/// <summary>
/// Linux implementation of <see cref="ISystemTel"/> that reads system information
/// from <c>/proc/sys</c>, <c>/proc/uptime</c>, and <c>/proc/loadavg</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class SystemTel(ILogger<SystemTel> logger, IFileSystem fileSystem) : ISystemTel
{
    private SystemInfoSnapshot _snapshot = new("", "", 0, 0, 0);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchSystemInfo();

    /// <inheritdoc />
    public SystemInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public string GetKernelVersion() => _snapshot.KernelVersion;

    /// <inheritdoc />
    public string GetHostname() => _snapshot.Hostname;

    /// <inheritdoc />
    public double GetUptimeSeconds() => _snapshot.UptimeSeconds;

    /// <inheritdoc />
    public int GetRunningTasks() => _snapshot.RunningTasks;

    /// <inheritdoc />
    public int GetTotalTasks() => _snapshot.TotalTasks;

    private SystemInfoSnapshot FetchSystemInfo()
    {
        try
        {
            logger.LogTrace("Fetching all System info...");

            var kernel = fileSystem.File.ReadAllText("/proc/sys/kernel/osrelease").Trim();
            var hostname = fileSystem.File.ReadAllText("/proc/sys/kernel/hostname").Trim();

            var uptimeParts = fileSystem.File.ReadAllText("/proc/uptime").Trim().Split(' ');
            var uptime = double.Parse(uptimeParts[0]);

            var loadParts = fileSystem.File.ReadAllText("/proc/loadavg").Trim().Split(' ');

            var tasks = loadParts[3].Split('/');
            var running = int.Parse(tasks[0]);
            var total = int.Parse(tasks[1]);

            return new SystemInfoSnapshot(
                kernel, hostname, uptime,
                running, total);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch System info");
            return new SystemInfoSnapshot("Error", "Error", 0, 0, 0);
        }
    }
}
