// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides system memory monitoring, including total, free, available,
/// used, cached, and swap metrics.
/// </summary>
public interface IMemoryTel
{
    /// <summary>Updates all cached memory metrics by reading from system files.</summary>
    void Update();

    /// <summary>
    /// Get the entire snapshot record of <see cref="IMemoryTel"/>
    /// </summary>
    /// <returns><see cref="MemoryInfoSnapshot"/> and all its underlying data</returns>
    MemoryInfoSnapshot GetSnapshot();

    /// <summary>Returns total physical memory in GiB.</summary>
    double GetMemTotal();

    /// <summary>Returns free physical memory in GiB.</summary>
    double GetMemFree();

    /// <summary>Returns available physical memory in GiB (includes reclaimable cached memory).</summary>
    double GetMemAvailable();

    /// <summary>Returns used physical memory in GiB (total minus available).</summary>
    double GetMemUsed();

    /// <summary>Returns cached memory in GiB.</summary>
    double GetCached();

    /// <summary>Returns total swap space in GiB.</summary>
    double GetSwapTotal();

    /// <summary>Returns free swap space in GiB.</summary>
    double GetSwapFree();
}
