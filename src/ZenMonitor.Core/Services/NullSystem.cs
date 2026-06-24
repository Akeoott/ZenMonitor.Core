// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions.Telemetry;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="ISystem"/> implementation that returns all-zero / empty defaults.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class NullSystem : ISystem
{
    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <summary>Returns an empty string.</summary>
    public string GetKernelVersion() => "";

    /// <summary>Returns an empty string.</summary>
    public string GetHostname() => "";

    /// <summary>Returns 0.</summary>
    public double GetUptimeSeconds() => 0;

    /// <summary>Returns 0.</summary>
    public int GetRunningTasks() => 0;

    /// <summary>Returns 0.</summary>
    public int GetTotalTasks() => 0;
}
