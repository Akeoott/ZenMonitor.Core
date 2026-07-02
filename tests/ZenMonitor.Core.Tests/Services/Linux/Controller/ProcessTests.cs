// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services.Controller;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Models.Controller;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Controller;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class ProcessTests
{
    private readonly Mock<IUtilsLinux> _mockUtils = new();

    private Process CreateProcessController() => new(_mockUtils.Object);

    [Fact]
    public void Expected_Run_DelegatesToUtils()
    {
        const string programName = "echo";
        var arguments = new[] { "hello", "world" };
        var expectedResult = new ProcessResult(0, "hello world", "");
        _mockUtils
            .Setup(u => u.RunProcess(programName, arguments))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Run(programName, arguments);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess(programName, arguments), Times.Once);
    }

    [Fact]
    public void Expected_TerminateByName_CallsPkill()
    {
        const string processName = "firefox";
        var expectedResult = new ProcessResult(0, "", "");
        _mockUtils
            .Setup(u => u.RunProcess("pkill", processName))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Terminate(processName);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess("pkill", processName), Times.Once);
    }

    [Fact]
    public void Expected_TerminateById_CallsKill()
    {
        const int processId = 1234;
        var expectedResult = new ProcessResult(0, "", "");
        _mockUtils
            .Setup(u => u.RunProcess("kill", processId.ToString()))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Terminate(processId);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess("kill", processId.ToString()), Times.Once);
    }

    [Fact]
    public void Expected_KillByName_CallsPkill9()
    {
        const string processName = "firefox";
        var expectedResult = new ProcessResult(0, "", "");
        _mockUtils
            .Setup(u => u.RunProcess("pkill", "-9", processName))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Kill(processName);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess("pkill", "-9", processName), Times.Once);
    }

    [Fact]
    public void Expected_KillById_CallsKill9()
    {
        const int processId = 1234;
        var expectedResult = new ProcessResult(0, "", "");
        _mockUtils
            .Setup(u => u.RunProcess("kill", "-9", processId.ToString()))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Kill(processId);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess("kill", "-9", processId.ToString()), Times.Once);
    }

    [Fact]
    public void Edge_RunWithMultipleArguments()
    {
        var arguments = new[] { "-la", "/tmp", "/var" };
        var expectedResult = new ProcessResult(0, "some output", "");
        _mockUtils
            .Setup(u => u.RunProcess("ls", arguments))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Run("ls", arguments);

        Assert.Same(expectedResult, result);
        _mockUtils.Verify(u => u.RunProcess("ls", arguments), Times.Once);
    }

    [Fact]
    public void Error_Run_ProcessNotFound_ReturnsNonZeroExitCode()
    {
        var arguments = new[] { "/nonexistent" };
        var expectedResult = new ProcessResult(127, "", "command not found");
        _mockUtils
            .Setup(u => u.RunProcess("somecmd", arguments))
            .Returns(expectedResult);

        var controller = CreateProcessController();
        var result = controller.Run("somecmd", arguments);

        Assert.Equal(127, result.ExitCode);
        Assert.Equal("command not found", result.StandardError);
        _mockUtils.Verify(u => u.RunProcess("somecmd", arguments), Times.Once);
    }
}
