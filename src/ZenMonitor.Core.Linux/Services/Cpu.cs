// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="ICpu"/> that reads CPU metrics from
/// <c>/proc</c> and <c>/sys</c> filesystems.
/// </summary>
[SupportedOSPlatform("linux")]
public class Cpu(ILogger<Cpu> logger, IFileSystem fileSystem, IAbstractionsLinux helper) : ICpu
{
    private const string EnergyUjPath = "/sys/class/powercap/intel-rapl:0/energy_uj";
    private readonly ILogger<Cpu> _logger = logger;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IAbstractionsLinux _helper = helper;
    private CpuInfoSnapshot _snapshot = new("", 0, 0, 0, 0, [], [], []);

    private long[] _currentTotalTicks = [];
    private long[] _previousTotalTicks = [];
    private long[][] _currentCoreTicks = [];
    private long[][] _previousCoreTicks = [];

    private double _prevEnergyUj;
    private DateTime _prevEnergyTime;

    /// <inheritdoc />
    public void Update() => _snapshot = FetchCpuInfo();

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
        string cpuName = "Unknown CPU";
        var speeds = new List<CpuCoreSpeed>();
        int coreIndex = 0;

        using var stream = _fileSystem.FileStream.New("/proc/cpuinfo", FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan();

            if (lineSpan.StartsWith("model name") && cpuName == "Unknown CPU")
            {
                int colonIndex = lineSpan.IndexOf(':');
                if (colonIndex != -1)
                {
                    cpuName = lineSpan[(colonIndex + 1)..].Trim().ToString();
                }
            }
            else if (lineSpan.StartsWith("cpu MHz"))
            {
                int colonIndex = lineSpan.IndexOf(':');
                if (colonIndex != -1)
                {
                    var speedValue = lineSpan[(colonIndex + 1)..].Trim();
                    if (double.TryParse(speedValue, out double mhz))
                    {
                        speeds.Add(new CpuCoreSpeed(coreIndex, mhz));
                        coreIndex++;
                    }
                }
            }
        }

