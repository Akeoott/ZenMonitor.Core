// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Windows.Services.Telemetry;

/// <summary>
/// Windows implementation of <see cref="ISystemTel"/>
/// </summary>
[SupportedOSPlatform("windows")]
public class SystemTel(ILogger<SystemTel> logger) : ISystemTel
{
    private SystemInfoSnapshot _snapshot = new(
        "", "", 0, 0, 0);

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
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch System info");
            return new SystemInfoSnapshot("Error", "Error", 0, 0, 0);
        }
    }
}
