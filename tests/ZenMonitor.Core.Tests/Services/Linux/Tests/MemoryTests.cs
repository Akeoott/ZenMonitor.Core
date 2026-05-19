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
    private readonly Mock<ILogger<Memory>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;

    public MemoryTests()
    {
        _mockLogger = new Mock<ILogger<Memory>>();
        _mockFileSystem = new MockFileSystem();
    }

    private Memory CreateMemory() => new(_mockLogger.Object, _mockFileSystem);

    [Fact]
    public void Update_ParsesMemInfoCorrectly()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();

        memory.Update();

        Assert.NotEqual(0, memory.GetMemTotal());
    }

    [Fact]
    public void GetMemTotal_ReturnsMemTotal()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 30.5;
        Assert.Equal(expected, memory.GetMemTotal());
    }

    [Fact]
    public void GetMemFree_ReturnsMemFree()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 1.73;
        Assert.Equal(expected, memory.GetMemFree());
    }

    [Fact]
    public void GetMemAvailable_ReturnsMemAvailable()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 16.81;
        Assert.Equal(expected, memory.GetMemAvailable());
    }

    [Fact]
    public void GetMemUsed_ReturnsMemUsed()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 13.69;
        Assert.Equal(expected, memory.GetMemUsed());
    }

    [Fact]
    public void GetCached_ReturnsCached()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 16.57;
        Assert.Equal(expected, memory.GetCached());
    }

    [Fact]
    public void GetSwapTotal_ReturnsSwapTotal()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 30.5;
        Assert.Equal(expected, memory.GetSwapTotal());
    }

    [Fact]
    public void GetSwapFree_ReturnsSwapFree()
    {
        string meminfo = TestData.MemInfo();
        _mockFileSystem.AddFile("/proc/meminfo", new MockFileData(meminfo));

        var memory = CreateMemory();
        memory.Update();

        double expected = 30.5;
        Assert.Equal(expected, memory.GetSwapFree());
    }
}
