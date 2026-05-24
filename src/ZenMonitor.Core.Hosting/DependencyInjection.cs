// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Hosting.Registration;
using ZenMonitor.Core.Services;

namespace ZenMonitor.Core.Hosting;

/// <summary>
/// Extension methods for auto-detecting and registering the correct
/// platform-specific ZenMonitor services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Detects the current OS and GPU vendor, then registers the appropriate
    /// hardware monitoring services into the DI container.
    /// </summary>
    public static IServiceCollection AddZenMonitor(this IServiceCollection services)
    {
        return AddZenMonitor(services, out _);
    }

    /// <summary>
    /// Detects the current OS and GPU vendor, then registers the appropriate
    /// hardware monitoring services into the DI container.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="gpuNotSupported">
    /// <c>true</c> if no supported GPU was detected and the Null GPU fallback was used.
    /// Callers can use this to show a warning to the user.
    /// </param>
    public static IServiceCollection AddZenMonitor(this IServiceCollection services, out bool gpuNotSupported)
    {
        // Common infrastructure, used by all platforms
        services.AddSingleton<IFileSystem, FileSystem>();
#pragma warning disable CA1416 // disable validation of platform compatibility
        services.AddSingleton<ILinux, Linux.ServiceAbstraction.Linux>();
        services.AddSingleton<IWindows, Windows.ServiceAbstraction.Windows>();
        services.AddSingleton<IServiceAbstraction, ServiceAbstraction>();
#pragma warning restore CA1416 // warning will appear if used elsewhere

        gpuNotSupported = false;

        if (OperatingSystem.IsLinux())
        {
            LinuxRegistration.Register(services, out gpuNotSupported);
        }
        else if (OperatingSystem.IsWindows())
        {
            WindowsRegistration.Register(services, out gpuNotSupported);
        }
        else
        {
            NullRegistration.Register(services);
        }

        services.AddSingleton<IHardwareMonitor, HardwareMonitor>();
        return services;
    }
}
