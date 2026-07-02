// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Windows.Services.Telemetry;

/// <summary>
/// Windows implementation of <see cref="IProcessTel"/> that provides process
/// monitoring capabilities. Currently, returns an empty snapshot as the
/// implementation is pending.
/// </summary>
[SupportedOSPlatform("windows")]
public class ProcessTel(ILogger<ProcessTel> logger) : IProcessTel
{
    private ProcessInfoSnapshot _snapshot = new(0, []);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchProcessInfo();

    /// <inheritdoc />
    public ProcessInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public int GetTotalProcesses() => _snapshot.TotalProcesses;

    /// <inheritdoc />
    public ProcessDetail[] GetProcesses() => _snapshot.ProcessDetails;

    private ProcessInfoSnapshot FetchProcessInfo()
    {
        logger.LogWarning("IProcessTel not implemented yet. Returning empty snapshot.");
        return new ProcessInfoSnapshot(0,[]);
    }
}
