// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class SystemTests
{
    // Need to use explicit naming cause of conflict with namespace System
    private readonly Mock<ILogger<Core.Linux.Services.System>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;

    public SystemTests()
    {
        _mockLogger = new Mock<ILogger<Core.Linux.Services.System>>();
        _mockFileSystem = new MockFileSystem();
    }

    private Core.Linux.Services.System CreateSystem() => new(_mockLogger.Object, _mockFileSystem);

    [Fact]
    public void Update_ParsesSystemInfoCorrectly()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();

        system.Update();

        Assert.NotEqual("Unknown", system.GetKernelVersion());
        Assert.NotEqual("Unknown", system.GetHostname());
    }

    [Fact]
    public void GetKernelVersion_ReturnsKernelVersion()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("7.0.2-2-cachyos", system.GetKernelVersion());
    }

    [Fact]
    public void GetHostname_ReturnsHostname()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal("arch", system.GetHostname());
    }

    [Fact]
    public void GetUptimeSeconds_ReturnsUptimeSeconds()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal(17125.98, system.GetUptimeSeconds());
    }

    [Fact]
    public void GetRunningTasks_ReturnsRunningTasks()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal(1, system.GetRunningTasks());
    }

    [Fact]
    public void GetTotalTasks_ReturnsTotalTasks()
    {
        _mockFileSystem.AddFile("/proc/sys/kernel/osrelease", new MockFileData(TestData.OsRelease()));
        _mockFileSystem.AddFile("/proc/sys/kernel/hostname", new MockFileData(TestData.Hostname()));
        _mockFileSystem.AddFile("/proc/uptime", new MockFileData(TestData.Uptime()));
        _mockFileSystem.AddFile("/proc/loadavg", new MockFileData(TestData.LoadAvg()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var system = CreateSystem();
        system.Update();

        Assert.Equal(2209, system.GetTotalTasks());
    }
}
