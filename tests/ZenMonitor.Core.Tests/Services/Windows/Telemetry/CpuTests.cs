// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Models;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;
using ZenMonitor.Core.Windows.Services.Telemetry;

namespace ZenMonitor.Core.Tests.Services.Windows.Telemetry;

[Trait("Platform", "Windows")]
[SupportedOSPlatform("windows")]
public class CpuTests
{
    private readonly Mock<ILogger<Cpu>> _mockLogger = new();
    private readonly Mock<IUtilsWindows> _mockUtils = new();

    private Cpu CreateCpu() => new(_mockLogger.Object, _mockUtils.Object);

    private void SetupStdCpuInfo(string name = "Test CPU", int coreCount = 2, int baseMhz = 3600)
    {
        _mockUtils.Setup(w => w.RawCpu.GetProcessorName()).Returns(name);
        _mockUtils.Setup(w => w.RawCpu.GetProcessorCount()).Returns(coreCount);
        _mockUtils.Setup(w => w.RawCpu.GetBaseFrequencyMHz()).Returns(baseMhz);
    }

    private void SetupStdTicks(long idle = 1000, long kernel = 2000, long user = 3000, int cores = 2)
    {
        _mockUtils.Setup(w => w.RawCpu.GetSystemTimes()).Returns(new CpuTickInfo(idle, kernel, user));
        _mockUtils.Setup(w => w.RawCpu.GetPerCoreTimes()).Returns(
            [.. Enumerable.Repeat(new CpuTickInfo(100, 200, 300), cores)]);
    }

    [Fact]
    public void Expected_CpuNameAndSpeed()
    {
        SetupStdCpuInfo("AMD Ryzen 7 7800X3D 8-Core Processor", 8, 4200);
        SetupStdTicks(cores: 8);
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(0);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("AMD Ryzen 7 7800X3D 8-Core Processor", cpu.GetCpuName());
        Assert.Equal(4200, cpu.GetCpuSpeed());
    }

    [Fact]
    public void Expected_CpuUsageDelta()
    {
        SetupStdCpuInfo();
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(0);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Returns(0);

        // First snapshot: returns 0%
        SetupStdTicks();
        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetCpuUsage());

        // Second snapshot: diffTotal = (2100+3100)-(2000+3000) = 200, diffIdle = 1100-1000 = 100, usage = (200-100)/200*100 = 50%
        SetupStdTicks(1100, 2100, 3100);
        cpu.Update();

        Assert.Equal(50, cpu.GetCpuUsage());
    }

    [Fact]
    public void Expected_CoreUsages()
    {
        SetupStdCpuInfo();
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(0);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Returns(0);

        // First snapshot: per-core ticks (idle: 100, kernel: 200, user: 300) each
        _mockUtils.Setup(w => w.RawCpu.GetSystemTimes()).Returns(new CpuTickInfo(1000, 2000, 3000));
        _mockUtils.Setup(w => w.RawCpu.GetPerCoreTimes()).Returns([
            new CpuTickInfo(100, 200, 300),
            new CpuTickInfo(100, 200, 300)
        ]);
        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal([new CpuCoreUsage(0, 0), new CpuCoreUsage(1, 0)], cpu.GetCoreUsages());

        // Second snapshot: per-core ticks (idle: 110, kernel: 210, user: 310) each
        // diffTotal = (210+310)-(200+300) = 20, diffIdle = 110-100 = 10
        // usage = (20-10)/20*100 = 50%
        _mockUtils.Setup(w => w.RawCpu.GetSystemTimes()).Returns(new CpuTickInfo(1100, 2100, 3100));
        _mockUtils.Setup(w => w.RawCpu.GetPerCoreTimes()).Returns([
            new CpuTickInfo(110, 210, 310),
            new CpuTickInfo(110, 210, 310)
        ]);
        cpu.Update();

        Assert.Equal([new CpuCoreUsage(0, 50), new CpuCoreUsage(1, 50)], cpu.GetCoreUsages());
    }

    [Fact]
    public void Expected_TemperatureAndPowerDraw()
    {
        SetupStdCpuInfo();
        SetupStdTicks();
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(65);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Returns(46.03);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(65, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 65), new CpuCoreTemp(1, 65)], cpu.GetCoreTemps());
        Assert.Equal(46.03, cpu.GetPowerDraw());
    }

    [Fact]
    public void Edge_TemperatureZeroWhenNoSensor()
    {
        SetupStdCpuInfo();
        SetupStdTicks();
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(0);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Returns(0);

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 0), new CpuCoreTemp(1, 0)], cpu.GetCoreTemps());
    }

    [Fact]
    public void Error_ReturnsErrorSnapshotOnFailure()
    {
        _mockUtils.Setup(w => w.RawCpu.GetProcessorName()).Throws<InvalidOperationException>();

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

    [Fact]
    public void Error_PowerDrawHandlesException()
    {
        SetupStdCpuInfo();
        SetupStdTicks();
        _mockUtils.Setup(w => w.RawCpu.GetTemperature()).Returns(0);
        _mockUtils.Setup(w => w.RawCpu.GetPowerDraw()).Throws<Exception>();

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetPowerDraw());
    }
}
