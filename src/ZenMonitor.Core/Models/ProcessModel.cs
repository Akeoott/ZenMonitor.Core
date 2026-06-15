// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

/// <summary>Represents the current execution state of a process.</summary>
public enum ProcessState : byte
{
    /// <summary>The process state could not be determined.</summary>
    Unknown,
    /// <summary>The process is currently running or ready to run.</summary>
    Running,
    /// <summary>The process is sleeping (interruptible or idle).</summary>
    Sleeping,
    /// <summary>The process is in an uninterruptible disk sleep (usually I/O).</summary>
    DiskSleep,
    /// <summary>The process is a zombie (terminated but not yet reaped by its parent).</summary>
    Zombie,
    /// <summary>The process has been stopped (e.g., by a SIGSTOP signal).</summary>
    Stopped,
    /// <summary>The process is being traced and is stopped by a ptrace event.</summary>
    TracingStop,
    /// <summary>The process is dead (should not be visible in most listings).</summary>
    Dead
}

/// <summary>Provides a snapshot of a single process's identifier, metadata, and resource usage.</summary>
/// <param name="Pid">The unique process identifier.</param>
/// <param name="Program">The name of the executable or program.</param>
/// <param name="Command">The full command line used to start the process.</param>
/// <param name="User">The name of the user who owns the process.</param>
/// <param name="State">The current execution state of the process.</param>
/// <param name="Threads">The number of threads in the process.</param>
/// <param name="MemoryUsage">The memory usage of the process in megabytes (MB).</param>
/// <param name="CpuUsage">The current CPU usage percentage of the process.</param>
public readonly record struct ProcessDetail(
    int Pid,
    string Program,
    string Command,
    string User,
    ProcessState State,
    int Threads,
    int MemoryUsage,
    double CpuUsage
);

/// <summary>A snapshot of all processes and metadata obtained during a single monitoring tick.</summary>
/// <param name="TotalProcesses">The total number of processes in the snapshot.</param>
/// <param name="ProcessDetails">The array of individual process details.</param>
public record ProcessInfoSnapshot(
    int TotalProcesses,
    ProcessDetail[] ProcessDetails
);
