// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models.Telemetry;

/// <summary>
/// A snapshot of system-level information collected at a single point in time.
/// </summary>
/// <param name="KernelVersion">Operating system kernel version string.</param>
/// <param name="Hostname">System hostname.</param>
/// <param name="UptimeSeconds">System uptime in seconds.</param>
/// <param name="RunningTasks">Number of currently running tasks/processes.</param>
/// <param name="TotalTasks">Total number of tasks/processes on the system.</param>
public record SystemInfoSnapshot(
    string KernelVersion,
    string Hostname,
    double UptimeSeconds,
    int RunningTasks,
    int TotalTasks
);
