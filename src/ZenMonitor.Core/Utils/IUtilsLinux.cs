// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Utils;

/// <summary>
/// Abstraction for system-level operations that need to be mockable in tests.
/// </summary>
[SupportedOSPlatform("linux")]
public interface IUtilsLinux
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }

    /// <summary>Gets the number of logical processors on the system.</summary>
    int ProcessorCount { get; }

    /// <summary>
    /// Runs an external process and captures its output, error, and exit code.
    /// </summary>
    /// <param name="programName">The executable file name or path.</param>
    /// <param name="arguments">Command-line arguments to pass.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult RunProcess(string programName, params string[] arguments);

    /// <summary>
    /// Terminates a process by its name using SIGTERM.
    /// </summary>
    /// <param name="processName">The name of the process to terminate.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult TerminateProcess(string processName);

    /// <summary>
    /// Terminates a process by its process ID using SIGTERM.
    /// </summary>
    /// <param name="processId">The ID of the process to terminate.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult TerminateProcess(int processId);

    /// <summary>
    /// Forcefully kills a process by its name using SIGKILL.
    /// </summary>
    /// <param name="processName">The name of the process to kill.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult KillProcess(string processName);

    /// <summary>
    /// Forcefully kills a process by its process ID using SIGKILL.
    /// </summary>
    /// <param name="processId">The ID of the process to kill.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult KillProcess(int processId);
}
