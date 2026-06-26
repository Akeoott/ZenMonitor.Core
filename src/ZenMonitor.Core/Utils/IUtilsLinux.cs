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
    /// Runs a process and returns results.
    /// </summary>
    /// <param name="fileName">process to run (has to exist in PATH)</param>
    /// <param name="arguments">args the process receives</param>
    /// <returns><see cref="ProcessResult"/> containing exit code, stdout and stderr from said process</returns>
    ProcessResult RunProcess(string fileName, params string[] arguments);
}
