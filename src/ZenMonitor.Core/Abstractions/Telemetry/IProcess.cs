// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides process monitoring capabilities, including enumeration of
/// running processes and their resource usage.
/// </summary>
public interface IProcess
{
    /// <summary>Updates the internal process snapshot with the latest data from the system.</summary>
    void Update();

    /// <summary>
    /// Get the entire snapshot record of <see cref="IProcess"/>
    /// </summary>
    /// <returns><see cref="ProcessInfoSnapshot"/> and all its underlying data</returns>
    ProcessInfoSnapshot GetSnapshot();

    /// <summary>Returns the total number of processes detected during the last update.</summary>
    int GetTotalProcesses();

    /// <summary>Returns a read-only span of <see cref="ProcessDetail"/> records representing the current process snapshot.</summary>
    ReadOnlySpan<ProcessDetail> GetProcesses();
}
