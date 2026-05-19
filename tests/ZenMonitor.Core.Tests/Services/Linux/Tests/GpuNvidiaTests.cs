// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

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
public class GpuNvidiaTests
{
    private readonly Mock<ILogger<GpuNvidia>> _mockLogger;
    private readonly Mock<IHelper> _mockHelper;

    public GpuNvidiaTests()
    {
        _mockLogger = new Mock<ILogger<GpuNvidia>>();
        _mockHelper = new Mock<IHelper>();
    }

    [Fact]
    public void Update_ReturnGpuInfoFromGpuNvidia()
    {
        // Arrange
        string output = "GeForce RTX 4090, 12, 6, 1024, 24576, 72, P0, 450.00";
        _mockHelper
            .Setup(r => r.Linux.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(0, output, string.Empty));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);

        // Act
        gpu.Update();

        // Assert
        Assert.Equal("GeForce RTX 4090", gpu.GetGpuName());
        Assert.Equal(12, gpu.GetUsageGpu());
        Assert.Equal(6, gpu.GetUsageMemory());
        Assert.Equal(1024.0, gpu.GetMemoryUsed());
        Assert.Equal(24576.0, gpu.GetMemoryTotal());
        Assert.Equal(72, gpu.GetTemperatureGpu());
        Assert.Equal("P0", gpu.GetPowerState());
        Assert.Equal(450.00, gpu.GetPowerDraw());
    }

    [Fact]
    public void Update_ThrowsInvalidOperationExceptionWhenNvidiaSmiFails()
    {
        // Arrange
        _mockHelper
            .Setup(r => r.Linux.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(1, string.Empty, "Failed to query GPU"));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => gpu.Update());
        Assert.Contains("nvidia-smi error", exception.Message);
    }
}
