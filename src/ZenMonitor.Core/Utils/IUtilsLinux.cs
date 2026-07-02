// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Controller;

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
    /// <param name="fileName">The executable file name or path.</param>
    /// <param name="arguments">Command-line arguments to pass.</param>
    /// <returns>A <see cref="ProcessResult"/> containing the captured results.</returns>
    ProcessResult RunProcess(string fileName, params string[] arguments);
}
