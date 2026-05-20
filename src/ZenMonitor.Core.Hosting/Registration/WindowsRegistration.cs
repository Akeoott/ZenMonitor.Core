// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Services;
using ZenMonitor.Core.Windows.Services;

namespace ZenMonitor.Core.Hosting.Registration;

/// <summary>
/// Registers all Windows-specific ZenMonitor services,
/// including GPU vendor auto-detection.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class WindowsRegistration
{
    /// <summary>
    /// Registers Windows hardware monitoring services into the DI container.
    /// Automatically detects NVIDIA vs AMD GPU to select the correct implementation.
    /// </summary>
    public static void Register(IServiceCollection services)
    {
        Register(services, out _);
    }

    /// <summary>
    /// Registers Windows hardware monitoring services into the DI container.
    /// Automatically detects NVIDIA vs AMD GPU to select the correct implementation.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="gpuNotSupported">
    /// <c>true</c> if GPU vendor could not be detected and NullGpu was used.
    /// </param>
    public static void Register(IServiceCollection services, out bool gpuNotSupported)
    {
        // Infrastructure
        services.AddSingleton<IServiceAbstraction, Helper>();

        // Platform-level services
        services.AddSingleton<ICpu, Cpu>();
        services.AddSingleton<IDrive, Drive>();
        services.AddSingleton<IMemory, Memory>();
        services.AddSingleton<INetwork, Network>();
        services.AddSingleton<ISystem, Windows.Services.System>();

        var vendor = DetectWindowsGpuVendor();
        gpuNotSupported = vendor == GpuVendor.Unknown;

        services.AddSingleton<IGpu>(serviceProvider =>
        {
            return vendor switch
            {
                GpuVendor.Nvidia => ActivatorUtilities.CreateInstance<GpuNvidia>(serviceProvider),
                GpuVendor.Amd => ActivatorUtilities.CreateInstance<GpuAmd>(serviceProvider),
                _ => new NullGpu(),
            };
        });

        services.AddSingleton<IHardwareMonitor, HardwareMonitor>();
    }

    /// <summary>
    /// Detects the GPU vendor on Windows by reading /sys/class/drm or /proc/driver/nvidia.
    /// Returns the most specific <see cref="GpuVendor"/> value, or <see cref="GpuVendor.Unknown"/>.
    /// </summary>
    private static GpuVendor DetectWindowsGpuVendor()
    {
        try
        {

        }
        catch
        {
            // Ignore detection errors, fall back to NullGpu
        }

        return GpuVendor.Unknown;
    }
}
