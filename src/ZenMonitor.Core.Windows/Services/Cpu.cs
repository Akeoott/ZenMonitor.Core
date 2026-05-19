// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="ICpu"/> that reads CPU metrics
/// via native Win32 API calls through <see cref="IWindows"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public class Cpu(ILogger<Cpu> logger, IHelper helper) : ICpu
{
    private readonly ILogger<Cpu> _logger = logger;
    private readonly IHelper _helper = helper;
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    private CpuTickInfo _previousTotalTicks = new(0, 0, 0);
    private CpuTickInfo[] _previousCoreTicks = [];
    private bool _firstRead = true;

    /// <summary>Updates all cached CPU metrics by reading from system files.</summary>
    public void Update() => _snapshot = FetchCpuInfo();

    /// <summary>Returns the CPU model name.</summary>
    public string GetCpuName() => _snapshot.CpuName;

    /// <summary>Returns the overall CPU frequency in MHz.</summary>
    public double GetCpuSpeed() => _snapshot.CpuSpeed;

    /// <summary>Returns the overall CPU usage percentage (0-100).</summary>
    public int GetCpuUsage() => _snapshot.CpuUsage;

    /// <summary>Returns the overall CPU temperature in degrees Celsius.</summary>
    public int GetCpuTemp() => _snapshot.CpuTemp;

    /// <summary>Returns the current CPU package power draw in watts.</summary>
    public double GetPowerDraw() => _snapshot.PowerDraw;

    /// <summary>Returns per-core frequency measurements.</summary>
    public CpuCoreSpeed[] GetCoreSpeeds() => _snapshot.CoreSpeeds;

    /// <summary>Returns per-core usage percentages (0-100).</summary>
    public CpuCoreUsage[] GetCoreUsages() => _snapshot.CoreUsages;

    /// <summary>Returns per-core temperature readings in degrees Celsius.</summary>
    public CpuCoreTemp[] GetCoreTemps() => _snapshot.CoreTemps;

    private CpuInfoSnapshot FetchCpuInfo()
    {
        try
        {
            _logger.LogTrace("Fetching all CPU info...");

            var (cpuName, coreSpeeds) = ReadCpuInfo();
            var (totalUsage, coreUsages) = ReadCpuUsages();
            int coreCount = coreSpeeds.Length;
            var (overallTemp, coreTemps) = ReadCpuTemps(coreCount);
            double powerDraw = ReadPowerDraw();

            double overallSpeed = coreSpeeds.Length > 0
                ? Math.Round(coreSpeeds.Average(s => s.Speed), 3)
                : 0;

            return new CpuInfoSnapshot(
                cpuName,
                overallSpeed,
                totalUsage,
                overallTemp,
                powerDraw,
                coreSpeeds,
                coreUsages,
                coreTemps
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch CPU info");
            return new CpuInfoSnapshot("Error", 0, 0, 0, 0, [], [], []);
        }
    }

    #region CpuInfo
    private (string cpuName, CpuCoreSpeed[] coreSpeeds) ReadCpuInfo()
    {
        string cpuName = _helper.Windows.GetProcessorName();
        int coreCount = _helper.Windows.GetProcessorCount();
        int baseMhz = _helper.Windows.GetProcessorBaseFrequencyMHz();

        var speeds = new CpuCoreSpeed[coreCount];
        for (int i = 0; i < coreCount; i++)
            speeds[i] = new CpuCoreSpeed(i, baseMhz);

        return (cpuName, speeds);
    }
    #endregion

    #region CpuUsages
    private (int totalUsage, CpuCoreUsage[] coreUsages) ReadCpuUsages()
    {
        CpuTickInfo currentTotal = _helper.Windows.GetSystemTimes();
        CpuTickInfo[] currentCore = _helper.Windows.GetPerCoreTimes();

        int totalUsage = 0;
        CpuCoreUsage[] coreUsages;

        if (_firstRead)
        {
            _previousTotalTicks = currentTotal;
            _previousCoreTicks = currentCore;
            _firstRead = false;

            coreUsages = [.. currentCore.Select((_, i) => new CpuCoreUsage(i, 0))];
        }
        else
        {
            totalUsage = (int)Math.Round(ComputeUsage(currentTotal, _previousTotalTicks));

            int minLen = Math.Min(currentCore.Length, _previousCoreTicks.Length);
            coreUsages = new CpuCoreUsage[currentCore.Length];

            for (int i = 0; i < currentCore.Length; i++)
            {
                double usage = i < minLen
                    ? ComputeUsage(currentCore[i], _previousCoreTicks[i])
                    : 0;
                coreUsages[i] = new CpuCoreUsage(i, (int)Math.Round(usage));
            }

            _previousTotalTicks = currentTotal;
            _previousCoreTicks = currentCore;
        }

        return (totalUsage, coreUsages);
    }

    /// <summary>
    /// Computes CPU usage percentage from a delta between two tick snapshots.
    /// Total = kernel + user, idle = idle.
    /// usage = (total_delta - idle_delta) / total_delta * 100
    /// </summary>
    private static double ComputeUsage(CpuTickInfo curr, CpuTickInfo prev)
    {
        long totalCurr = curr.KernelTime + curr.UserTime;
        long totalPrev = prev.KernelTime + prev.UserTime;
        long diffTotal = totalCurr - totalPrev;

        if (diffTotal <= 0) return 0;

        long diffIdle = curr.IdleTime - prev.IdleTime;
        return (double)(diffTotal - diffIdle) / diffTotal * 100.0;
    }
    #endregion

    #region CpuTemps
    private (int overallTemp, CpuCoreTemp[] coreTemps) ReadCpuTemps(int coreCount)
    {
        int overall = _helper.Windows.GetCpuTemperature(); // Windows only provides overall temp.

        var coreTemps = new CpuCoreTemp[coreCount];
        for (int i = 0; i < coreCount; i++)
            coreTemps[i] = new CpuCoreTemp(i, overall);

        return (overall, coreTemps);
    }
    #endregion

    #region PowerDraw
    private double ReadPowerDraw()
    {
        try
        {
            return _helper.Windows.GetCpuPowerDraw();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read CPU power draw");
            return 0.0;
        }
    }
    #endregion
}
