![ZenMonitor.Core](https://raw.githubusercontent.com/Akeoott/ZenMonitor.Core/refs/heads/main/assets/images/ZenMonitor.Core.svg)

**[ZenMonitor.Core on GitHub](https://github.com/Akeoott/ZenMonitor.Core/) |
[ZenMonitor.Core on NuGet](https://www.nuget.org/packages/ZenMonitor.Core/)**

# ZenMonitor.Core.Windows

The Windows library of ZenMonitor.Core provides
all concrete implementations and services specific to Windows.

This library is not meant to be used directly.
Use [ZenMonitor.Core.Hosting](https://github.com/Akeoott/ZenMonitor.Core/blob/main/src/ZenMonitor.Core.Hosting/README.md)
to automatically register all Windows services. Read its README for instructions.

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
[SupportedOSPlatform("windows")]
public class CpuTel(ILogger<CpuTel> logger, IUtilsWindows utils) : ICpuTel
{
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    ...
}
```

- ### Data Sources

  All Windows related data is retrieved via native Win32 API calls (P/Invoke), WMI and the Windows Registry.

  | Telemetry Service | Data Sources                                                                               |
  |-------------------|--------------------------------------------------------------------------------------------|
  | `CpuTel`          | Win32 API (P/Invoke), Windows Registry, WMI (`MSAcpi_ThermalZoneTemperature`)              |
  | `DriveTel`        | WIP                                                                                        |
  | `GpuTelNvidia`    | WIP                                                                                        |
  | `GpuTelAmd`       | WIP                                                                                        |
  | `MemoryTel`       | WIP                                                                                        |
  | `NetworkTel`      | WIP                                                                                        |
  | `ProcessTel`      | WIP                                                                                        |
  | `SystemTel`       | WIP                                                                                        |