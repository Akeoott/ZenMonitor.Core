// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models.Telemetry;

/// <summary>
/// Known GPU vendors for hardware-specific implementation selection.
/// </summary>
public enum GpuVendor
{
    /// <summary>GPU vendor could not be detected or is unsupported.</summary>
    Unknown,

    /// <summary>AMD graphics hardware.</summary>
    Amd,

    /// <summary>NVIDIA graphics hardware.</summary>
    Nvidia,

    /// <summary>Intel graphics hardware.</summary>
    Intel,
}
