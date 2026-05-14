// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

/// <summary>
/// A snapshot of system memory metrics collected at a single point in time.
/// All values are in gibibytes (GiB).
/// </summary>
/// <param name="MemTotal">Total physical memory in GiB.</param>
/// <param name="MemFree">Free (unused) physical memory in GiB.</param>
/// <param name="MemAvailable">Available memory including reclaimable cache in GiB.</param>
/// <param name="MemUsed">Used physical memory (total - available) in GiB.</param>
/// <param name="Cached">Cached memory in GiB.</param>
/// <param name="SwapTotal">Total swap space in GiB.</param>
/// <param name="SwapFree">Free swap space in GiB.</param>
public record MemoryInfoSnapshot(
    double MemTotal,
    double MemFree,
    double MemAvailable,
    double MemUsed,
    double Cached,
    double SwapTotal,
    double SwapFree
);
