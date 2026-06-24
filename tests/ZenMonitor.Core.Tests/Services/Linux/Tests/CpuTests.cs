// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class CpuTests
{
    private readonly Mock<ILogger<Cpu>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();
    private readonly Mock<IUtilsLinux> _mockHelper = new();

    private Cpu CreateCpu() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    [Fact]
    public void Expected_CpuNameAndSpeed()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("AMD Ryzen 7 7800X3D 8-Core Processor", cpu.GetCpuName());
        Assert.Equal(3997.17, cpu.GetCpuSpeed());
    }

    [Fact]
    public void Expected_CoreSpeeds()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var cpu = CreateCpu();
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
    public void Expected_CpuUsageAndCoreUsages()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        var cpu = CreateCpu();

        cpu.Update();

        // Second call with new stat data produces the delta
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat2()));
        cpu.Update();

        Assert.Equal(4, cpu.GetCpuUsage());

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
    public void Expected_IntelTemps()
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
    public void Expected_AmdTemps()
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
    public void Expected_PowerDraw()
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

    [Fact]
    public void Edge_UnknownCpuNameWhenNoModelName()
    {
        // cpuinfo has MHz lines but no "model name"
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(
            "processor\t: 0\ncpu MHz\t\t: 3200.000\n\nprocessor\t: 1\ncpu MHz\t\t: 3200.000\n"
        ));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("Unknown CPU", cpu.GetCpuName());
        Assert.Equal(3200.0, cpu.GetCpuSpeed());
    }

    [Fact]
    public void Edge_FallsBackToAverageTempWhenCountMismatch()
    {
        // 4-core CPU but hwmon only provides 2 temperature values
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(
            "processor\t: 0\nmodel name\t: Test CPU\ncpu MHz\t\t: 3200.000\n\n" +
            "processor\t: 1\nmodel name\t: Test CPU\ncpu MHz\t\t: 3200.000\n\n" +
            "processor\t: 2\nmodel name\t: Test CPU\ncpu MHz\t\t: 3200.000\n\n" +
            "processor\t: 3\nmodel name\t: Test CPU\ncpu MHz\t\t: 3200.000\n"
        ));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        // Intel hwmon with only Package temp and Core 0 temp (missing Core 1)
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/name", new MockFileData("coretemp"));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_input", new MockFileData("45000"));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp1_label", new MockFileData("Package id 0"));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_input", new MockFileData("42000"));
        _mockFileSystem.AddFile("/sys/class/hwmon/hwmon0/temp2_label", new MockFileData("Core 0"));

        var cpu = CreateCpu();
        cpu.Update();

        // overall = 45, avg of raw temps = (42) / 1 = 42 → all 4 cores get 42
        Assert.Equal(45, cpu.GetCpuTemp());
        Assert.Equal(4, cpu.GetCoreTemps().Length);
        Assert.All(cpu.GetCoreTemps(), t => Assert.Equal(42, t.Temp));
    }

    [Fact]
    public void Edge_PowerDrawReturnsZeroWhenFileMissing()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0.0, cpu.GetPowerDraw());
    }

    [Fact]
    public void Error_MissingProcCpuinfo()
    {
        // No /proc/cpuinfo added
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("Error", cpu.GetCpuName());
        Assert.Equal(0, cpu.GetCpuSpeed());
        Assert.Equal(0, cpu.GetCpuUsage());
        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.Equal(0.0, cpu.GetPowerDraw());
        Assert.Equal([], cpu.GetCoreSpeeds());
        Assert.Equal([], cpu.GetCoreUsages());
        Assert.Equal([], cpu.GetCoreTemps());
    }

    [Fact]
    public void Error_MissingProcStat()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal("Error", cpu.GetCpuName());
        Assert.Equal(0, cpu.GetCpuSpeed());
        Assert.Equal(0, cpu.GetCpuUsage());
    }

    [Fact]
    public void Error_HwmonDirectoryNotExists()
    {
        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        // /sys/class/hwmon does not exist

        var cpu = CreateCpu();
        cpu.Update();

        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.All(cpu.GetCoreTemps(), t => Assert.Equal(0, t.Temp));
    }

    [Fact]
    public void Error_EnergyFileInvalidContent()
    {
        const string energyUjPath = "/sys/class/powercap/intel-rapl:0/energy_uj";

        _mockFileSystem.AddFile("/proc/cpuinfo", new MockFileData(TestData.CpuInfo()));
        _mockFileSystem.AddFile("/proc/stat", new MockFileData(TestData.Stat1()));
        _mockFileSystem.AddFile(energyUjPath, new MockFileData("not_a_number"));

        _mockHelper.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        var cpu = CreateCpu();

        cpu.Update();

        Assert.Equal(0.0, cpu.GetPowerDraw());
    }
}
