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
public class NetworkTests
{
    private readonly Mock<ILogger<Network>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;
    private readonly Mock<IServiceAbstraction> _mockHelper;

    public NetworkTests()
    {
        _mockLogger = new Mock<ILogger<Network>>();
        _mockFileSystem = new MockFileSystem();
        _mockHelper = new Mock<IServiceAbstraction>();
    }

    private Network CreateNetwork() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    [Fact]
    public void GetNetworks_ReturnsNetworkData()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev1()));
        _mockFileSystem.AddFile("/sys/class/net/eth0/operstate", new MockFileData(TestData.OperstateEth0()));
        _mockHelper.Setup(h => h.Linux.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();

        network.Update();
        var networks = network.GetNetworks();
        var downloadSpeed = network.GetDownloadSpeed();
        var uploadSpeed = network.GetUploadSpeed();

        Assert.Single(networks);
        Assert.Equal("eth0", networks[0].Name);
        Assert.Equal(10_000_000, networks[0].TotalBytesDownloaded);
        Assert.Equal(5_000_000, networks[0].TotalBytesUploaded);
        Assert.True(networks[0].IsUp);
        Assert.Equal(0, networks[0].DownloadSpeed);  // first call, no prior data
        Assert.Equal(0, networks[0].UploadSpeed);
        Assert.Equal(0, downloadSpeed);
        Assert.Equal(0, uploadSpeed);

        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev2()));
        _mockHelper.Setup(h => h.Linux.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 2));

        network.Update();
        networks = network.GetNetworks();
        downloadSpeed = network.GetDownloadSpeed();
        uploadSpeed = network.GetUploadSpeed();

        Assert.Single(networks);
        Assert.Equal("eth0", networks[0].Name);
        Assert.Equal(12_000_000, networks[0].TotalBytesDownloaded);
        Assert.Equal(5_600_000, networks[0].TotalBytesUploaded);
        Assert.Equal(1_000_000, networks[0].DownloadSpeed);
        Assert.Equal(300_000, networks[0].UploadSpeed);
        Assert.Equal(1_000_000, downloadSpeed);
        Assert.Equal(300_000, uploadSpeed);
    }

    [Fact]
    public void GetNetworks_SkipsLoopback()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(
            "Inter-|   Receive                                                |  Transmit\n" +
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n" +
            "    lo:   500000     500    0    0    0    0          0         0   500000     500    0    0    0    0       0          0\n"
        ));
        _mockHelper.Setup(h => h.Linux.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();

        network.Update();
        var networks = network.GetNetworks();

        Assert.Empty(networks);
        Assert.Equal(0, network.GetDownloadSpeed());
        Assert.Equal(0, network.GetUploadSpeed());
    }

    [Fact]
    public void GetNetworks_InterfaceIsDown()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev1()));
        _mockFileSystem.AddFile("/sys/class/net/eth0/operstate", new MockFileData("down"));
        _mockHelper.Setup(h => h.Linux.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();

        network.Update();
        var networks = network.GetNetworks();

        Assert.Single(networks);
        Assert.Equal("eth0", networks[0].Name);
        Assert.False(networks[0].IsUp);
    }
}