        return (cpuName, speeds.ToArray());
    }
    #endregion

    #region CpuUsages
    private (int totalUsage, CpuCoreUsage[] coreUsages) ReadCpuUsages()
    {
        ReadCurrentTicks();

        int totalUsage = 0;
        CpuCoreUsage[] coreUsages = [];

        if (_previousTotalTicks.Length == 0)
        {
            _previousTotalTicks = new long[_currentTotalTicks.Length];
            Array.Copy(_currentTotalTicks, _previousTotalTicks, _currentTotalTicks.Length);

            _previousCoreTicks = new long[_currentCoreTicks.Length][];
            for (int i = 0; i < _currentCoreTicks.Length; i++)
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
            for (int i = 0; i < coreUsages.Length; i++)
            {
                if (i < _previousCoreTicks.Length)
                {
                    double u = ComputeUsage(_currentCoreTicks[i], _previousCoreTicks[i]);
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
        var lines = _fileSystem.File.ReadLines("/proc/stat").Where(l => l.StartsWith("cpu"));
        bool first = true;

        var coreTickList = new List<long[]>();

        foreach (var line in lines)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int fieldCount = parts.Length - 1;
            var ticks = new long[fieldCount];
            for (int j = 0; j < fieldCount; j++)
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
        int len = Math.Min(curr.Length, prev.Length);
        long totalCurr = 0, totalPrev = 0;
        for (int j = 0; j < len; j++)
        {
            totalCurr += curr[j];
            totalPrev += prev[j];
        }
        long diffTotal = totalCurr - totalPrev;
        if (diffTotal <= 0) return 0;

        long idleCurr = curr.Length > 4 ? curr[3] + curr[4] : curr.Length > 3 ? curr[3] : 0;
        long idlePrev = prev.Length > 4 ? prev[3] + prev[4] : prev.Length > 3 ? prev[3] : 0;
        long diffIdle = idleCurr - idlePrev;

        return (double)(diffTotal - diffIdle) / diffTotal * 100.0;
    }
    #endregion

    #region CpuTemps
    private (int overallTemp, CpuCoreTemp[] coreTemps) ReadCpuTemps(int coreCount)
    {
        int overall = 0;
        var rawSensorTemps = new List<CpuCoreTemp>();

        try
        {
            foreach (var hwmonDir in _fileSystem.Directory.GetDirectories("/sys/class/hwmon"))
            {
                string nameFile = _fileSystem.Path.Combine(hwmonDir, "name");
                if (!_fileSystem.File.Exists(nameFile)) continue;

                string name = _fileSystem.File.ReadAllText(nameFile).Trim();
                if (name == "coretemp")
                {
                    var (devOverall, devTemps) = ReadIntelTemps(hwmonDir);
                    overall = devOverall != 0 ? devOverall : overall;
                    rawSensorTemps.AddRange(devTemps);
                }
                else if (name == "k10temp")
                {
                    var (devOverall, devTemps) = ReadAmdTemps(hwmonDir);
                    overall = devOverall != 0 ? devOverall : overall;
                    rawSensorTemps.AddRange(devTemps);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read CPU temperatures from hwmon");
        }

        CpuCoreTemp[] uniformTemps;

        if (rawSensorTemps.Count == coreCount && rawSensorTemps.All(t => t.Index >= 0))
        {
            uniformTemps = [.. rawSensorTemps.OrderBy(t => t.Index)];
        }
        else
        {
            double avgTemp = rawSensorTemps.Count > 0 ? rawSensorTemps.Average(t => t.Temp) : overall;
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
        int overall = 0;
        var temps = new List<CpuCoreTemp>();

        try
        {
            foreach (var inputFile in _fileSystem.Directory.GetFiles(hwmonDir, "temp*_input"))
            {
                string prefix = _fileSystem.Path.GetFileName(inputFile).Replace("_input", "");
                string labelFile = _fileSystem.Path.Combine(hwmonDir, $"{prefix}_label");

                if (!int.TryParse(_fileSystem.File.ReadAllText(inputFile).Trim(), out int millideg))
                    continue;
                int temp = millideg / 1000;

                string? label = _fileSystem.File.Exists(labelFile) ? _fileSystem.File.ReadAllText(labelFile).Trim() : null;

                if (label != null && (label.Contains("Package") || label == "CPU"))
                {
                    overall = temp;
                }
                else if (label != null && label.StartsWith("Core "))
                {
                    ReadOnlySpan<char> afterSpace = label.AsSpan(label.LastIndexOf(' ') + 1);
                    if (int.TryParse(afterSpace, out int coreIdx))
                        temps.Add(new CpuCoreTemp(coreIdx, temp));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading Intel CPU temperatures from {Dir}", hwmonDir);
        }

        return (overall, temps.ToArray());
    }

    /// <summary>
    /// Reads AMD CPU temperature sensors from hwmon (k10temp).
    /// Looks for "Tctl"/"Tdie" for overall temp and "Tccd#" for per-CCD temps.
    /// </summary>
    private (int overall, CpuCoreTemp[] temps) ReadAmdTemps(string hwmonDir)
    {
        int overall = 0;
        var temps = new List<CpuCoreTemp>();

        try
        {
            foreach (var inputFile in _fileSystem.Directory.GetFiles(hwmonDir, "temp*_input"))
            {
                string prefix = _fileSystem.Path.GetFileName(inputFile).Replace("_input", "");
                string labelFile = _fileSystem.Path.Combine(hwmonDir, $"{prefix}_label");

                if (!int.TryParse(_fileSystem.File.ReadAllText(inputFile).Trim(), out int millideg))
                    continue;
                int temp = millideg / 1000;

                string? label = _fileSystem.File.Exists(labelFile) ? _fileSystem.File.ReadAllText(labelFile).Trim() : null;

                if (label != null && (label.Contains("Tctl") || label.Contains("Tdie")))
                {
                    overall = temp;
                }
                else if (label != null && label.StartsWith("Tccd"))
                {
                    ReadOnlySpan<char> numberPart = label.AsSpan(4);
                    if (int.TryParse(numberPart, out int ccdIdx))
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
            _logger.LogWarning(ex, "Error reading AMD CPU temperatures from {Dir}", hwmonDir);
        }

        return (overall, temps.ToArray());
    }
    #endregion

    #region PowerDraw
    private double ReadPowerDraw()
    {
        if (!_fileSystem.File.Exists(EnergyUjPath)) return 0.0;

        try
        {
            double energyUj = double.Parse(_fileSystem.File.ReadAllText(EnergyUjPath).Trim());
            DateTime now = _helper.UtcNow;

            double power = 0;
            if (_prevEnergyUj > 0)
            {
                double deltaUj = energyUj - _prevEnergyUj;
                if (deltaUj < 0) deltaUj = 0;
                double deltaSec = (now - _prevEnergyTime).TotalSeconds;
                if (deltaSec > 0)
                    power = deltaUj / 1_000_000.0 / deltaSec;
            }

            _prevEnergyUj = energyUj;
            _prevEnergyTime = now;

            return Math.Round(power, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read CPU power draw");
            return 0.0;
        }
    }
    #endregion
}
