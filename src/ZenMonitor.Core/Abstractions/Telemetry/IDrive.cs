// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Abstractions.Telemetry;

/// <summary>
/// Provides storage device monitoring, including mount points,
/// capacity, and disk I/O usage.
/// </summary>
public interface IDrive
{
    /// <summary>Updates all cached drive metrics by reading from system files.</summary>
    void Update();

    /// <summary>Returns information about all mounted filesystems.</summary>
    DriveMountInfo[] GetMountInfos();
}
