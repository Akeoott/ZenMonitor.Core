// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class MemoryTests
{
    private readonly Mock<ILogger<Memory>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();

    private Memory CreateMemory() => new(_mockLogger.Object, _mockFileSystem);

    private void AddStdMemInfo() =>
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(TestData.MemInfo()));

    [Fact]
    public void Expected_AllMemoryMetrics()
    {
        AddStdMemInfo();

        var memory = CreateMemory();
        memory.Update();

        Assert.Equal(30.5, memory.GetMemTotal());
        Assert.Equal(1.73, memory.GetMemFree());
        Assert.Equal(16.81, memory.GetMemAvailable());
        Assert.Equal(13.69, memory.GetMemUsed());
        Assert.Equal(16.57, memory.GetCached());
        Assert.Equal(30.5, memory.GetSwapTotal());
        Assert.Equal(30.5, memory.GetSwapFree());
    }

    [Fact]
    public void Error_MissingMeminfo()
    {
        var memory = CreateMemory();
        memory.Update();

        Assert.Equal(0, memory.GetMemTotal());
        Assert.Equal(0, memory.GetMemFree());
        Assert.Equal(0, memory.GetMemAvailable());
        Assert.Equal(0, memory.GetMemUsed());
        Assert.Equal(0, memory.GetCached());
        Assert.Equal(0, memory.GetSwapTotal());
        Assert.Equal(0, memory.GetSwapFree());
    }

    [Fact]
    public void Error_MissingRequiredKeys()
    {
        // Provide meminfo but without MemTotal
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(
            "MemFree:       1813564 kB\n" +
            "MemAvailable:  17630756 kB\n" +
            "Cached:        17376204 kB\n" +
            "SwapTotal:     31985660 kB\n" +
            "SwapFree:      31985392 kB\n"
        ));

        var memory = CreateMemory();
        memory.Update();

        Assert.Equal(0, memory.GetMemTotal());
        Assert.Equal(0, memory.GetMemFree());
        Assert.Equal(0, memory.GetMemAvailable());
        Assert.Equal(0, memory.GetMemUsed());
        Assert.Equal(0, memory.GetCached());
        Assert.Equal(0, memory.GetSwapTotal());
        Assert.Equal(0, memory.GetSwapFree());
    }

    [Fact]
    public void Error_UnparseableValue()
    {
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(
            "MemTotal:      notanumber kB\n" +
            "MemFree:       1813564 kB\n" +
            "MemAvailable:  17630756 kB\n" +
            "Cached:        17376204 kB\n" +
            "SwapTotal:     31985660 kB\n" +
            "SwapFree:      31985392 kB\n"
        ));

        var memory = CreateMemory();
        memory.Update();

        Assert.Equal(0, memory.GetMemTotal());
    }
}
