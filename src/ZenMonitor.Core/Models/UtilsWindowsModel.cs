// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

// Contains non-explicit user facing models only used internally.

/// <summary>
/// Represents a snapshot of processor time counters from the system.
/// </summary>
/// <param name="IdleTime">Total idle time in system ticks.</param>
/// <param name="KernelTime">Total kernel time in system ticks (includes idle time).</param>
/// <param name="UserTime">Total user time in system ticks.</param>
[SupportedOSPlatform("windows")]
public record CpuTickInfo(long IdleTime, long KernelTime, long UserTime);
