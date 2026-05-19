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
public class CpuTests
{
    private readonly Mock<ILogger<Cpu>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;
    private readonly Mock<IHelper> _mockHelper;

    public CpuTests()
    {
        _mockLogger = new Mock<ILogger<Cpu>>();
        _mockFileSystem = new MockFileSystem();
        _mockHelper = new Mock<IHelper>();
    }

    private Cpu CreateCpu() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    [Fact]
    public void GetCpuUsage_ReturnsCpuUsage()
    {
        string cpuinfo = TestData.CpuInfo();
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(cpuinfo));

        string stat1 = TestData.Stat1();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat1));
        var cpu = CreateCpu();

        cpu.Update();

        string stat2 = TestData.Stat2();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat2));

        cpu.Update();

        Assert.Equal(4, cpu.GetCpuUsage());
    }

    [Fact]
    public void GetCpuTemp_ReturnsIntelPackageTemp()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo2Core()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/name", new MockFileData(TestData.HwmonIntelName()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_input", new MockFileData(TestData.HwmonIntelTemp1Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_label", new MockFileData(TestData.HwmonIntelTemp1Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_input", new MockFileData(TestData.HwmonIntelTemp2Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_label", new MockFileData(TestData.HwmonIntelTemp2Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_input", new MockFileData(TestData.HwmonIntelTemp3Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_label", new MockFileData(TestData.HwmonIntelTemp3Label()));

        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal(45, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 42), new CpuCoreTemp(1, 43)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetCpuTemp_ReturnsAmdOverallTemp()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo2Core()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/name", new MockFileData(TestData.HwmonAmdName()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_input", new MockFileData(TestData.HwmonAmdTemp1Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_label", new MockFileData(TestData.HwmonAmdTemp1Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_input", new MockFileData(TestData.HwmonAmdTemp2Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_label", new MockFileData(TestData.HwmonAmdTemp2Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_input", new MockFileData(TestData.HwmonAmdTemp3Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_label", new MockFileData(TestData.HwmonAmdTemp3Label()));

        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal(51, cpu.GetCpuTemp());
        Assert.Equal([new CpuCoreTemp(0, 49), new CpuCoreTemp(1, 49)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetCpuName_ReturnsCpuName()
    {
        string cpuinfo = TestData.CpuInfo();
        string stat = TestData.Stat1();
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(cpuinfo));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat));

        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal("AMD Ryzen 7 7800X3D 8-Core Processor", cpu.GetCpuName());
    }

    [Fact]
    public void GetCpuSpeed_ReturnsCpuSpeed()
    {
        string cpuinfo = TestData.CpuInfo();
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(cpuinfo));

        string stat1 = TestData.Stat1();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat1));
        var cpu = CreateCpu();

        cpu.Update();

        string stat2 = TestData.Stat2();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat2));

        cpu.Update();

        Assert.Equal(3997.17, cpu.GetCpuSpeed());
    }

    [Fact]
    public void GetCoreSpeeds_ReturnsCoreSpeeds()
    {
        string cpuinfo = TestData.CpuInfo();
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(cpuinfo));

        string stat1 = TestData.Stat1();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat1));
        var cpu = CreateCpu();

        cpu.Update();

        string stat2 = TestData.Stat2();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat2));

        cpu.Update();

        var speeds = new[]
        {
            new CpuCoreSpeed(0, 4399.214), new CpuCoreSpeed(1, 4375.453), new CpuCoreSpeed(2, 4398.893), new CpuCoreSpeed(3, 4394.1),
            new CpuCoreSpeed(4, 4368.925), new CpuCoreSpeed(5, 4395.762), new CpuCoreSpeed(6, 4397.023), new CpuCoreSpeed(7, 4398.63),
            new CpuCoreSpeed(8, 2983.319), new CpuCoreSpeed(9, 4370.437), new CpuCoreSpeed(10, 2983.319), new CpuCoreSpeed(11, 4399.153),
            new CpuCoreSpeed(12, 3817.651), new CpuCoreSpeed(13, 4306.206), new CpuCoreSpeed(14, 2983.319), new CpuCoreSpeed(15, 2983.319)
        };

        Assert.Equal(speeds, cpu.GetCoreSpeeds());
    }

    [Fact]
    public void GetCoreUsages_ReturnsCoreUsages()
    {
        string cpuinfo = TestData.CpuInfo();
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(cpuinfo));

        string stat1 = TestData.Stat1();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat1));
        var cpu = CreateCpu();

        cpu.Update();

        string stat2 = TestData.Stat2();
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(stat2));

        cpu.Update();

        var usages = new[]
        {
            new CpuCoreUsage(0, 7), new CpuCoreUsage(1, 17), new CpuCoreUsage(2, 3), new CpuCoreUsage(3, 12),
            new CpuCoreUsage(4, 3), new CpuCoreUsage(5, 6), new CpuCoreUsage(6, 1), new CpuCoreUsage(7, 2),
            new CpuCoreUsage(8, 1), new CpuCoreUsage(9, 1), new CpuCoreUsage(10, 2), new CpuCoreUsage(11, 2),
            new CpuCoreUsage(12, 4), new CpuCoreUsage(13, 1), new CpuCoreUsage(14, 1), new CpuCoreUsage(15, 0)
        };

        Assert.Equal(usages, cpu.GetCoreUsages());
    }

    [Fact]
    public void GetCoreTemps_ReturnsIntelCoreTemps()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo2Core()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/name", new MockFileData(TestData.HwmonIntelName()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_input", new MockFileData(TestData.HwmonIntelTemp1Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_label", new MockFileData(TestData.HwmonIntelTemp1Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_input", new MockFileData(TestData.HwmonIntelTemp2Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_label", new MockFileData(TestData.HwmonIntelTemp2Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_input", new MockFileData(TestData.HwmonIntelTemp3Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_label", new MockFileData(TestData.HwmonIntelTemp3Label()));

        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal([new CpuCoreTemp(0, 42), new CpuCoreTemp(1, 43)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetCoreTemps_ReturnsAmdCcdTemps()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo2Core()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/name", new MockFileData(TestData.HwmonAmdName()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_input", new MockFileData(TestData.HwmonAmdTemp1Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_label", new MockFileData(TestData.HwmonAmdTemp1Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_input", new MockFileData(TestData.HwmonAmdTemp2Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_label", new MockFileData(TestData.HwmonAmdTemp2Label()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_input", new MockFileData(TestData.HwmonAmdTemp3Input()));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp3_label", new MockFileData(TestData.HwmonAmdTemp3Label()));

        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal([new CpuCoreTemp(0, 49), new CpuCoreTemp(1, 49)], cpu.GetCoreTemps());
    }

    [Fact]
    public void GetPowerDraw_ReturnsPowerDraw()
    {
        const string energyUjPath = "/sys/class/powercap/intel-rapl:0/energy_uj";

        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockFileSystem.AddFile(energyUjPath, new MockFileData(TestData.EnergyUj1()));
        var cpu = CreateCpu();

        cpu.Update();

        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 6));
        _mockFileSystem.AddFile(energyUjPath, new MockFileData(TestData.EnergyUj2()));

        cpu.Update();

        Assert.Equal(46.03, cpu.GetPowerDraw());
    }
}
