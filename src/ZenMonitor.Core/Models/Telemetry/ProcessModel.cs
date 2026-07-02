// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models.Telemetry;

/// <summary>Represents the current execution state of a process.</summary>
public enum ProcessState : byte
{
    /// <summary>The process state could not be determined.</summary>
    Unknown,
    /// <summary>The process is currently running or ready to run.</summary>
    Running,
    /// <summary>The process is sleeping, idle, or waiting for resources (e.g., I/O).</summary>
    Sleeping,
    /// <summary>The process has terminated but its parent has not yet reaped it.</summary>
    Zombie,
    /// <summary>The process has been stopped or suspended.</summary>
    Stopped,
    /// <summary>The process is dead and should no longer appear in listings.</summary>
    Dead
}

/// <summary>Defines the scheduling priority of a process, from lowest to highest.</summary>
public enum ProcessPriority : byte
{
    /// <summary>Lowest priority; runs only when the system is idle.</summary>
    Idle,
    /// <summary>Priority below normal (e.g., positive nice value on Unix systems).</summary>
    BelowNormal,
    /// <summary>Default or normal scheduling priority.</summary>
    Normal,
    /// <summary>Priority above normal (e.g., negative nice value on Unix systems).</summary>
    AboveNormal,
    /// <summary>High priority, reserved for time-critical tasks.</summary>
    High,
    /// <summary>Highest priority; real-time scheduling. May require root priviliges.</summary>
    RealTime
}

/// <summary>Provides a snapshot of a single process's identifier, metadata, and resource usage.</summary>
/// <param name="Pid">The unique process identifier.</param>
/// <param name="Program">The name of the executable or program.</param>
/// <param name="Command">The full command line used to start the process.</param>
/// <param name="User">The name of the user who owns the process.</param>
/// <param name="State">The current execution state of the process.</param>
/// <param name="Priority">The scheduling priority of the process.</param>
/// <param name="Threads">The number of threads in the process.</param>
/// <param name="MemoryUsage">The memory usage of the process in megabytes (MB).</param>
/// <param name="CpuUsage">The current CPU usage percentage of the process.</param>
public record ProcessDetail(
    int Pid,
    string Program,
    string Command,
    string User,
    ProcessState State,
    ProcessPriority Priority,
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
