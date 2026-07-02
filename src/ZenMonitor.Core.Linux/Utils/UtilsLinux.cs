// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;

using ZenMonitor.Core.Models;
using ZenMonitor.Core.Models.Controller;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Utils;

/// <summary>
/// Provides system-level helper operations that are abstracted for testability.
/// </summary>
[SupportedOSPlatform("linux")]
public class UtilsLinux : IUtilsLinux
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public int ProcessorCount => Environment.ProcessorCount;

    /// <inheritdoc />
    public ProcessResult RunProcess(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, output.Trim(), error.Trim());
    }
}
