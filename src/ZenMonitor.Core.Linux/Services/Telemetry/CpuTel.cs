// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.IO.Abstractions;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;
using ZenMonitor.Core.Utils;

namespace ZenMonitor.Core.Linux.Services.Telemetry;

/// <summary>
/// Linux implementation of <see cref="ICpuTel"/> that reads CPU metrics from
/// <c>/proc</c> and <c>/sys</c> filesystems.
/// </summary>
[SupportedOSPlatform("linux")]
public class CpuTel(ILogger<CpuTel> logger, IFileSystem fileSystem, IUtilsLinux utils) : ICpuTel
{
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    private long[] _currentTotalTicks = [];
    private long[] _previousTotalTicks = [];
    private long[][] _currentCoreTicks = [];
    private long[][] _previousCoreTicks = [];

    private static readonly string[] RaplNamePatterns = ["intel-rapl", "amd-rapl"];
    private string? _energyUjPath;
    private double _prevEnergyUj;
    private DateTime _prevEnergyTime;
    private bool _raplDiscovered;

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
        var cpuName = "Unknown CPU";
        var speeds = new List<CpuCoreSpeed>();
        var coreIndex = 0;

        using var stream = fileSystem.FileStream.New("/proc/cpuinfo", FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            var lineSpan = line.AsSpan();

            if (lineSpan.StartsWith("model name") && cpuName == "Unknown CPU")
            {
                var colonIndex = lineSpan.IndexOf(':');
                if (colonIndex != -1)
                {
                    cpuName = lineSpan[(colonIndex + 1)..].Trim().ToString();
                }
            }
            else if (lineSpan.StartsWith("cpu MHz"))
            {
                var colonIndex = lineSpan.IndexOf(':');
                if (colonIndex == -1) continue;

                var speedValue = lineSpan[(colonIndex + 1)..].Trim();
                if (!double.TryParse(speedValue, out var mhz)) continue;

                speeds.Add(new CpuCoreSpeed(coreIndex, mhz));
                coreIndex++;
            }
        }

