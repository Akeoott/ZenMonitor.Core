![ZenMonitor.Core](https://raw.githubusercontent.com/Akeoott/ZenMonitor.Core/refs/heads/main/assets/images/ZenMonitor.Core.svg)

**[ZenMonitor.Core on GitHub](https://github.com/Akeoott/ZenMonitor.Core/) |
[ZenMonitor.Core on NuGet](https://www.nuget.org/packages/ZenMonitor.Core/)**

# ZenMonitor.Core.Linux

The Linux library of ZenMonitor.Core provides
all concrete implementations and services specific to Linux.

This library is not meant to be used directly.
Use [ZenMonitor.Core.Hosting](https://github.com/Akeoott/ZenMonitor.Core/blob/main/src/ZenMonitor.Core.Hosting/README.md)
to automatically register all Linux services. Read its README for instructions.

---

## Documentation

> [!IMPORTANT]
> Major docs are all present in [ZenMonitor.Core](https://github.com/Akeoott/ZenMonitor.Core/blob/main/README.md).
> This includes data structures, interfaces, project status and more.
>
> It's highly recommended to read them first.

All services implement the interfaces in `ZenMonitor.Core.Abstractions`
and use its pre-defined records as data structures.

They also have dependencies which allow them to allow testing and logging with ease.

Example:
```cs
[SupportedOSPlatform("linux")]
public class CpuTel(ILogger<CpuTel> logger, IFileSystem fileSystem, IUtilsLinux utils) : ICpuTel
{
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    ...
}
```

- ### Data Sources

  All Linux related data is retrieved via reading virtual files in `/proc`, `/sys` and others or reading `stdout`/`stderr`.

  | Telemetry Service | Data Sources                                                                               |
  |-------------------|--------------------------------------------------------------------------------------------|
  | `CpuTel`          | `/proc/cpuinfo`, `/proc/stat`, `/sys/class/hwmon`, `/sys/class/powercap` (RAPL)            |
  | `DriveTel`        | `df -T -B1`, `/proc/diskstats`                                                             |
  | `GpuTelNvidia`    | `nvidia-smi` CLI tool                                                                      |
  | `GpuTelAmd`       | planned `/sys/class/drm/card*/device/hwmon`                                                |
  | `MemoryTel`       | `/proc/meminfo`                                                                            |
  | `NetworkTel`      | `/proc/net/dev`, `/sys/class/net/*/operstate`                                              |
  | `ProcessTel`      | `/proc/[pid]/status`, `/proc/[pid]/stat`, `/proc/[pid]/cmdline`, `/etc/passwd`             |
  | `SystemTel`       | `/proc/sys/kernel/osrelease`, `/proc/sys/kernel/hostname`, `/proc/uptime`, `/proc/loadavg` |

  | Controller Service  | Data Sources                        |
  | ------------------- |-------------------------------------|
  | `ProcessCon`        | `pkill`, `kill`, `renice` CLI tools |
