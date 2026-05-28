# ZenMonitor.Core

### Core library for ZenMonitor system telemetry

![Last Commit](https://img.shields.io/github/last-commit/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%23201a19&color=%23ffb4a2)
![Stars](https://img.shields.io/github/stars/Akeoott/ZenMonitor.Core?style=for-the-badge&labelColor=%231d1b16&color=%23e6c419)
![Repo Size](https://img.shields.io/github/repo-size/Akeoott/ZenMonitor.Core?style=for-the-badge&labelColor=%231a1b1f&color=%23a8c7ff)

[![GitHub License](https://img.shields.io/github/license/akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%23201a19&color=%23ffb4a2)](https://github.com/Akeoott/ZenMonitor.Core/blob/main/LICENSE)
[![Code Coverage](https://img.shields.io/codecov/c/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231d1b16)](https://codecov.io/gh/Akeoott/ZenMonitor.Core)
[![CodeFactor Grade](https://img.shields.io/codefactor/grade/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231a1b1f)](https://www.codefactor.io/repository/github/akeoott/ZenMonitor.Core)

### A light and fast system monitor

[![Nuget Version](https://img.shields.io/nuget/vpre/ZenMonitor.Core?style=for-the-badge&logo=nuget&label=ZenMonitor.Core&labelColor=%231a1b1f&color=%23a8c7ff)](https://www.nuget.org/packages/ZenMonitor.Core/)

Core hardware abstraction interfaces, models, and platform services powering the [ZenMonitor](https://github.com/Akeoott/ZenMonitor) system monitor.

> [!WARNING]
> This repository is a separate NuGet library extracted from the main [ZenMonitor](https://github.com/Akeoott/ZenMonitor) project.
>
> Only fully supports Linux at the moment. Windows support is WIP. The structure is still being defined. Expect changes.

---

## Quick Start

### Using the package:

```bash
# Add the packages to your project
dotnet add package ZenMonitor.Core # Main components
dotnet add package ZenMonitor.Core.Hosting # Init ZenMonitor.Core (optional, requires Dependency Injection)
```

```cs
using Microsoft.Extensions.DependencyInjection;
using ZenMonitor.Core.Hosting;

// Optional: Register all services using dependency injection (auto-detects OS)
var services = new ServiceCollection();
services.AddZenMonitor();
// You can also manually register the services you need.
// Skipping dependency injection is possible but not recommended.
```

> [!IMPORTANT]
> See [CONTRIBUTING.md](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CONTRIBUTING.md) for the contribution workflow and our [Code of Conduct](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CODE_OF_CONDUCT.md).

## Technical Details

- **Stack**: C# 100%, .NET 10.0.203
- **Platform**: Linux (Windows is currently WIP)
- **License**: [LGPL-3.0](LICENSE)

---

## Project Documentation

### Hardware Interfaces

Each hardware component is defined as an interface in the `Abstractions` namespace, with a corresponding Null-object service in `Services` (used as fallback when no platform implementation is available). The Linux project provides real implementations, while Windows support is under development.

`IHardwareMonitor` aggregates all interfaces as properties, providing a single entry point for consumers to simplify usage.

Each interface exposes a `void Update()` method plus typed getters:

| Interface | Provides |
|-----------|----------|
| `ICpu` | CPU usage, temperature, frequency etc. |
| `IDrive` | Disk I/O, partition usage etc. |
| `IGpu` | GPU utilization, VRAM etc. |
| `IMemory` | RAM usage, swap etc. |
| `INetwork` | Network throughput, interfaces etc. |
| `ISystem` | OS info, uptime, hostname etc. |

### Project Structure

The repo is split across five projects:

| Project | DescriptioSn |
|---------|-------------|
| `ZenMonitor.Core` | Hardware abstraction interfaces (`ICpu`, `IDrive`, `IGpu`, `IMemory`, `INetwork`, `ISystem`), data models, and Null-object fallback services. |
| `ZenMonitor.Core.Hosting` | DI registration extensions (`AddZenMonitor.Core()`) that auto-detect the OS and register the correct platform services. This is optional but recommended. |
| `ZenMonitor.Core.Linux` | Linux-specific platform implementations for all interfaces. |
| `ZenMonitor.Core.Windows` | Windows-specific platform implementations for all interfaces. |
| `ZenMonitor.Core.Debug` | Quick debugging interface providing all info out of the box to the terminal with logging. See [CONTRIBUTING.md](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CONTRIBUTING.md) for usage details. |
| `ZenMonitor.Core.Tests` | xUnit test suite. |

### NuGet Packages

Four NuGet packages are built and published:
- `ZenMonitor.Core` — interfaces, models, Null services.
- `ZenMonitor.Core.Hosting` — DI registration helpers.
- `ZenMonitor.Core.Linux` — Linux platform services.
- `ZenMonitor.Core.Windows` — Windows platform services.

Use `using ZenMonitor.Core.Hosting;` for initialization and DI, and `using ZenMonitor.Core;` for core components.

---

## Contributing

Please read [CONTRIBUTING.md](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CONTRIBUTING.md) for guidelines on code style, commit conventions, and pull requests.
