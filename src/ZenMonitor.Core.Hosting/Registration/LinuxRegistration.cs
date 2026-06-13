// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

#if PLATFORM_LINUX

using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Linux.ServiceAbstraction;
using ZenMonitor.Core.Linux.Services;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Services;

namespace ZenMonitor.Core.Hosting.Registration;

/// <summary>
/// Registers all Linux-specific ZenMonitor services.
/// </summary>
[SupportedOSPlatform("linux")]
internal static class LinuxRegistration
{
    internal static void Register(IServiceCollection services)
    {
        // If the consumer hasn't configured logging via AddLogging(), provide
        // null logger singletons so ILogger<T> constructor params don't crash
        // during DI resolution. When the consumer has configured logging, we
        // skip this block so their open-generic ILogger<T> resolver takes full effect.
        if (!DependencyInjection.HasLogging(services))
        {
            services.AddSingleton<ILogger<Cpu>>(NullLogger<Cpu>.Instance);
            services.AddSingleton<ILogger<Drive>>(NullLogger<Drive>.Instance);
            services.AddSingleton<ILogger<Memory>>(NullLogger<Memory>.Instance);
            services.AddSingleton<ILogger<Network>>(NullLogger<Network>.Instance);
            services.AddSingleton<ILogger<Linux.Services.System>>(NullLogger<Linux.Services.System>.Instance);
            services.AddSingleton<ILogger<GpuAmd>>(NullLogger<GpuAmd>.Instance);
            services.AddSingleton<ILogger<GpuNvidia>>(NullLogger<GpuNvidia>.Instance);
        }

        services.AddSingleton<IAbstractionsLinux, AbstractionsLinux>();
        services.AddSingleton<ICpu, Cpu>();
        services.AddSingleton<IDrive, Drive>();
        services.AddSingleton<IMemory, Memory>();
        services.AddSingleton<INetwork, Network>();
        services.AddSingleton<ISystem, Linux.Services.System>();

        var vendor = DetectGpuVendor();

        services.AddSingleton<IGpu>(serviceProvider =>
        {
            return vendor switch
            {
                GpuVendor.Nvidia => ActivatorUtilities.CreateInstance<GpuNvidia>(serviceProvider),
                GpuVendor.Amd => ActivatorUtilities.CreateInstance<GpuAmd>(serviceProvider),
                _ => new NullGpu(),
            };
        });
    }

    private static GpuVendor DetectGpuVendor()
    {
        try
        {
            if (Directory.Exists("/proc/driver/nvidia") ||
                File.Exists("/usr/bin/nvidia-smi"))
            {
                return GpuVendor.Nvidia;
            }

            if (Directory.Exists("/sys/class/drm"))
            {
                var cards = Directory.GetDirectories("/sys/class/drm", "card*");
                foreach (var card in cards)
                {
                    var deviceDir = Path.Combine(card, "device");
                    if (!Directory.Exists(deviceDir))
                        continue;

                    var vendorFile = Path.Combine(deviceDir, "vendor");
                    if (!File.Exists(vendorFile))
                        continue;

                    var vendorId = File.ReadAllText(vendorFile).Trim();

                    return vendorId switch
                    {
                        "0x1002" => GpuVendor.Amd,
                        "0x10de" => GpuVendor.Nvidia,
                        _ => GpuVendor.Unknown,
                    };
                }
            }
        }
        catch
        {
            // Ignore detection errors, fall back to NullGpu
        }
        return GpuVendor.Unknown;
    }
}

#endif
