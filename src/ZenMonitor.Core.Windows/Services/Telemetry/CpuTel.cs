// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Linq;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Windows.Services.Telemetry;

/// <summary>
/// Windows implementation of <see cref="ICpuTel"/> that reads CPU metrics
/// via native Win32 API calls through <see cref="IUtilsWindows"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public class CpuTel(ILogger<CpuTel> logger, IUtilsWindows utils) : ICpuTel
{
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    private CpuTickInfo _previousTotalTicks = new(0, 0, 0);
    private CpuTickInfo[] _previousCoreTicks = [];
    private bool _firstRead = true;

    /// <inheritdoc />
    public void Update() => _snapshot = FetchCpuInfo();

    /// <inheritdoc />
    public CpuInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public string GetCpuName() => _snapshot.CpuName;

    /// <inheritdoc />
    public double GetCpuSpeed() => _snapshot.CpuSpeed;

    /// <inheritdoc />
    public int GetCpuUsage() => _snapshot.CpuUsage;

    /// <inheritdoc />
    public int GetCpuTemp() => _snapshot.CpuTemp;

    /// <inheritdoc />
    public double GetPowerDraw() => _snapshot.PowerDraw;

    /// <inheritdoc />
    public CpuCoreSpeed[] GetCoreSpeeds() => _snapshot.CoreSpeeds;

    /// <inheritdoc />
    public CpuCoreUsage[] GetCoreUsages() => _snapshot.CoreUsages;

    /// <inheritdoc />
    public CpuCoreTemp[] GetCoreTemps() => _snapshot.CoreTemps;

    private CpuInfoSnapshot FetchCpuInfo()
    {
        try
        {
            logger.LogTrace("Fetching all CPU info...");

            var (cpuName, coreSpeeds) = ReadCpuInfo();
            var (totalUsage, coreUsages) = ReadCpuUsages();
            var coreCount = coreSpeeds.Length;
            var (overallTemp, coreTemps) = ReadCpuTemps(coreCount);
            var powerDraw = ReadPowerDraw();

            var overallSpeed = coreSpeeds.Length > 0
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
            logger.LogError(ex, "Failed to fetch CPU info");
            return new CpuInfoSnapshot("Error", 0, 0, 0, 0, [], [], []);
        }
    }

    #region CpuInfo
    private (string cpuName, CpuCoreSpeed[] coreSpeeds) ReadCpuInfo()
    {
        var cpuName = utils.RawCpu.GetProcessorName();
        var coreCount = utils.RawCpu.GetProcessorCount();
        var baseMhz = utils.RawCpu.GetCpuFrequencyMHz();

        var speeds = new CpuCoreSpeed[coreCount];
        for (var i = 0; i < coreCount; i++)
            speeds[i] = new CpuCoreSpeed(i, baseMhz);

        return (cpuName, speeds);
    }
    #endregion

    #region CpuUsages
    private (int totalUsage, CpuCoreUsage[] coreUsages) ReadCpuUsages()
    {
        var currentTotal = utils.RawCpu.GetSystemTimes();
        var currentCore = utils.RawCpu.GetPerCoreTimes();

        var totalUsage = 0;
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

            var minLen = Math.Min(currentCore.Length, _previousCoreTicks.Length);
            coreUsages = new CpuCoreUsage[currentCore.Length];

            for (var i = 0; i < currentCore.Length; i++)
            {
                var usage = i < minLen
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
        var totalCurr = curr.KernelTime + curr.UserTime;
        var totalPrev = prev.KernelTime + prev.UserTime;
        var diffTotal = totalCurr - totalPrev;

        if (diffTotal <= 0) return 0;

        var diffIdle = curr.IdleTime - prev.IdleTime;
        return (double)(diffTotal - diffIdle) / diffTotal * 100.0;
    }
    #endregion

    #region CpuTemps
    private (int overallTemp, CpuCoreTemp[] coreTemps) ReadCpuTemps(int coreCount)
    {
        var overall = utils.RawCpu.GetTemperature(); // Windows only provides overall temp.

        var coreTemps = new CpuCoreTemp[coreCount];
        for (var i = 0; i < coreCount; i++)
            coreTemps[i] = new CpuCoreTemp(i, overall);

        return (overall, coreTemps);
    }
    #endregion

    #region PowerDraw
    private double ReadPowerDraw()
    {
        try
        {
            return utils.RawCpu.GetPowerDraw();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read CPU power draw");
            return 0.0;
        }
    }
    #endregion
}
