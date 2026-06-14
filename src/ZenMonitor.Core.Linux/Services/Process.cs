// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

[SupportedOSPlatform("linux")]
public class Process(ILogger<Process>? logger) : IProcess
{
    private readonly ILogger<Process> _logger = logger ?? NullLogger<Process>.Instance;
    private ProcessInfoSnapshot _snapshot = new(0, []);

    public void Update() => _snapshot = FetchProcessInfo();

    public int GetTotalProcesses() => _snapshot.TotalProcesses;

    public ProcessDetail[] GetProcesses() => _snapshot.ProcessDetails;

    private ProcessInfoSnapshot FetchProcessInfo()
    {
        _logger.LogWarning("IProcess not implemented yet. Returning empty snapshot.");
        return new ProcessInfoSnapshot(0,[]);
    }
}
