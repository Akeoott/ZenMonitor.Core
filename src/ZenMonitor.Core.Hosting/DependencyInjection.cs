// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using ZenMonitor.Core.Hosting.Registration;

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

        gpuNotSupported = false;

        if (OperatingSystem.IsLinux())
        {
            LinuxRegistration.Register(services, out gpuNotSupported);
        }
        else
        {
            NullRegistration.Register(services);
        }

        return services;
    }
}
