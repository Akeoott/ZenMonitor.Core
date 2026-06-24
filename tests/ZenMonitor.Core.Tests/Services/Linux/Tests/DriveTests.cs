// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class DriveTests
{
    private readonly Mock<ILogger<Drive>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();
    private readonly Mock<IUtilsLinux> _mockHelper = new();

    private Drive CreateDrive() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    [Fact]
    public void Expected_MountInfoWithIOUsage()
    {
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats1()));
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, TestData.DfOutput(), ""));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));

        var drive = CreateDrive();
        drive.Update();
        var mountInfos = drive.GetMountInfos();

        Assert.Single(mountInfos);
        Assert.Equal("/", mountInfos[0].MountPoint);
        Assert.Equal("/dev/sda1", mountInfos[0].DeviceName);
        Assert.Equal("ext4", mountInfos[0].FileSystem);
        Assert.Equal(1000000000, mountInfos[0].TotalBytes);
        Assert.Equal(400000000, mountInfos[0].AvailableBytes);
        Assert.Equal(500000000, mountInfos[0].UsedBytes);
        Assert.Equal(0, mountInfos[0].IoUsage);
        Assert.Equal(0, mountInfos[0].Index);

        // Second call with updated diskstats produces IO delta
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats2()));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 5));

        drive.Update();
        mountInfos = drive.GetMountInfos();

        Assert.Equal(0.375, mountInfos[0].IoUsage);
    }

    [Fact]
    public void Edge_FiltersPseudoFilesystems()
    {
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats1()));
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, TestData.DfOutput(), "")); // df has tmpfs and ext4
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));

        var drive = CreateDrive();
        drive.Update();

        // tmpfs should be filtered out, only /dev/sda1 remains
        Assert.Single(drive.GetMountInfos());
    }

    [Fact]
    public void Error_DfNonZeroExit_ReturnsEmptyMountInfos()
    {
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats1()));
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(1, string.Empty, "df: cannot read table"));

        var drive = CreateDrive();
        drive.Update();

        Assert.Empty(drive.GetMountInfos());
    }

    [Fact]
    public void Error_MissingProcDiskstats_ReturnsMountInfosWithZeroIO()
    {
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, TestData.DfOutput(), ""));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));

        var drive = CreateDrive();
        drive.Update();

        var mountInfos = drive.GetMountInfos();
        Assert.Single(mountInfos);
        Assert.Equal("/dev/sda1", mountInfos[0].DeviceName);
        Assert.Equal(0, mountInfos[0].IoUsage);
    }

    [Fact]
    public void Error_DfMalformedOutput()
    {
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, "Not enough columns", ""));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats1()));

        var drive = CreateDrive();
        drive.Update();

        Assert.Empty(drive.GetMountInfos());
    }

    [Fact]
    public void Error_ProcDiskstatsMalformedLine()
    {
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(
            "8       0 sda 79252 10076 6669686 71586 1056792 25175 31019894\n" // fewer than 14 fields
        ));
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, TestData.DfOutput(), ""));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));

        var drive = CreateDrive();
        drive.Update();
        var mountInfos = drive.GetMountInfos();

        // Drive still reports the mount info, but IO usage defaults to 0
        Assert.Single(mountInfos);
        Assert.Equal(0, mountInfos[0].IoUsage);
    }
}
