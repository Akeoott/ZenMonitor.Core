// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

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
            services.AddSingleton<ILogger<CpuTel>>(NullLogger<CpuTel>.Instance);
            services.AddSingleton<ILogger<DriveTel>>(NullLogger<DriveTel>.Instance);
            services.AddSingleton<ILogger<MemoryTel>>(NullLogger<MemoryTel>.Instance);
            services.AddSingleton<ILogger<NetworkTel>>(NullLogger<NetworkTel>.Instance);
            services.AddSingleton<ILogger<ProcessTel>>(NullLogger<ProcessTel>.Instance);
            services.AddSingleton<ILogger<SystemTel>>(NullLogger<SystemTel>.Instance);

            services.AddSingleton<ILogger<GpuTelNvidia>>(NullLogger<GpuTelNvidia>.Instance);
            services.AddSingleton<ILogger<GpuTelAmd>>(NullLogger<GpuTelAmd>.Instance);
        }

        services.AddSingleton<IUtilsWindows, UtilsWindows>();
        services.AddSingleton<IRawCpuTel, UtilsWindows.RawCpuTel>();

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
                _ => new NullGpuTel(),
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
            // Ignore detection errors, fall back to NullGpuTel
        }
        return GpuVendor.Unknown;
    }
}
