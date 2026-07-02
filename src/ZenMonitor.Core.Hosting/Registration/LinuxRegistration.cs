// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

#if PLATFORM_LINUX

using System.IO;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions.Controller;
using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Linux.Services.Controller;
using ZenMonitor.Core.Linux.Services.Telemetry;
using ZenMonitor.Core.Linux.Utils;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Services;
using ZenMonitor.Core.Utils;

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
            services.AddSingleton<ILogger<UtilsLinux>>(NullLogger<UtilsLinux>.Instance);
            services.AddSingleton<ILogger<CpuTel>>(NullLogger<CpuTel>.Instance);
            services.AddSingleton<ILogger<DriveTel>>(NullLogger<DriveTel>.Instance);
            services.AddSingleton<ILogger<MemoryTel>>(NullLogger<MemoryTel>.Instance);
            services.AddSingleton<ILogger<NetworkTel>>(NullLogger<NetworkTel>.Instance);
            services.AddSingleton<ILogger<ProcessTel>>(NullLogger<ProcessTel>.Instance);
            services.AddSingleton<ILogger<SystemTel>>(NullLogger<SystemTel>.Instance);

            services.AddSingleton<ILogger<GpuTelAmd>>(NullLogger<GpuTelAmd>.Instance);
            services.AddSingleton<ILogger<GpuTelNvidia>>(NullLogger<GpuTelNvidia>.Instance);
        }

        services.AddSingleton<IUtilsLinux, UtilsLinux>();

        services.AddSingleton<IProcessCon, ProcessCon>();

        services.AddSingleton<ICpuTel, CpuTel>();
        services.AddSingleton<IDriveTel, DriveTel>();
        services.AddSingleton<IMemoryTel, MemoryTel>();
        services.AddSingleton<INetworkTel, NetworkTel>();
        services.AddSingleton<IProcessTel, ProcessTel>();
        services.AddSingleton<ISystemTel, SystemTel>();

        var vendor = DetectGpuVendor();

        services.AddSingleton<IGpuTel>(serviceProvider =>
        {
            return vendor switch
            {
                GpuVendor.Nvidia => ActivatorUtilities.CreateInstance<GpuTelNvidia>(serviceProvider),
                GpuVendor.Amd => ActivatorUtilities.CreateInstance<GpuTelAmd>(serviceProvider),
                _ => new NullGpuTel()
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
                        _ => GpuVendor.Unknown
                    };
                }
            }
        }
        catch
        {
            // Ignore detection errors, fall back to NullGpuTel
        }
        return GpuVendor.Unknown;
    }
}

#endif
