# ZenMonitor.Core

### Core library for ZenMonitor system telemetry

![Last Commit](https://img.shields.io/github/last-commit/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%23201a19&color=%23ffb4a2)
![Stars](https://img.shields.io/github/stars/Akeoott/ZenMonitor.Core?style=for-the-badge&labelColor=%231d1b16&color=%23e6c419)
![Repo Size](https://img.shields.io/github/repo-size/Akeoott/ZenMonitor.Core?style=for-the-badge&labelColor=%231a1b1f&color=%23a8c7ff)

[![GitHub License](https://img.shields.io/github/license/akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%23201a19&color=%23ffb4a2)](https://github.com/Akeoott/ZenMonitor.Core/blob/main/LICENSE)
[![Code Coverage](https://img.shields.io/codecov/c/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231d1b16)](https://codecov.io/gh/Akeoott/ZenMonitor.Core)
[![CodeFactor Grade](https://img.shields.io/codefactor/grade/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231a1b1f)](https://www.codefactor.io/repository/github/akeoott/ZenMonitor.Core)

Core hardware abstraction interfaces, models, and platform services powering the [ZenMonitor](https://github.com/Akeoott/ZenMonitor) system monitor.

> [!WARNING]
> This repository is a separate NuGet library extracted from the main [ZenMonitor](https://github.com/Akeoott/ZenMonitor) project.
>
> The structure is still being defined. Expect changes.

---

## Architecture

The library is split across four projects:

| Project | Description |
|---------|-------------|
| `ZenMonitor.Core` | Hardware abstraction interfaces (`ICpu`, `IDrive`, `IGpu`, `IMemory`, `INetwork`, `ISystem`), data models, and Null-object fallback services. |
| `ZenMonitor.Core.Hosting` | DI registration extensions (`AddZenMonitor()`) that auto-detect the OS and register the correct platform services. This is optional but recommended. |
| `ZenMonitor.Core.Linux` | Linux-specific platform implementations for all interfaces. |
| `ZenMonitor.Core.Windows` | Windows-specific platform implementations for all interfaces. |
| `ZenMonitor.Core.Tests` | xUnit test suite. |

### Interface Pattern

Each hardware component is defined as an interface in the `Abstractions` namespace, with a corresponding Null-object service in `Services` (used as fallback when no platform implementation is available). The Linux project provides real implementations, while Windows support is currently under developments.

`IHardwareMonitor` aggregates all interfaces together as properties, providing a single entry point for consumers to simplify usage.

### NuGet Packages

Four NuGet packages are built and published:
- `ZenMonitor.Core` — interfaces, models, Null services.
- `ZenMonitor.Core.Linux` — Linux platform services.
- `ZenMonitor.Core.Windows` — Windows platform services.
- `ZenMonitor.Core.Hosting` — DI registration helpers.

Use `using ZenMonitor.Core.Hosting;` for initialization and DI,<br>
and use `using ZenMonitor.Core;` for using core components of this package.

---

## Status

This project is in early development. The API surface and project layout are not yet stable.

Key milestones being worked on:
- [x] Define hardware abstraction interfaces
- [ ] Implement main platform services
  - [ ] Linux (Currentlz missing network implementation)
  - [ ] Windows (Only CPU service is implemented)
- [x] Publish initial NuGet packages

---

## Getting Started

```bash
# Add the packages to your project
dotnet add package ZenMonitor.Core # Main components
dotnet add package ZenMonitor.Core.Hosting # Init ZenMonitor (Is optional and requires Dependency injection)
```

```cs
// Optional: Register all services using dependency injection (auto-detects OS)
var services = new ServiceCollection();
services.AddZenMonitor();
// You can also manually register the services you need.
// You could also skip dependency injection entirely, tho its not recommended.
```

---

## License

LGPL-3.0 — see the [LICENSE](LICENSE) file for details.