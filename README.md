![ZenMonitor.Core](https://raw.githubusercontent.com/Akeoott/ZenMonitor.Core/refs/heads/main/assets/images/ZenMonitor.Core.svg)

**[ZenMonitor.Core on GitHub](https://github.com/Akeoott/ZenMonitor.Core/) |
[ZenMonitor.Core on NuGet](https://www.nuget.org/packages/ZenMonitor.Core/)**

# ZenMonitor.Core

[![CodeFactor Grade](https://img.shields.io/codefactor/grade/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231a1b1f)](https://www.codefactor.io/repository/github/akeoott/ZenMonitor.Core)
[![Code Coverage](https://img.shields.io/codecov/c/github/Akeoott/ZenMonitor.Core?style=for-the-badge&logoSize=auto&labelColor=%231d1b16)](https://codecov.io/gh/Akeoott/ZenMonitor.Core)
[![Nuget Version](https://img.shields.io/nuget/vpre/ZenMonitor.Core?style=for-the-badge&logo=nuget&label=ZenMonitor.Core&labelColor=%231a1b1f&color=%23a8c7ff)](https://www.nuget.org/packages/ZenMonitor.Core/)

### Core library for ZenMonitor system telemetry

Core hardware abstraction interfaces, models, and platform services
powering the [ZenMonitor](https://github.com/Akeoott/ZenMonitor) project.

> [!WARNING]
> This repository is a separate NuGet library extracted from
> the original [ZenMonitor](https://github.com/Akeoott/ZenMonitor) project.
>
> Only fully supports Linux at the moment. Windows support is WIP.
> The structure is still being defined. Expect changes.

---

## Usage

**Add the dependencies to your project:**
```bash
dotnet package add ZenMonitor.Core
dotnet package add ZenMonitor.Core.Hosting
```

**Then add the dependencies to your service provider:**
```cs
using Microsoft.Extensions.DependencyInjection;
using ZenMonitor.Core.Hosting;

// Create a service provider, anything based on using DI works.
var services = new ServiceCollection();

// Then inject ZenMonitor
services.AddZenMonitor();

// You can also manually register the services you need.
// Skipping dependency injection is also possible but not recommended.
```

> [!IMPORTANT]
> To optionally enable logging for the library,
> inject a logging service BEFORE injecting ZenMonitor.
>
> Example:
> ```cs
> // Before adding ZenMonitor
> services.AddLogging(builder =>
> {
>     builder.ClearProviders();
>     builder.AddSerilog();
> });
>
> // Then add ZenMonitor
> services.AddZenMonitor();
> ```

### Technical Details

- **License**: [LGPL-3.0](LICENSE)
- **Stack**: C# 100%, built using .NET 10.0.203
- **Platform Support**: Linux (Windows is currently WIP)
- **Dependencies**: All listed in [Directory.Packages.props](Directory.Packages.props)
- **AOT Compilation**: Supported (Not heavily tested)

---

## Project Overview

- ### Interfaces

  Each service is defined as an interface in `Abstractions.*` and it's sub namespaces.
  The Linux project provides real implementations, while Windows support is under development.

  - ### `ITelemetryAggregate`:
    aggregates all telemetry interfaces as properties,
    providing a single entry point for consumers to simplify usage.

    | ITelemetryAggregate | Provides                               |
    |---------------------|----------------------------------------|
    | `UpdateAll`         | Updates all records in all interfaces. |
    | `ICpu`              | CPU usage, temperature, frequency etc. |
    | `IDrive`            | Disk I/O, partition usage etc.         |
    | `IGpu`              | GPU utilization, VRAM etc.             |
    | `IMemory`           | RAM usage, swap etc.                   |
    | `INetwork`          | Network throughput, interfaces etc.    |
    | `IProcess`          | Details of processes etc.              |
    | `ISystem`           | OS info, uptime, hostname etc.         |

  - ### `IControllerAggregate` (WIP):
    aggregates all controller interfaces as properties,
    providing a single entry point for consumers to simplify usage.

    | IControllerAggregate  | Provides                                    |
    |--------------------|---------------------------------------------|
    | IProcessController | Running, terminating and killing processes. |

    More will be added in the future!


- ### Project Structure

  The repo is split across six projects:

  | Project                   | Descriptions                                                                                                                                         |
  |---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
  | `ZenMonitor.Core`         | Interfaces for consumers, data models and utils for services.                                                                                        |
  | `ZenMonitor.Core.Hosting` | DI registration extensions (`AddZenMonitor()`) that auto-detect the OS and register the correct platform services. This is optional but recommended. |
  | `ZenMonitor.Core.Linux`   | Linux-specific platform implementations for all interfaces.                                                                                          |
  | `ZenMonitor.Core.Windows` | Windows-specific platform implementations for all interfaces.                                                                                        |
  | `ZenMonitor.Core.Debug`   | Quick debugging interface providing all info out of the box to the terminal with logging.                                                            |
  | `ZenMonitor.Core.Tests`   | xUnit test suite.                                                                                                                                    |

- ### NuGet Packages

  Four NuGet packages are built and published:
  - `ZenMonitor.Core` — interfaces, models, utils for services.
  - `ZenMonitor.Core.Hosting` — DI registration helpers.
  - `ZenMonitor.Core.Linux` — Linux platform services.
  - `ZenMonitor.Core.Windows` — Windows platform services.

  Use `using ZenMonitor.Core.Hosting;` for initialization and DI, and `using ZenMonitor.Core;` for core components.

---

## Contributing

Please read our [CONTRIBUTING.md](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CONTRIBUTING.md) and [Code of Conduct](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CODE_OF_CONDUCT.md)
for comunity guidelines, code style, commit conventions, and pull requests.
