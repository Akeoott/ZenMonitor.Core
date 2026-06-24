// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class NetworkTests
{
    private readonly Mock<ILogger<Network>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();
    private readonly Mock<IUtilsLinux> _mockUtils = new();

    private Network CreateNetwork() => new(_mockLogger.Object, _mockFileSystem, _mockUtils.Object);

    [Fact]
    public void Expected_PerInterfaceMetricsAndAggregates()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev1()));
        _mockFileSystem.AddFile("/sys/class/net/eth0/operstate", new MockFileData(TestData.OperstateEth0()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();

        var networks = network.GetNetworks();
        Assert.Single(networks);
        Assert.Equal("eth0", networks[0].Name);
        Assert.Equal(10_000_000, networks[0].TotalBytesDownloaded);
        Assert.Equal(5_000_000, networks[0].TotalBytesUploaded);
        Assert.True(networks[0].IsUp);

        // Second snapshot produces speed delta
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev2()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 2));

        network.Update();
        networks = network.GetNetworks();

        Assert.Single(networks);
        Assert.Equal(12_000_000, networks[0].TotalBytesDownloaded);
        Assert.Equal(5_600_000, networks[0].TotalBytesUploaded);
        Assert.Equal(1_000_000, networks[0].DownloadSpeed);
        Assert.Equal(300_000, networks[0].UploadSpeed);
        Assert.Equal(1_000_000, network.GetDownloadSpeed());
        Assert.Equal(300_000, network.GetUploadSpeed());
    }

    [Fact]
    public void Edge_SkipsLoopback()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(
            "Inter-|   Receive                                                |  Transmit\n" +
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n" +
            "    lo:   500000     500    0    0    0    0          0         0   500000     500    0    0    0    0       0          0\n"
        ));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();

        Assert.Empty(network.GetNetworks());
        Assert.Equal(0, network.GetDownloadSpeed());
        Assert.Equal(0, network.GetUploadSpeed());
    }

    [Fact]
    public void Edge_InterfaceIsDown()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev1()));
        _mockFileSystem.AddFile("/sys/class/net/eth0/operstate", new MockFileData("down"));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();
        var networks = network.GetNetworks();

        Assert.Single(networks);
        Assert.Equal("eth0", networks[0].Name);
        Assert.False(networks[0].IsUp);
    }

    [Fact]
    public void Edge_OperstateFileMissing()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(TestData.NetDev1()));
        // No /sys/class/net/eth0/operstate file added

        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();
        var networks = network.GetNetworks();

        Assert.Single(networks);
        Assert.False(networks[0].IsUp);
    }

    [Fact]
    public void Error_MissingProcNetDev()
    {
        var network = CreateNetwork();
        network.Update();

        Assert.Empty(network.GetNetworks());
        Assert.Equal(0, network.GetDownloadSpeed());
        Assert.Equal(0, network.GetUploadSpeed());
    }

    [Fact]
    public void Error_SkipsMalformedLines()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(
            "Inter-|   Receive                                                |  Transmit\n" +
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n" +
            "   eth0: invalid bytes                                           more stuff\n" +
            "   eth1: 1000 10 0 0 0 0 0 0 2000 20 0 0 0 0 0 0\n"
        ));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();
        var networks = network.GetNetworks();

        Assert.Single(networks);
        Assert.Equal("eth1", networks[0].Name);
        Assert.Equal(1000, networks[0].TotalBytesDownloaded);
        Assert.Equal(2000, networks[0].TotalBytesUploaded);
    }

    [Fact]
    public void Error_MissingColonSeparator()
    {
        _mockFileSystem.AddFile("/proc/net/dev", new MockFileData(
            "Inter-|   Receive                                                |  Transmit\n" +
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n" +
            "   eth0 1000 10 0 0 0 0 0 0 2000 20 0 0 0 0 0 0\n" // no colon
        ));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0));

        var network = CreateNetwork();
        network.Update();

        Assert.Empty(network.GetNetworks());
    }
}
