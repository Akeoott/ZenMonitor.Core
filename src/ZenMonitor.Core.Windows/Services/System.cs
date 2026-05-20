// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="ISystem"/> that reads system information
/// from <c>/proc/sys</c>, <c>/proc/uptime</c>, and <c>/proc/loadavg</c>.
/// </summary>
[SupportedOSPlatform("windows")]
public class System(ILogger<System> logger, IFileSystem fileSystem) : ISystem
{
    private readonly ILogger<System> _logger = logger;
    private readonly IFileSystem _fileSystem = fileSystem;
    private SystemInfoSnapshot _snapshot = new(
        "", "", 0, 0, 0);

    /// <summary>Updates all cached system info by reading from system files.</summary>
    public void Update() => _snapshot = FetchSystemInfo();

    /// <summary>Returns the operating system kernel version string.</summary>
    public string GetKernelVersion() => _snapshot.KernelVersion;

    /// <summary>Returns the system hostname.</summary>
    public string GetHostname() => _snapshot.Hostname;

    /// <summary>Returns the system uptime in seconds.</summary>
    public double GetUptimeSeconds() => _snapshot.UptimeSeconds;

    /// <summary>Returns the number of currently running tasks/processes.</summary>
    public int GetRunningTasks() => _snapshot.RunningTasks;

    /// <summary>Returns the total number of tasks/processes on the system.</summary>
    public int GetTotalTasks() => _snapshot.TotalTasks;

    private SystemInfoSnapshot FetchSystemInfo()
    {
        try
        {
            _logger.LogTrace("Fetching all System info...");
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch System info");
            return new SystemInfoSnapshot("Error", "Error", 0, 0, 0);
        }
    }
}
