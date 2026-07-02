// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services.Telemetry;

namespace ZenMonitor.Core.Tests.Services.Linux.Telemetry;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class GpuTelAmdTests
{
    private readonly Mock<ILogger<GpuTelAmd>> _mockLogger = new();

    /// <summary>
    /// GpuTelAmd is not implemented, so it must return empty strings and zeros.
    /// </summary>
    [Fact]
    public void Expected_ReturnsEmptySnapshot()
    {
        var gpu = new GpuTelAmd(_mockLogger.Object);

        gpu.Update();

        Assert.Equal("", gpu.GetGpuName());
        Assert.Equal(0, gpu.GetUsageGpu());
        Assert.Equal(0, gpu.GetUsageMemory());
        Assert.Equal(0.0, gpu.GetMemoryUsed());
        Assert.Equal(0.0, gpu.GetMemoryTotal());
        Assert.Equal(0, gpu.GetTemperatureGpu());
        Assert.Equal("", gpu.GetPowerState());
        Assert.Equal(0.0, gpu.GetPowerDraw());
    }

    [Fact]
    public void Expected_DoesNotThrow()
    {
        var gpu = new GpuTelAmd(_mockLogger.Object);

        var exception = Record.Exception(gpu.Update);
        Assert.Null(exception);
    }
}
