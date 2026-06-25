// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;

using ZenMonitor.Core.Models;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Utils;

/// <summary>
/// Provides system-level helper operations that are abstracted for testability.
/// </summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("linux")]
public class UtilsLinux : IUtilsLinux
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public int ProcessorCount => Environment.ProcessorCount;

    /// <inheritdoc />
    public ProcessResult RunProcess(string programName, params string[] arguments) => ProcessHelper(programName, arguments);

    /// <inheritdoc />
    public ProcessResult TerminateProcess(string processName) => ProcessHelper("pkill", processName);

    /// <inheritdoc />
    public ProcessResult TerminateProcess(int processId) => ProcessHelper("kill", processId.ToString());

    /// <inheritdoc />
    public ProcessResult KillProcess(string processName) => ProcessHelper("pkill", "-9", processName);

    /// <inheritdoc />
    public ProcessResult KillProcess(int processId) => ProcessHelper("kill", "-9", processId.ToString());

    private static ProcessResult ProcessHelper(string fileName, params string[] arguments)
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

        var errorTask = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEnd();
        var error = errorTask.GetAwaiter().GetResult();

        process.WaitForExit();

        return new ProcessResult(process.ExitCode, output.Trim(), error.Trim());
    }
}
