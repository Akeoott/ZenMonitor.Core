// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Xunit;

namespace ZenMonitor.Core.Tests.Services.Null.Tests;

[Trait("Platform", "Linux")] // Only to define runner, does not reflect reality XP
public class CpuTests
{
    [Fact]
    public void GetAll_CheckThatEverythingIsNull()
    {
        Core.Services.NullCpu cpu = new();
        cpu.Update();
        Assert.Equal("", cpu.GetCpuName());
        Assert.Equal(0, cpu.GetCpuSpeed());
        Assert.Equal(0, cpu.GetCpuUsage());
        Assert.Equal(0, cpu.GetCpuTemp());
        Assert.Equal(0, cpu.GetPowerDraw());
        Assert.Equal([], cpu.GetCoreSpeeds());
        Assert.Equal([], cpu.GetCoreUsages());
        Assert.Equal([], cpu.GetCoreTemps());
    }
}
