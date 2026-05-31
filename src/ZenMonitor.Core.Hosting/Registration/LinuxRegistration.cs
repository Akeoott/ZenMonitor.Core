// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Linux.ServiceAbstraction;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Services;

namespace ZenMonitor.Core.Hosting.Registration;

/// <summary>
/// Registers all Linux-specific ZenMonitor services.
/// </summary>
[SupportedOSPlatform("linux")]
internal static class LinuxRegistration
{
    internal static void Register(IServiceCollection services, out bool gpuNotSupported)
    {
        services.AddSingleton<IAbstractionsLinux, AbstractionsLinux>();
        services.AddSingleton<ICpu, Linux.Services.Cpu>();
        services.AddSingleton<IDrive, Linux.Services.Drive>();
        services.AddSingleton<IMemory, Linux.Services.Memory>();
        services.AddSingleton<INetwork, Linux.Services.Network>();
        services.AddSingleton<ISystem, Linux.Services.System>();

        var vendor = DetectGpuVendor();
        gpuNotSupported = vendor == GpuVendor.Unknown;

        services.AddSingleton<IGpu>(serviceProvider =>
        {
            return vendor switch
            {
                GpuVendor.Nvidia => ActivatorUtilities.CreateInstance<Linux.Services.GpuNvidia>(serviceProvider),
                GpuVendor.Amd => ActivatorUtilities.CreateInstance<Linux.Services.GpuAmd>(serviceProvider),
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
