// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Threading;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Debug;

internal sealed class Monitor(ILogger<Monitor> logger, ITelemetryAggregate monitor)
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
                Console.WriteLine($"  Name: {monitor.CpuTel.GetCpuName()}");
                Console.Write($"  Speed (MHz): C0 {monitor.CpuTel.GetCpuSpeed()}");
                var cpuCoreSpeed = monitor.CpuTel.GetCoreSpeeds();
                foreach (var speed in cpuCoreSpeed)
                {
                    Console.Write($", C{speed.Index + 1} {speed.Speed}");
                }
                Console.WriteLine();

                Console.Write($"  Usage (%): C0 {monitor.CpuTel.GetCpuUsage()}");
                var cpuCoreUsage = monitor.CpuTel.GetCoreUsages();
                foreach (var usage in cpuCoreUsage)
                {
                    Console.Write($", C{usage.Index + 1} {usage.Usage}");
                }
                Console.WriteLine();

                Console.Write($"  Temperature (°C): C0 {monitor.CpuTel.GetCpuTemp()}");
                var cpuCoreTemp = monitor.CpuTel.GetCoreTemps();
                foreach (var temp in cpuCoreTemp)
                {
                    Console.Write($", C{temp.Index + 1} {temp.Temp}");
                }
                Console.WriteLine();
                Console.WriteLine($"  Power Draw (W): {monitor.CpuTel.GetPowerDraw()}\n");

                Console.WriteLine("DRIVE INFORMATION");
                var mountInfos = monitor.DriveTel.GetMountInfos();
                foreach (var mount in mountInfos)
                {
                    var usagePercent = mount.TotalBytes > 0 ? (double)mount.UsedBytes / mount.TotalBytes * 100 : 0;
                    Console.WriteLine($"  {mount.MountPoint}: {mount.DeviceName} ({mount.FileSystem}) - {usagePercent:F1}% used ({mount.UsedBytes}/{mount.TotalBytes} bytes), IO: {mount.IoUsage:F1}%");
                }
                Console.WriteLine();

                Console.WriteLine("GPU INFORMATION");
                Console.WriteLine($"  Name: {monitor.GpuTel.GetGpuName()}");
                Console.WriteLine($"  GPU Usage (%): {monitor.GpuTel.GetUsageGpu()}");
                Console.WriteLine($"  Memory Usage (%): {monitor.GpuTel.GetUsageMemory()}");
                Console.WriteLine($"  Memory Used: {monitor.GpuTel.GetMemoryUsed()}");
                Console.WriteLine($"  Memory Total: {monitor.GpuTel.GetMemoryTotal()}");
                Console.WriteLine($"  Temperature (°C): {monitor.GpuTel.GetTemperatureGpu()}");
                Console.WriteLine($"  Power State: {monitor.GpuTel.GetPowerState()}");
                Console.WriteLine($"  Power Draw (W): {monitor.GpuTel.GetPowerDraw()}\n");

                Console.WriteLine("MEMORY INFORMATION");
                Console.WriteLine($"  Total: {monitor.MemoryTel.GetMemTotal()}");
                Console.WriteLine($"  Free: {monitor.MemoryTel.GetMemFree()}");
                Console.WriteLine($"  Available: {monitor.MemoryTel.GetMemAvailable()}");
                Console.WriteLine($"  Used: {monitor.MemoryTel.GetMemUsed()}");
                Console.WriteLine($"  Cached: {monitor.MemoryTel.GetCached()}");
                Console.WriteLine($"  Swap Total: {monitor.MemoryTel.GetSwapTotal()}");
                Console.WriteLine($"  Swap Free: {monitor.MemoryTel.GetSwapFree()}\n");

                // Hidden due to the MASSIVE output.
                Console.WriteLine("PROCESS INFORMATION");
                Console.WriteLine($"  Total Processes: {monitor.ProcessTel.GetTotalProcesses()}");
                var processes = monitor.ProcessTel.GetProcesses();
                foreach (var process in processes)
                {
                    Console.WriteLine($"  {process.Pid}, {process.Program}, {process.Command}, {process.State}, {process.Threads}, {process.User}, {process.MemoryUsage}, {process.CpuUsage}");
                }
                Console.WriteLine();

                Console.WriteLine("NETWORK INFORMATION");
                Console.WriteLine($"  Download in bytes: {monitor.NetworkTel.GetDownloadSpeed()}");
                Console.WriteLine($"  Upload in bytes: {monitor.NetworkTel.GetUploadSpeed()}\n");

                var networks = monitor.NetworkTel.GetNetworks();
                foreach (var network in networks)
                {
                    Console.WriteLine($"  {network.Name} {network.IsUp}, Upload (Speed {network.UploadSpeed} / Total {network.TotalBytesUploaded}), Download (Speed {network.DownloadSpeed} / Total {network.TotalBytesDownloaded})");
                }
                Console.WriteLine();

                Console.WriteLine("SYSTEM INFORMATION");
                Console.WriteLine($"  Kernel: {monitor.SystemTel.GetKernelVersion()}");
                Console.WriteLine($"  Hostname: {monitor.SystemTel.GetHostname()}");
                Console.WriteLine($"  Uptime (s): {monitor.SystemTel.GetUptimeSeconds()}");
                Console.WriteLine($"  Running Tasks: {monitor.SystemTel.GetRunningTasks()}");
                Console.WriteLine($"  Total Tasks: {monitor.SystemTel.GetTotalTasks()}\n");
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
                monitor.UpdateAll();
                logger.LogTrace("Done! Sending event to update interface.");
                _dataReadyEvent.Release();
                await Task.Delay(loopDelay, cts);
            }
        }, cts);
    }
}
