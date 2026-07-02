// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides system-level information such as kernel version, hostname,
/// uptime, and running process counts.
/// </summary>
public interface ISystemTel
{
    /// <summary>Updates all cached system info by reading from system files.</summary>
    void Update();

    /// <summary>
    /// Get the entire snapshot record of <see cref="ISystemTel"/>
    /// </summary>
    /// <returns><see cref="SystemInfoSnapshot"/> and all its underlying data</returns>
    SystemInfoSnapshot GetSnapshot();

    /// <summary>Returns the operating system kernel version string.</summary>
    string GetKernelVersion();

    /// <summary>Returns the system hostname.</summary>
    string GetHostname();

    /// <summary>Returns the system uptime in seconds.</summary>
    double GetUptimeSeconds();

    /// <summary>Returns the number of currently running tasks/processes.</summary>
    int GetRunningTasks();

    /// <summary>Returns the total number of tasks/processes on the system.</summary>
    int GetTotalTasks();
}
