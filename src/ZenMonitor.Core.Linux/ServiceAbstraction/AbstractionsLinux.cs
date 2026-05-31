// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.ServiceAbstraction;

/// <summary>
/// Provides system-level helper operations that are abstracted for testability.
/// </summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("linux")]
public class AbstractionsLinux : IAbstractionsLinux
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public ProcessResult RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return new ProcessResult(process.ExitCode, output.Trim(), error.Trim());
    }
}
