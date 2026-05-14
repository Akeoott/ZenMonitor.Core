// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Services;

/// <summary>
/// No-op <see cref="IMemory"/> implementation that returns all-zero defaults.
/// Used as a fallback when the platform is unsupported or detection fails.
/// </summary>
public sealed class NullMemory : IMemory
{
    /// <summary>No-op update — does nothing.</summary>
    public void Update() { }

    /// <summary>Returns 0.</summary>
    public double GetMemTotal() => 0;

    /// <summary>Returns 0.</summary>
    public double GetMemFree() => 0;

    /// <summary>Returns 0.</summary>
    public double GetMemAvailable() => 0;

    /// <summary>Returns 0.</summary>
    public double GetMemUsed() => 0;

    /// <summary>Returns 0.</summary>
    public double GetCached() => 0;

    /// <summary>Returns 0.</summary>
    public double GetSwapTotal() => 0;

    /// <summary>Returns 0.</summary>
    public double GetSwapFree() => 0;
}
