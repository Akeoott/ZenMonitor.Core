// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Xunit;

using ZenMonitor.Core.Linux.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Utils;

// NOTE: These unit tests are there to confirm that `RunProcess` works

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class UtilsLinuxTests
{
    private readonly UtilsLinux _sut = new();

    [Fact]
    public void Expected_Run_Echo_ReturnsZeroExitCodeAndCapturesStdout()
    {
        var result = _sut.RunProcess("echo", "hello world");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("hello world", result.StandardOutput);
        Assert.Equal("", result.StandardError);
    }

    [Fact]
    public void Expected_Run_True_ReturnsZeroExitCode()
    {
        var result = _sut.RunProcess("true");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("", result.StandardOutput);
    }

    [Fact]
    public void Expected_Run_Whoami_ReturnsNonEmptyStdout()
    {
        var result = _sut.RunProcess("whoami");

        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal("", result.StandardError);
    }

    [Fact]
    public void Expected_Run_MultipleArguments()
    {
        var result = _sut.RunProcess("echo", "hello", "world");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("hello world", result.StandardOutput);
    }

    [Fact]
    public void Expected_Run_ProgramWithArguments()
    {
        var result = _sut.RunProcess("printf", "%s-%s", "a", "b");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("a-b", result.StandardOutput);
    }

    [Fact]
    public void Expected_Run_WithNoArguments()
    {
        var result = _sut.RunProcess("echo");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("", result.StandardOutput);
    }

    [Fact]
    public void Error_Run_False_ReturnsNonZeroExitCode()
    {
        var result = _sut.RunProcess("false");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("", result.StandardOutput);
    }

    [Fact]
    public void Error_Run_Cat_InvalidFile_StderrCaptured()
    {
        var result = _sut.RunProcess("cat", "/nonexistent_file_xyz");

        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal("", result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
    }
}
