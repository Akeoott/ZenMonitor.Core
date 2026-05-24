// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

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
        services.AddSingleton<IServiceAbstraction, ServiceAbstraction>();

        // Platform-level services
        services.AddSingleton<ICpu, Cpu>();
        services.AddSingleton<IDrive, Drive>();
        services.AddSingleton<IMemory, Memory>();
        services.AddSingleton<INetwork, Network>();
        services.AddSingleton<ISystem, Windows.Services.System>();

        var vendor = DetectGpuVendor();
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
    }

    private static GpuVendor DetectGpuVendor()
    {
        try
        {
            const string gpuClass = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
            using var gpuKey = Registry.LocalMachine.OpenSubKey(gpuClass);
            if (gpuKey == null)
                return GpuVendor.Unknown;

            foreach (var subKeyName in gpuKey.GetSubKeyNames())
            {
                using var subKey = gpuKey.OpenSubKey(subKeyName);
                if (subKey == null)
                    continue;

                string? provider = subKey.GetValue("ProviderName") as string;
                if (string.IsNullOrEmpty(provider))
                    continue;

                if (provider.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                    return GpuVendor.Nvidia;

                if (provider.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
                    provider.Contains("Advanced Micro Devices", StringComparison.OrdinalIgnoreCase))
                    return GpuVendor.Amd;
            }
        }
        catch
        {
            // Ignore detection errors, fall back to NullGpu
        }

        return GpuVendor.Unknown;
    }
}
