// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Abstractions;
using System.Linq;

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
    /// <param name="services">The service collection to register with.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Detects the current OS and GPU vendor, then registers the appropriate
        /// hardware monitoring services into the DI container.
        /// </summary>
        public IServiceCollection AddZenMonitor()
        {
            services.AddSingleton<IFileSystem, FileSystem>();

#if PLATFORM_LINUX
            if (OperatingSystem.IsLinux())
            {
                LinuxRegistration.Register(services);
            }
#elif PLATFORM_WINDOWS
            if (OperatingSystem.IsWindows())
            {
                WindowsRegistration.Register(services);
            }
#else
            NullRegistration.Register(services);
#endif
            services.AddSingleton<IHardwareMonitor, HardwareMonitor>();
            return services;
        }
    }

    internal static bool HasLogging(IServiceCollection services)
        => services.Any(d => d.ServiceType.Name == "ILoggerFactory");
}
