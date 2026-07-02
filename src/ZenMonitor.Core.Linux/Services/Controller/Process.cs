// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions.Controller;
using ZenMonitor.Core.Models.Controller;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Services.Controller;

/// <summary>>
/// Linux implementation of <see cref="IProcessCon"/>
/// that allows the user to run, terminate and kill processes
/// </summary>
[SupportedOSPlatform("linux")]
public class ProcessCon(IUtilsLinux utils) : IProcessCon
{
    /// <inheritdoc />
    public ProcessResult Run(string programName, params string[] arguments) => utils.RunProcess(programName, arguments);

    /// <inheritdoc />
    public ProcessResult Terminate(string processName) => utils.RunProcess("pkill", processName);

    /// <inheritdoc />
    public ProcessResult Terminate(int processId) => utils.RunProcess("kill", processId.ToString());

    /// <inheritdoc />
    public ProcessResult Kill(string processName) => utils.RunProcess("pkill", "-9", processName);

    /// <inheritdoc />
    public ProcessResult Kill(int processId) => utils.RunProcess("kill", "-9", processId.ToString());

    /// <inheritdoc />
    public ProcessResult SetPriority(int processId, ProcessPriority priority)
    {
        var niceValue = priority switch
        {
            ProcessPriority.Idle => 19,
            ProcessPriority.BelowNormal => 10,
            ProcessPriority.Normal => 0,
            ProcessPriority.AboveNormal => -5,
            ProcessPriority.High => -10,
            ProcessPriority.RealTime => -20,
            _ => 0
        };
        return utils.RunProcess("renice", niceValue.ToString(), "-p", processId.ToString());
    }
}
