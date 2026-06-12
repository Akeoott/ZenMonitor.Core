// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Models;

/// <summary>
/// Represents information about a single mounted filesystem.
/// </summary>
/// <param name="Index">Zero-based index in the mount array.</param>
/// <param name="MountPoint">Path to the mount point (e.g. "/").</param>
/// <param name="DeviceName">Device name or path (e.g. "/dev/nvme0n1p2").</param>
/// <param name="FileSystem">Type of filesystem (e.g. "btrfs", "ext4", "ntfs").</param>
/// <param name="TotalBytes">Total capacity in bytes.</param>
/// <param name="AvailableBytes">Available free space in bytes.</param>
/// <param name="UsedBytes">Used space in bytes.</param>
/// <param name="IoUsage">Disk I/O usage percentage.</param>
public record DriveMountInfo(
    int Index,
    string MountPoint,
    string DeviceName,
    string FileSystem,
    long TotalBytes,
    long AvailableBytes,
    long UsedBytes,
    double IoUsage
);

/// <summary>
/// A snapshot of all mounted filesystem metrics collected at a single point in time.
/// </summary>
/// <param name="MountInfos">Array of mounted drive information entries.</param>
public record DriveInfoSnapshot(
    DriveMountInfo[] MountInfos
);
