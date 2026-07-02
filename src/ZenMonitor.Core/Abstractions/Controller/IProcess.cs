// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Controller;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Controller;

/// <summary>
/// Provides access to running, terminating and killing processes.
/// </summary>
public interface IProcess
{
    /// <summary>
    /// Runs an external process and captures its output, error, and exit code.
    /// </summary>
    /// <param name="programName">The executable file name or path.</param>
    /// <param name="arguments">Command-line arguments to pass.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult Run(string programName, params string[] arguments);

    /// <summary>
    /// Terminates a process by its name using SIGTERM.
    /// </summary>
    /// <param name="processName">The name of the process to terminate.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult Terminate(string processName);

    /// <summary>
    /// Terminates a process by its process ID using SIGTERM.
    /// </summary>
    /// <param name="processId">The ID of the process to terminate.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult Terminate(int processId);

    /// <summary>
    /// Forcefully kills a process by its name using SIGKILL.
    /// </summary>
    /// <param name="processName">The name of the process to kill.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult Kill(string processName);

    /// <summary>
    /// Forcefully kills a process by its process ID using SIGKILL.
    /// </summary>
    /// <param name="processId">The ID of the process to kill.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult Kill(int processId);

    /// <summary>
    /// Sets the scheduling priority of a process.
    /// </summary>
    /// <param name="processId">The ID of the process to modify.</param>
    /// <param name="priority">The desired scheduling priority.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult SetPriority(int processId, ProcessPriority priority);
}
