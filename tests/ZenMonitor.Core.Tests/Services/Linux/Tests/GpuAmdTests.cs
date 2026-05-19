// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class GpuAmdTests
{
    private readonly Mock<ILogger<GpuAmd>> _mockLogger;

    public GpuAmdTests()
    {
        _mockLogger = new Mock<ILogger<GpuAmd>>();
    }

    /// <summary>
    /// GpuAmd is not implemented, as long as this is so, it must return empty strings
    /// </summary>
    [Fact]
    public void Update_ReturnEmpryStringsFromGpuAmd()
    {
        // Arrange
        var gpu = new GpuAmd(_mockLogger.Object);

        // Act
        gpu.Update();

        // Assert
        Assert.Equal("", gpu.GetGpuName());
        Assert.Equal(0, gpu.GetUsageGpu());
        Assert.Equal(0, gpu.GetUsageMemory());
        Assert.Equal(0.0, gpu.GetMemoryUsed());
        Assert.Equal(0.0, gpu.GetMemoryTotal());
        Assert.Equal(0, gpu.GetTemperatureGpu());
        Assert.Equal("", gpu.GetPowerState());
        Assert.Equal(0.0, gpu.GetPowerDraw());
    }
}
