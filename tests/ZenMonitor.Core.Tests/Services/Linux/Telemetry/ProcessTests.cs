// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Tests.Services.Linux.Telemetry;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class ProcessTests
{
    private readonly Mock<ILogger<Process>> _mockLogger = new();
    private readonly MockFileSystem _mockFileSystem = new();
    private readonly Mock<IUtilsLinux> _mockUtils = new();

    private const int MockProcessorCount = 4;

    private Process CreateProcess() => new(_mockLogger.Object, _mockFileSystem, _mockUtils.Object);

    [Fact]
    public void Expected_ParsesProcessMetadata()
    {
        // Single process /proc/1234 with all files present
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0--flag\0value"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(1, process.GetTotalProcesses());

        var processes = process.GetProcesses().ToArray();
        Assert.Single(processes);

        var p = processes[0];
        Assert.Equal(1234, p.Pid);
        Assert.Equal("test-program", p.Program);
        Assert.Equal("test-program --flag value", p.Command);
        Assert.Equal("testuser", p.User);
        Assert.Equal(ProcessState.Running, p.State);
        Assert.Equal(5, p.Threads);
        Assert.Equal(10, p.MemoryUsage);
    }

    [Fact]
    public void Expected_CpuUsageDelta()
    {
        // First call caches baseline → CPU = 0
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0--flag\0value"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(0, process.GetProcesses().ToArray()[0].CpuUsage);

        // Second call with delta — utime+1, stime unchanged over 1 second
        // deltaCpu = 1, deltaTime = 1, ProcessorCount = 4
        // result = (1/1) / 4 * 100 = 25
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample2()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 2));

        process.Update();

        Assert.Equal(25.0, process.GetProcesses().ToArray()[0].CpuUsage);
    }

    [Fact]
    public void Expected_MultipleProcesses()
    {
        // Two processes: PID 1234 and PID 5678
        const string proc1234 = "/proc/1234";
        const string proc5678 = "/proc/5678";
        _mockFileSystem.AddDirectory(proc1234);
        _mockFileSystem.AddDirectory(proc5678);

        _mockFileSystem.AddFile(Path.Combine(proc1234, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(proc1234, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(proc1234, "cmdline"), new MockFileData("proc-one\0"));

        _mockFileSystem.AddFile(Path.Combine(proc5678, "status"), new MockFileData(
            "Name:\tproc-two\n" +
            "State:\tS (sleeping)\n" +
            "Tgid:\t5678\n" +
            "Pid:\t5678\n" +
            "Threads:\t3\n" +
            "Uid:\t0\t0\t0\t0\n" +
            "VmRSS:\t2048 kB\n"
        ));
        _mockFileSystem.AddFile(Path.Combine(proc5678, "stat"), new MockFileData(
            "5678 (proc-two) S 1 1 1 0 -1 4194304 0 0 0 0 0 0 0 0 20 0 3 0 0 0 2048 0 0 0 0 0 0 0 0 0"
        ));
        _mockFileSystem.AddFile(Path.Combine(proc5678, "cmdline"), new MockFileData("/usr/bin/proc-two"));

        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();
        var processes = process.GetProcesses().ToArray();

        Assert.Equal(2, process.GetTotalProcesses());
        Assert.Equal(2, processes.Length);

        // PID 1234
        var p1 = processes.Single(p => p.Pid == 1234);
        Assert.Equal("test-program", p1.Program);
        Assert.Equal("proc-one", p1.Command);
        Assert.Equal("testuser", p1.User);
        Assert.Equal(ProcessState.Running, p1.State);
        Assert.Equal(5, p1.Threads);
        Assert.Equal(10, p1.MemoryUsage);

        // PID 5678
        var p2 = processes.Single(p => p.Pid == 5678);
        Assert.Equal("proc-two", p2.Program);
        Assert.Equal("/usr/bin/proc-two", p2.Command);
        Assert.Equal("root", p2.User);
        Assert.Equal(ProcessState.Sleeping, p2.State);
        Assert.Equal(3, p2.Threads);
        Assert.Equal(2, p2.MemoryUsage);
    }

    [Fact]
    public void Expected_ProcessLifecycle_RemovesDeadProcessFromCache()
    {
        // First update includes PID 1234
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();
        Assert.Equal(1, process.GetTotalProcesses());

        // Second update — /proc/1234 is removed (no status file = skipped)
        // We simulate removal by removing the status file
        _mockFileSystem.RemoveFile(Path.Combine(procDir, "status"));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 2));

        process.Update();

        Assert.Equal(0, process.GetTotalProcesses());
        Assert.Empty(process.GetProcesses());
    }

    [Fact]
    public void Edge_NoCommandLine_FallsBackToDirName()
    {
        // cmdline is empty or nonexistent → program name falls back to PID directory name
        const string procDir = "/proc/9999";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(
            "Name:\t\n" + // empty name
            "State:\tR (running)\n" +
            "Pid:\t9999\n" +
            "Threads:\t1\n" +
            "Uid:\t0\t0\t0\t0\n" +
            "VmRSS:\t0 kB\n"
        ));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(
            "9999 (my-process) R 1 1 1 0 -1 4194304 0 0 0 0 0 0 0 0 20 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0"
        ));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        var p = process.GetProcesses().ToArray()[0];
        Assert.Equal("9999", p.Program); // Falls back to dir name
        Assert.Equal(string.Empty, p.Command); // No cmdline → empty
        Assert.Equal("root", p.User);
    }

    [Fact]
    public void Edge_CmdlineWithoutNullSeparators()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        // cmdline without null separators — just a plain string
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("firefox"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        var p = process.GetProcesses().ToArray()[0];
        Assert.Equal("firefox", p.Command);
    }

    [Fact]
    public void Edge_UnknownStateChar_DefaultsToUnknown()
    {
        const string procDir = "/proc/1111";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(
            "Name:\tweird\n" +
            "State:\tQ (weird)\n" + // Q is not a known state
            "Pid:\t1111\n" +
            "Threads:\t1\n" +
            "Uid:\t0\t0\t0\t0\n" +
            "VmRSS:\t0 kB\n"
        ));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(
            "1111 (weird) Q 1 1 1 0 -1 4194304 0 0 0 0 0 0 0 0 20 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0"
        ));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(ProcessState.Unknown, process.GetProcesses().ToArray()[0].State);
    }

    [Fact]
    public void Edge_NoProcDirectory_ReturnsEmpty()
    {
        // /proc does not exist in the mock filesystem
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(0, process.GetTotalProcesses());
        Assert.Empty(process.GetProcesses());
    }

    [Fact]
    public void Edge_NonNumericDirectoriesAreSkipped()
    {
        _mockFileSystem.AddDirectory("/proc/net");
        _mockFileSystem.AddDirectory("/proc/sys");
        _mockFileSystem.AddDirectory("/proc/1234");
        _mockFileSystem.AddFile(Path.Combine("/proc/1234", "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine("/proc/1234", "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine("/proc/1234", "cmdline"), new MockFileData("test-program\0"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(1, process.GetTotalProcesses()); // Only PID 1234, not /proc/net or /proc/sys
    }

    [Fact]
    public void Edge_UserMapRefreshesAfter10Minutes()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();
        Assert.Equal("testuser", process.GetProcesses().ToArray()[0].User);

        // Replace passwd with different mapping at T+11min (past the 10-min refresh threshold)
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(
            "newuser:x:1000:1000:New User:/home/newuser:/bin/bash\n"
        ));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 11, 0));

        process.Update();
        Assert.Equal("newuser", process.GetProcesses().ToArray()[0].User);
    }

    [Fact]
    public void Error_MissingStatusFile_SkipsProcess()
    {
        _mockFileSystem.AddDirectory("/proc/1234");
        // No status file added
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(0, process.GetTotalProcesses());
    }

    [Fact]
    public void Error_MissingPasswdFile_UserDefaultsToN_A()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0"));
        // No /etc/passwd file
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal("N/A", process.GetProcesses().ToArray()[0].User);
    }

    [Fact]
    public void Error_StatFileMalformed_ReturnsZeroCpu()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        // stat file with too few fields
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(
            "1234 (test-program) R 1 2 3 4"
        ));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        // Process is still added but CPU is 0
        Assert.Equal(1, process.GetTotalProcesses());
        Assert.Equal(0, process.GetProcesses().ToArray()[0].CpuUsage);
    }

    [Fact]
    public void Error_MissingStatFile_ReturnsZeroCpu()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData("test-program\0"));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        // No stat file
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        Assert.Equal(1, process.GetTotalProcesses());
        Assert.Equal(0, process.GetProcesses().ToArray()[0].CpuUsage);
    }

    [Fact]
    public void Error_EmptyCmdline_ReturnsEmptyString()
    {
        const string procDir = "/proc/1234";
        _mockFileSystem.AddDirectory(procDir);
        _mockFileSystem.AddFile(Path.Combine(procDir, "status"), new MockFileData(TestData.ProcStatusSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "stat"), new MockFileData(TestData.ProcStatSample1()));
        _mockFileSystem.AddFile(Path.Combine(procDir, "cmdline"), new MockFileData(""));
        _mockFileSystem.AddFile("/etc/passwd", new MockFileData(TestData.EtcPasswd()));
        _mockUtils.Setup(h => h.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 1));
        _mockUtils.Setup(h => h.ProcessorCount).Returns(MockProcessorCount);

        var process = CreateProcess();
        process.Update();

        var p = process.GetProcesses().ToArray()[0];
        Assert.Equal(string.Empty, p.Command);
    }
}
