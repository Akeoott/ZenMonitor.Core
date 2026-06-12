// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Debug;

internal sealed class Monitor(ILogger<Monitor> logger, IHardwareMonitor monitor)
{
    private readonly SemaphoreSlim _dataReadyEvent = new(0, int.MaxValue);

    internal async Task InitMonitor(int loopDelay, CancellationToken cts)
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
                Console.WriteLine($"  Name: {monitor.Cpu.GetCpuName()}");
                Console.Write($"  Speed (MHz): C0 {monitor.Cpu.GetCpuSpeed()}");
                var cpuCoreSpeed = monitor.Cpu.GetCoreSpeeds();
                foreach (var speed in cpuCoreSpeed)
                {
                    Console.Write($", C{speed.Index + 1} {speed.Speed}");
                }
                Console.WriteLine();

                Console.Write($"  Usage (%): C0 {monitor.Cpu.GetCpuUsage()}");
                var cpuCoreUsage = monitor.Cpu.GetCoreUsages();
                foreach (var usage in cpuCoreUsage)
                {
                    Console.Write($", C{usage.Index + 1} {usage.Usage}");
                }
                Console.WriteLine();

                Console.Write($"  Temperature (°C): C0 {monitor.Cpu.GetCpuTemp()}");
                var cpuCoreTemp = monitor.Cpu.GetCoreTemps();
                foreach (var temp in cpuCoreTemp)
                {
                    Console.Write($", C{temp.Index + 1} {temp.Temp}");
                }
                Console.WriteLine();
                Console.WriteLine($"  Power Draw (W): {monitor.Cpu.GetPowerDraw()}\n");

                Console.WriteLine("DRIVE INFORMATION");
                var mountInfos = monitor.Drive.GetMountInfos();
                foreach (var mount in mountInfos)
                {
                    var usagePercent = mount.TotalBytes > 0 ? (double)mount.UsedBytes / mount.TotalBytes * 100 : 0;
                    Console.WriteLine($"  {mount.MountPoint}: {mount.DeviceName} ({mount.FileSystem}) - {usagePercent:F1}% used ({mount.UsedBytes}/{mount.TotalBytes} bytes), IO: {mount.IoUsage:F1}%");
                }
                Console.WriteLine();

                Console.WriteLine("GPU INFORMATION");
                Console.WriteLine($"  Name: {monitor.Gpu.GetGpuName()}");
                Console.WriteLine($"  GPU Usage (%): {monitor.Gpu.GetUsageGpu()}");
                Console.WriteLine($"  Memory Usage (%): {monitor.Gpu.GetUsageMemory()}");
                Console.WriteLine($"  Memory Used: {monitor.Gpu.GetMemoryUsed()}");
                Console.WriteLine($"  Memory Total: {monitor.Gpu.GetMemoryTotal()}");
                Console.WriteLine($"  Temperature (°C): {monitor.Gpu.GetTemperatureGpu()}");
                Console.WriteLine($"  Power State: {monitor.Gpu.GetPowerState()}");
                Console.WriteLine($"  Power Draw (W): {monitor.Gpu.GetPowerDraw()}\n");

                Console.WriteLine("MEMORY INFORMATION");
                Console.WriteLine($"  Total: {monitor.Memory.GetMemTotal()}");
                Console.WriteLine($"  Free: {monitor.Memory.GetMemFree()}");
                Console.WriteLine($"  Available: {monitor.Memory.GetMemAvailable()}");
                Console.WriteLine($"  Used: {monitor.Memory.GetMemUsed()}");
                Console.WriteLine($"  Cached: {monitor.Memory.GetCached()}");
                Console.WriteLine($"  Swap Total: {monitor.Memory.GetSwapTotal()}");
                Console.WriteLine($"  Swap Free: {monitor.Memory.GetSwapFree()}\n");

                Console.WriteLine("NETWORK INFORMATION");
                Console.WriteLine($"  Download in bytes: {monitor.Network.GetDownloadSpeed()}");
                Console.WriteLine($"  Upload in bytes: {monitor.Network.GetUploadSpeed()}");

                var networks = monitor.Network.GetNetworks();
                foreach (var network in networks)
                {
                    Console.WriteLine($"  {network.Name} {network.IsUp}, Upload (Speed {network.UploadSpeed} / Total {network.TotalBytesUploaded}), Download (Speed {network.DownloadSpeed} / Total {network.TotalBytesDownloaded})");
                }
                Console.WriteLine();

                Console.WriteLine("SYSTEM INFORMATION");
                Console.WriteLine($"  Kernel: {monitor.System.GetKernelVersion()}");
                Console.WriteLine($"  Hostname: {monitor.System.GetHostname()}");
                Console.WriteLine($"  Uptime (s): {monitor.System.GetUptimeSeconds()}");
                Console.WriteLine($"  Running Tasks: {monitor.System.GetRunningTasks()}");
                Console.WriteLine($"  Total Tasks: {monitor.System.GetTotalTasks()}\n");
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
                monitor.Cpu.Update();
                monitor.Drive.Update();
                monitor.Gpu.Update();
                monitor.Memory.Update();
                monitor.Network.Update();
                monitor.System.Update();
                logger.LogTrace("Done! Sending event to update interface.");
                _dataReadyEvent.Release();
                await Task.Delay(loopDelay, cts);
            }
        }, cts);
    }
}
