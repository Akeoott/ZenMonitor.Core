// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Debug;

public class Monitor(ILogger<Monitor> logger, IHardwareMonitor monitor)
{
    private readonly ILogger<Monitor> _logger = logger;
    private readonly IHardwareMonitor _monitor = monitor;

    private readonly SemaphoreSlim _dataReadyEvent = new(0, int.MaxValue);

    public async Task InitMonitor(int loopDelay, CancellationToken cts)
    {
        await Task.WhenAll(RunBackend(loopDelay, cts), RunDashboard(cts));
    }

    private async Task RunDashboard(CancellationToken cts)
    {
        while (true)
        {
            try
            {
                await _dataReadyEvent.WaitAsync(cts);

                Console.Write("\n\n========================DEBUG========================\n\n\n");

                Console.WriteLine("CPU INFORMATION");
                Console.WriteLine($"  Name: {_monitor.Cpu.GetCpuName()}");
                Console.Write($"  Speed (MHz): C0 {_monitor.Cpu.GetCpuSpeed()}");
                CpuCoreSpeed[] cpuCoreSpeed = _monitor.Cpu.GetCoreSpeeds();
                for (int i = 0; i < cpuCoreSpeed.Length; i++)
                {
                    CpuCoreSpeed speed = cpuCoreSpeed[i];
                    Console.Write($", C{speed.Index + 1} {speed.Speed}");
                }
                Console.WriteLine();

                Console.Write($"  Usage (%): C0 {_monitor.Cpu.GetCpuUsage()}");
                CpuCoreUsage[] cpuCoreUsage = _monitor.Cpu.GetCoreUsages();
                for (int i = 0; i < cpuCoreUsage.Length; i++)
                {
                    CpuCoreUsage? usage = cpuCoreUsage[i];
                    Console.Write($", C{usage.Index + 1} {usage.Usage}");
                }
                Console.WriteLine();

                Console.Write($"  Temperature (°C): C0 {_monitor.Cpu.GetCpuTemp()}");
                CpuCoreTemp[] cpuCoreTemp = _monitor.Cpu.GetCoreTemps();
                for (int i = 0; i < cpuCoreTemp.Length; i++)
                {
                    CpuCoreTemp? temp = cpuCoreTemp[i];
                    Console.Write($", C{temp.Index + 1} {temp.Temp}");
                }
                Console.WriteLine();
                Console.WriteLine($"  Power Draw (W): {_monitor.Cpu.GetPowerDraw()}\n");

                Console.WriteLine("DRIVE INFORMATION");
                var mountInfos = _monitor.Drive.GetMountInfos();
                foreach (var mount in mountInfos)
                {
                    double usagePercent = mount.TotalBytes > 0 ? (double)mount.UsedBytes / mount.TotalBytes * 100 : 0;
                    Console.WriteLine($"  {mount.MountPoint}: {mount.DeviceName} ({mount.FileSystem}) - {usagePercent:F1}% used ({mount.UsedBytes}/{mount.TotalBytes} bytes), IO: {mount.IOUsage:F1}%");
                }
                Console.WriteLine();

                Console.WriteLine("GPU INFORMATION");
                Console.WriteLine($"  Name: {_monitor.Gpu.GetGpuName()}");
                Console.WriteLine($"  GPU Usage (%): {_monitor.Gpu.GetUsageGpu()}");
                Console.WriteLine($"  Memory Usage (%): {_monitor.Gpu.GetUsageMemory()}");
                Console.WriteLine($"  Memory Used: {_monitor.Gpu.GetMemoryUsed()}");
                Console.WriteLine($"  Memory Total: {_monitor.Gpu.GetMemoryTotal()}");
                Console.WriteLine($"  Temperature (°C): {_monitor.Gpu.GetTemperatureGpu()}");
                Console.WriteLine($"  Power State: {_monitor.Gpu.GetPowerState()}");
                Console.WriteLine($"  Power Draw (W): {_monitor.Gpu.GetPowerDraw()}\n");

                Console.WriteLine("MEMORY INFORMATION");
                Console.WriteLine($"  Total: {_monitor.Memory.GetMemTotal()}");
                Console.WriteLine($"  Free: {_monitor.Memory.GetMemFree()}");
                Console.WriteLine($"  Available: {_monitor.Memory.GetMemAvailable()}");
                Console.WriteLine($"  Used: {_monitor.Memory.GetMemUsed()}");
                Console.WriteLine($"  Cached: {_monitor.Memory.GetCached()}");
                Console.WriteLine($"  Swap Total: {_monitor.Memory.GetSwapTotal()}");
                Console.WriteLine($"  Swap Free: {_monitor.Memory.GetSwapFree()}\n");

                Console.WriteLine("NETWORK INFORMATION");
                Console.WriteLine($"  Download in bytes: {_monitor.Network.GetDownloadSpeed()}");
                Console.WriteLine($"  Upload in bytes: {_monitor.Network.GetUploadSpeed()}");

                var networks = _monitor.Network.GetNetworks();
                foreach (var network in networks)
                {
                    Console.WriteLine($"  {network.Name} {network.IsUp}, Upload (Speed {network.UploadSpeed} / Total {network.TotalBytesUploaded}), Download (Speed {network.DownloadSpeed} / Total {network.TotalBytesDownloaded})");
                }
                Console.WriteLine();

                Console.WriteLine("SYSTEM INFORMATION");
                Console.WriteLine($"  Kernel: {_monitor.System.GetKernelVersion()}");
                Console.WriteLine($"  Hostname: {_monitor.System.GetHostname()}");
                Console.WriteLine($"  Uptime (s): {_monitor.System.GetUptimeSeconds()}");
                Console.WriteLine($"  Running Tasks: {_monitor.System.GetRunningTasks()}");
                Console.WriteLine($"  Total Tasks: {_monitor.System.GetTotalTasks()}\n");
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private Task RunBackend(int loopDelay, CancellationToken cts)
    {
        return Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                _monitor.Cpu.Update();
                _monitor.Drive.Update();
                _monitor.Gpu.Update();
                _monitor.Memory.Update();
                _monitor.Network.Update();
                _monitor.System.Update();
                _logger.LogTrace("Done! Sending event to update interface.");
                _dataReadyEvent.Release();
                await Task.Delay(loopDelay, cts);
            }
        }, cts);
    }
}
