// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Xunit;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Abstractions.Telemetry;

namespace ZenMonitor.Core.Tests.Abstractions;

public class SystemTelemetryTests
{
    [Fact]
    public void IHardwareMonitor_Interface_Is_Valid_Aggregator()
    {
        var type = typeof(ISystemTelemetry);
        Assert.True(type.IsInterface);
        Assert.NotNull(type.GetProperty("Cpu"));
        Assert.NotNull(type.GetProperty("Drive"));
        Assert.NotNull(type.GetProperty("Gpu"));
        Assert.NotNull(type.GetProperty("Memory"));
        Assert.NotNull(type.GetProperty("Network"));
        Assert.NotNull(type.GetProperty("System"));
    }

    [Fact]
    public void ICpu_Interface_Has_Expected_Members()
    {
        var type = typeof(ICpu);
        Assert.True(type.IsInterface);
        Assert.NotNull(type.GetMethod("GetCpuName"));
        Assert.NotNull(type.GetMethod("GetCpuSpeed"));
        Assert.NotNull(type.GetMethod("GetCpuUsage"));
        Assert.NotNull(type.GetMethod("GetCpuTemp"));
        Assert.NotNull(type.GetMethod("GetPowerDraw"));
        Assert.NotNull(type.GetMethod("GetCoreSpeeds"));
        Assert.NotNull(type.GetMethod("GetCoreUsages"));
        Assert.NotNull(type.GetMethod("GetCoreTemps"));
    }

    [Fact]
    public void IMemory_Interface_Has_Expected_Members()
    {
        var type = typeof(IMemory);
        Assert.NotNull(type.GetMethod("GetMemTotal"));
        Assert.NotNull(type.GetMethod("GetMemFree"));
        Assert.NotNull(type.GetMethod("GetMemAvailable"));
        Assert.NotNull(type.GetMethod("GetMemUsed"));
    }
}
