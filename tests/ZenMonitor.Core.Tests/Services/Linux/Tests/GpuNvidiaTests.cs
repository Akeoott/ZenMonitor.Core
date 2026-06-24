// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class GpuNvidiaTests
{
    private readonly Mock<ILogger<GpuNvidia>> _mockLogger = new();
    private readonly Mock<IUtilsLinux> _mockHelper = new();

    [Fact]
    public void Expected_ReturnsParsedValues()
    {
        const string output = "GeForce RTX 4090, 12, 6, 1024, 24576, 72, P0, 450.00";
        _mockHelper
            .Setup(r => r.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(0, output, string.Empty));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);
        gpu.Update();

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
    public void Error_NvidiaSmiFails_ReturnsEmptySnapshot()
    {
        _mockHelper
            .Setup(r => r.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(1, string.Empty, "Failed to query GPU"));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);
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
    public void Error_MalformedCsvOutput_ReturnsEmptySnapshot()
    {
        // nvidia-smi returns output with only 2 fields — part[7] access would throw IndexOutOfRangeException
        _mockHelper
            .Setup(r => r.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(0, "NVIDIA GPU, 50", string.Empty));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);
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
    public void Error_NoCsvOutput_ReturnsEmptySnapshot()
    {
        // nvidia-smi returns an empty string — hits the < 8 guard
        _mockHelper
            .Setup(r => r.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(0, "", string.Empty));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);
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
    public void Edge_UnparseableNumericFields_FallsBackToZero()
    {
        // 8 CSV fields but numeric fields are garbage — exercises TryParse fallback to 0
        _mockHelper
            .Setup(r => r.RunProcess("nvidia-smi", It.IsAny<string>()))
            .Returns(new ProcessResult(0, "My GPU, abc, def, ghi, jkl, mno, P8, xyz", string.Empty));

        var gpu = new GpuNvidia(_mockLogger.Object, _mockHelper.Object);
        gpu.Update();

        Assert.Equal("My GPU", gpu.GetGpuName());
        Assert.Equal(0, gpu.GetUsageGpu());
        Assert.Equal(0, gpu.GetUsageMemory());
        Assert.Equal(0.0, gpu.GetMemoryUsed());
        Assert.Equal(0.0, gpu.GetMemoryTotal());
        Assert.Equal(0, gpu.GetTemperatureGpu());
        Assert.Equal("P8", gpu.GetPowerState());
        Assert.Equal(0.0, gpu.GetPowerDraw());
    }
}