        return (cpuName, speeds.ToArray());
    }
    #endregion

    #region CpuUsages
    private (int totalUsage, CpuCoreUsage[] coreUsages) ReadCpuUsages()
    {
        ReadCurrentTicks();

        var totalUsage = 0;
        CpuCoreUsage[] coreUsages;

        if (_previousTotalTicks.Length == 0)
        {
            _previousTotalTicks = new long[_currentTotalTicks.Length];
            Array.Copy(_currentTotalTicks, _previousTotalTicks, _currentTotalTicks.Length);

            _previousCoreTicks = new long[_currentCoreTicks.Length][];
            for (var i = 0; i < _currentCoreTicks.Length; i++)
            {
                _previousCoreTicks[i] = new long[_currentCoreTicks[i].Length];
                Array.Copy(_currentCoreTicks[i], _previousCoreTicks[i], _currentCoreTicks[i].Length);
            }

            coreUsages = [.. _currentCoreTicks.Select((_, i) => new CpuCoreUsage(i, 0))];
        }
        else
        {
            totalUsage = (int)Math.Round(ComputeUsage(_currentTotalTicks, _previousTotalTicks));

            coreUsages = new CpuCoreUsage[_currentCoreTicks.Length];
            for (var i = 0; i < coreUsages.Length; i++)
            {
                if (i < _previousCoreTicks.Length)
                {
                    var u = ComputeUsage(_currentCoreTicks[i], _previousCoreTicks[i]);
                    coreUsages[i] = new CpuCoreUsage(i, (int)Math.Round(u));
                }
                else
                    coreUsages[i] = new CpuCoreUsage(i, 0);
            }

            _previousTotalTicks = _currentTotalTicks;
            _previousCoreTicks = _currentCoreTicks;
            _currentTotalTicks = [];
            _currentCoreTicks = [];
        }

        return (totalUsage, coreUsages);
    }

    private void ReadCurrentTicks()
    {
        var lines = fileSystem.File.ReadLines("/proc/stat").Where(l => l.StartsWith("cpu"));
        var first = true;

        var coreTickList = new List<long[]>();

        foreach (var line in lines)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var fieldCount = parts.Length - 1;
            var ticks = new long[fieldCount];
            for (var j = 0; j < fieldCount; j++)
                ticks[j] = long.Parse(parts[j + 1]);

            if (first)
            {
                _currentTotalTicks = ticks;
                first = false;
            }
            else
            {
                coreTickList.Add(ticks);
            }
        }
        _currentCoreTicks = [.. coreTickList];
    }

    private static double ComputeUsage(long[] curr, long[] prev)
    {
        var len = Math.Min(curr.Length, prev.Length);
        long totalCurr = 0, totalPrev = 0;
        for (var j = 0; j < len; j++)
        {
            totalCurr += curr[j];
            totalPrev += prev[j];
        }
        var diffTotal = totalCurr - totalPrev;
        if (diffTotal <= 0) return 0;

        var idleCurr = curr.Length > 4 ? curr[3] + curr[4] : curr.Length > 3 ? curr[3] : 0;
        var idlePrev = prev.Length > 4 ? prev[3] + prev[4] : prev.Length > 3 ? prev[3] : 0;
        var diffIdle = idleCurr - idlePrev;

        return (double)(diffTotal - diffIdle) / diffTotal * 100.0;
    }
    #endregion

    #region CpuTemps
    private (int overallTemp, CpuCoreTemp[] coreTemps) ReadCpuTemps(int coreCount)
    {
        var overall = 0;
        var rawSensorTemps = new List<CpuCoreTemp>();

        try
        {
            foreach (var hwmonDir in fileSystem.Directory.GetDirectories("/sys/class/hwmon"))
            {
                var nameFile = fileSystem.Path.Combine(hwmonDir, "name");
                if (!fileSystem.File.Exists(nameFile)) continue;

                var name = fileSystem.File.ReadAllText(nameFile).Trim();
                switch (name)
                {
                    case "coretemp":
                        {
                            var (devOverall, devTemps) = ReadIntelTemps(hwmonDir);
                            overall = devOverall != 0 ? devOverall : overall;
                            rawSensorTemps.AddRange(devTemps);
                            break;
                        }
                    case "k10temp":
                        {
                            var (devOverall, devTemps) = ReadAmdTemps(hwmonDir);
                            overall = devOverall != 0 ? devOverall : overall;
                            rawSensorTemps.AddRange(devTemps);
                            break;
                        }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read CPU temperatures from hwmon");
        }

        CpuCoreTemp[] uniformTemps;

        if (rawSensorTemps.Count == coreCount && rawSensorTemps.All(t => t.Index >= 0))
        {
            uniformTemps = [.. rawSensorTemps.OrderBy(t => t.Index)];
        }
        else
        {
            var avgTemp = rawSensorTemps.Count > 0 ? rawSensorTemps.Average(t => t.Temp) : overall;
            uniformTemps = [.. Enumerable.Range(0, coreCount).Select(i =>
                new CpuCoreTemp(i, (int)Math.Round(avgTemp))
            )];
        }

        return (overall, uniformTemps);
    }

    /// <summary>
    /// Reads Intel CPU temperature sensors from hwmon.
    /// Looks for "Package" or "CPU" label for overall temp and "Core #" for per-core temps.
    /// </summary>
    private (int overall, CpuCoreTemp[] temps) ReadIntelTemps(string hwmonDir)
    {
        var overall = 0;
        var temps = new List<CpuCoreTemp>();

        try
        {
            foreach (var inputFile in fileSystem.Directory.GetFiles(hwmonDir, "temp*_input"))
            {
                var prefix = fileSystem.Path.GetFileName(inputFile).Replace("_input", "");
                var labelFile = fileSystem.Path.Combine(hwmonDir, $"{prefix}_label");

                if (!int.TryParse(fileSystem.File.ReadAllText(inputFile).Trim(), out var millideg))
                    continue;
                var temp = millideg / 1000;

                var label = fileSystem.File.Exists(labelFile) ? fileSystem.File.ReadAllText(labelFile).Trim() : null;

                if (label != null && (label.Contains("Package") || label == "CPU"))
                {
                    overall = temp;
                }
                else if (label != null && label.StartsWith("Core "))
                {
                    var afterSpace = label.AsSpan(label.LastIndexOf(' ') + 1);
                    if (int.TryParse(afterSpace, out var coreIdx))
                        temps.Add(new CpuCoreTemp(coreIdx, temp));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error reading Intel CPU temperatures from {Dir}", hwmonDir);
        }

        return (overall, temps.ToArray());
    }

    /// <summary>
    /// Reads AMD CPU temperature sensors from hwmon (k10temp).
    /// Looks for "Tctl"/"Tdie" for overall temp and "Tccd#" for per-CCD temps.
    /// </summary>
    private (int overall, CpuCoreTemp[] temps) ReadAmdTemps(string hwmonDir)
    {
        var overall = 0;
        var temps = new List<CpuCoreTemp>();

        try
        {
            foreach (var inputFile in fileSystem.Directory.GetFiles(hwmonDir, "temp*_input"))
            {
                var prefix = fileSystem.Path.GetFileName(inputFile).Replace("_input", "");
                var labelFile = fileSystem.Path.Combine(hwmonDir, $"{prefix}_label");

                if (!int.TryParse(fileSystem.File.ReadAllText(inputFile).Trim(), out var millideg))
                    continue;
                var temp = millideg / 1000;

                var label = fileSystem.File.Exists(labelFile) ? fileSystem.File.ReadAllText(labelFile).Trim() : null;

                if (label != null && (label.Contains("Tctl") || label.Contains("Tdie")))
                {
                    overall = temp;
                }
                else if (label != null && label.StartsWith("Tccd"))
                {
                    var numberPart = label.AsSpan(4);
                    if (int.TryParse(numberPart, out var ccdIdx))
                        temps.Add(new CpuCoreTemp(ccdIdx, temp));
                }
                else if (label == null && overall == 0)
                {
                    overall = temp;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error reading AMD CPU temperatures from {Dir}", hwmonDir);
        }

        return (overall, temps.ToArray());
    }
    #endregion

    #region PowerDraw
    private double ReadPowerDraw()
    {
        if (_energyUjPath == null)
        {
            if (_raplDiscovered) return 0.0;
            _energyUjPath = DiscoverRaplPath();
            _raplDiscovered = _energyUjPath == null;
            if (_energyUjPath == null) return 0.0;
        }

        if (!fileSystem.File.Exists(_energyUjPath)) return 0.0;

        try
        {
            var energyUj = double.Parse(fileSystem.File.ReadAllText(_energyUjPath).Trim());
            var currentTime = utils.UtcNow;

            double power = 0;
            if (_prevEnergyUj > 0)
            {
                var deltaUj = energyUj - _prevEnergyUj;
                if (deltaUj < 0) deltaUj = 0;
                var deltaSec = (currentTime - _prevEnergyTime).TotalSeconds;
                if (deltaSec > 0)
                    power = deltaUj / 1_000_000.0 / deltaSec;
            }

            _prevEnergyUj = energyUj;
            _prevEnergyTime = currentTime;

            return Math.Round(power, 2);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Failed to read CPU power draw. Requires root access.");
            return 0.0;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read CPU power draw");
            return 0.0;
        }
    }

    private string? DiscoverRaplPath()
    {
        const string powercapDir = "/sys/class/powercap";
        if (!fileSystem.Directory.Exists(powercapDir)) return null;

        try
        {
            foreach (var dir in fileSystem.Directory.EnumerateDirectories(powercapDir))
            {
                var nameFile = fileSystem.Path.Combine(dir, "name");
                if (!fileSystem.File.Exists(nameFile)) continue;

                var name = fileSystem.File.ReadAllText(nameFile).Trim();
                if (!RaplNamePatterns.Any(p => name.StartsWith(p, StringComparison.Ordinal))) continue;

                var energyPath = fileSystem.Path.Combine(dir, "energy_uj");
                if (!fileSystem.File.Exists(energyPath)) continue;

                logger.LogTrace("Discovered RAPL power domain: {Path}", energyPath);
                return energyPath;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to discover RAPL power domain");
        }

        return null;
    }
    #endregion
}
