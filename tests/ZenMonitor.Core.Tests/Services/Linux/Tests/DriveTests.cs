// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Linux.Services;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class DriveTests
{
    private readonly Mock<ILogger<Drive>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;
    private readonly Mock<IHelper> _mockHelper;

    public DriveTests()
    {
        _mockLogger = new Mock<ILogger<Drive>>();
        _mockFileSystem = new MockFileSystem();
        _mockHelper = new Mock<IHelper>();
    }

    private Drive CreateDrive() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    [Fact]
    public void GetMountInfos_ReturnsMountInfos()
    {
        // Arrange
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats1()));
        _mockHelper.Setup(h => h.RunProcess("df", "-T -B1"))
                   .Returns(new ProcessResult(0, TestData.DfOutput(), ""));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));

        var drive = CreateDrive();

        // Act
        drive.Update();
        var mountInfos = drive.GetMountInfos();

        // Assert
        Assert.Single(mountInfos);
        Assert.Equal("/", mountInfos[0].MountPoint);
        Assert.Equal("/dev/sda1", mountInfos[0].DeviceName);
        Assert.Equal("ext4", mountInfos[0].FileSystem);
        Assert.Equal(1000000000, mountInfos[0].TotalBytes);
        Assert.Equal(400000000, mountInfos[0].AvailableBytes);
        Assert.Equal(500000000, mountInfos[0].UsedBytes);
        Assert.Equal(0, mountInfos[0].IOUsage); // first call always 0

        // Arrange
        _mockFileSystem.AddFile("/proc/diskstats", new MockFileData(TestData.DiskStats2()));
        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 5));

        // Act
        drive.Update();
        mountInfos = drive.GetMountInfos();

        // Assert
        Assert.Equal(0.375, mountInfos[0].IOUsage);
    }
}
