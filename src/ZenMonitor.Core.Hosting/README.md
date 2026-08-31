![ZenMonitor.Core](https://raw.githubusercontent.com/Akeoott/ZenMonitor.Core/refs/heads/main/assets/images/ZenMonitor.Core.svg)

**[ZenMonitor.Core on GitHub](https://github.com/Akeoott/ZenMonitor.Core/) |
[ZenMonitor.Core on NuGet](https://www.nuget.org/packages/ZenMonitor.Core/)**

# ZenMonitor.Core.Hosting

The hosting library of ZenMonitor.Core that provides
an easy access point for injecting ZenMonitor as a Dependency.

---

## Usage

The recommended usage is by using dependency injection.
The hosting namespace provides a single method which does all this for you.
Simply follow these steps.

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

---

## What's happening in the background?

**`services.AddZenMonitor();` works like this:**
- It checks if the platform is supported.
- Checks if logging is available.
- Injects all required services related to the detected platform.
- Makes hardware specific injections and support checks.

It's there to do all the checking and injecting for you.
One line adds everything this lib has to offer.
