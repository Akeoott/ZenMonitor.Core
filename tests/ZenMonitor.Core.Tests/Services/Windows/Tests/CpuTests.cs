// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Windows.Services;

namespace ZenMonitor.Core.Tests.Services.Windows.Tests;

[Trait("Platform", "Windows")]
[SupportedOSPlatform("windows")]
public class CpuTests
{
    private readonly Mock<ILogger<Cpu>> _mockLogger;
    private readonly Mock<IHelper> _mockHelper;
    private readonly Mock<IWindows> _mockWindows;

    public CpuTests()
    {
        _mockLogger = new Mock<ILogger<Cpu>>();
        _mockWindows = new Mock<IWindows>();
        _mockHelper = new Mock<IHelper>();
        _mockHelper.Setup(h => h.Windows).Returns(_mockWindows.Object);
    }

    private Cpu CreateCpu() => new(_mockLogger.Object, _mockHelper.Object);

    [Fact]
    public void GetCpuName_ReturnsCpuName()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("AMD Ryzen 7 7800X3D 8-Core Processor");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(8);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(4200);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([.. Enumerable.Repeat(new CpuTickInfo(100, 200, 300), 8)]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("AMD Ryzen 7 7800X3D 8-Core Processor", cpu.GetCpuName());
    }

    [Fact]
    public void GetCpuSpeed_ReturnsAverageCpuSpeed()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(4);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([.. Enumerable.Repeat(new CpuTickInfo(100, 200, 300), 4)]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(3600, cpu.GetCpuSpeed());
    }

    [Fact]
    public void GetCpuUsage_ReturnsCpuUsage()
    {
        // First snapshot: first call returns 0%
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(2);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(150, 250, 350)
        ]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetCpuUsage());

        // Second snapshot: diffTotal = (2100+3100) - (2000+3000) = 200
        // diffIdle = 1100 - 1000 = 100
        // usage = (200-100)/200*100 = 50%
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1100, 2100, 3100));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(110, 210, 310),
            new CpuTickInfo(160, 260, 360)
        ]);

        cpu.Update();

        Assert.Equal(50, cpu.GetCpuUsage());
    }

    [Fact]
    public void GetCoreUsages_ReturnsCoreUsages()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(2);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(150, 250, 350)
        ]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal([new CpuCoreUsage(0, 0), new CpuCoreUsage(1, 0)], cpu.GetCoreUsages());

        // Second snapshot with known delta => 50% each
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1100, 2100, 3100));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(110, 210, 310),
            new CpuTickInfo(160, 260, 360)
        ]);

        cpu.Update();

        Assert.Equal([new CpuCoreUsage(0, 50), new CpuCoreUsage(1, 50)], cpu.GetCoreUsages());
    }

    [Fact]
    public void GetCpuTemp_ReturnsCpuTemp()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(2);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(150, 250, 350)
        ]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(65);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(65, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 65), new CpuCoreTemp(1, 65)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetCpuTemp_ReturnsZeroWhenNoSensor()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(2);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(150, 250, 350)
        ]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 0), new CpuCoreTemp(1, 0)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetPowerDraw_ReturnsPowerDraw()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Returns("Test CPU");
        _mockWindows.Setup(w => w.GetProcessorCount()).Returns(2);
        _mockWindows.Setup(w => w.GetProcessorBaseFrequencyMHz()).Returns(3600);
        _mockWindows.Setup(w => w.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockWindows.Setup(w => w.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(150, 250, 350)
        ]);
        _mockWindows.Setup(w => w.GetCpuTemperature()).Returns(0);
        _mockWindows.Setup(w => w.GetCpuPowerDraw()).Returns(46.03);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(46.03, cpu.GetPowerDraw());
    }

    [Fact]
    public void Update_ReturnsErrorSnapshotOnFailure()
    {
        _mockWindows.Setup(w => w.GetProcessorName()).Throws<InvalidOperationException>();

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("Error", cpu.GetCpuName());
        Assert.Equal(0, cpu.GetCpuUsage());
        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.Equal(0, cpu.GetPowerDraw());
        Assert.Equal([], cpu.GetCoreSpeeds());
        Assert.Equal([], cpu.GetCoreUsages());
        Assert.Equal([], cpu.GetCoreTemps());
    }
}
