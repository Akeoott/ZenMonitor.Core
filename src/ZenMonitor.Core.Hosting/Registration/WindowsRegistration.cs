// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

#if PLATFORM_WINDOWS

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Services;
using ZenMonitor.Core.Utils;
using ZenMonitor.Core.Windows.Services.Telemetry;
using ZenMonitor.Core.Windows.Utils;

namespace ZenMonitor.Core.Hosting.Registration;

/// <summary>
/// Registers all Windows-specific ZenMonitor services.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class WindowsRegistration
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
            services.AddSingleton<ILogger<Process>>(NullLogger<Process>.Instance);
            services.AddSingleton<ILogger<Windows.Services.Telemetry.System>>(NullLogger<Windows.Services.Telemetry.System>.Instance);

            services.AddSingleton<ILogger<GpuNvidia>>(NullLogger<GpuNvidia>.Instance);
            services.AddSingleton<ILogger<GpuAmd>>(NullLogger<GpuAmd>.Instance);
        }

        services.AddSingleton<IUtilsWindows, UtilsWindows>();
        services.AddSingleton<IRawCpuTelemetry, UtilsWindows.RawCpuTelemetry>();

        services.AddSingleton<ICpu, Cpu>();
        services.AddSingleton<IDrive, Drive>();
        services.AddSingleton<IMemory, Memory>();
        services.AddSingleton<INetwork, Network>();
        services.AddSingleton<IProcess, Process>();
        services.AddSingleton<ISystem, Windows.Services.Telemetry.System>();

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
            const string gpuClass = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
            using var gpuKey = Registry.LocalMachine.OpenSubKey(gpuClass);
            if (gpuKey == null)
                return GpuVendor.Unknown;

            foreach (var subKeyName in gpuKey.GetSubKeyNames())
            {
                using var subKey = gpuKey.OpenSubKey(subKeyName);
                if (subKey == null)
                    continue;

                var provider = subKey.GetValue("ProviderName") as string;
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

#endif
