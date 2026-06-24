// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class SystemTests
{
    private readonly Mock<ILogger<Core.Linux.Services.Telemetry.System>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();

    private Core.Linux.Services.Telemetry.System CreateSystem() => new(_mockLogger.Object, _mockFileSystem);

    [Fact]
    public void Expected_AllSystemInfo()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("7.0.2-2-cachyos", system.GetKernelVersion());
        Assert.Equal("arch", system.GetHostname());
        Assert.Equal(17125.98, system.GetUptimeSeconds());
        Assert.Equal(1, system.GetRunningTasks());
        Assert.Equal(2209, system.GetTotalTasks());
    }

    [Fact]
    public void Error_MissingOsrelease()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("Error", system.GetKernelVersion());
        Assert.Equal("Error", system.GetHostname());
        Assert.Equal(0, system.GetUptimeSeconds());
        Assert.Equal(0, system.GetRunningTasks());
        Assert.Equal(0, system.GetTotalTasks());
    }

    [Fact]
    public void Error_MissingUptime()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("Error", system.GetKernelVersion());
        Assert.Equal("Error", system.GetHostname());
        Assert.Equal(0, system.GetUptimeSeconds());
        Assert.Equal(0, system.GetRunningTasks());
        Assert.Equal(0, system.GetTotalTasks());
    }

    [Fact]
    public void Error_MissingLoadavg()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("Error", system.GetKernelVersion());
        Assert.Equal("Error", system.GetHostname());
        Assert.Equal(0, system.GetUptimeSeconds());
        Assert.Equal(0, system.GetRunningTasks());
        Assert.Equal(0, system.GetTotalTasks());
    }
}
