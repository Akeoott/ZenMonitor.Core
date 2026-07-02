// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions.Telemetry;
using ZenMonitor.Core.Models.Telemetry;

namespace ZenMonitor.Core.Linux.Services.Telemetry;

/// <summary>
/// Linux implementation of <see cref="IMemoryTel"/> that reads memory metrics
/// from <c>/proc/meminfo</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class MemoryTel(ILogger<MemoryTel> logger, IFileSystem fileSystem) : IMemoryTel
{
    private MemoryInfoSnapshot _snapshot = new(0, 0, 0, 0, 0, 0, 0);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchMemoryInfo();

    /// <inheritdoc />
    public MemoryInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public double GetMemTotal() => _snapshot.MemTotal;

    /// <inheritdoc />
    public double GetMemFree() => _snapshot.MemFree;

    /// <inheritdoc />
    public double GetMemAvailable() => _snapshot.MemAvailable;

    /// <inheritdoc />
    public double GetMemUsed() => _snapshot.MemUsed;

    /// <inheritdoc />
    public double GetCached() => _snapshot.Cached;

    /// <inheritdoc />
    public double GetSwapTotal() => _snapshot.SwapTotal;

    /// <inheritdoc />
    public double GetSwapFree() => _snapshot.SwapFree;

    private MemoryInfoSnapshot FetchMemoryInfo()
    {
        try
        {
            logger.LogTrace("Fetching all Memory info...");

            var values = new Dictionary<string, double>(StringComparer.Ordinal);
            const double kbToGib = 1.0 / 1_048_576;

            foreach (var line in fileSystem.File.ReadLines("/proc/meminfo"))
            {
                var colon = line.IndexOf(':');
                if (colon < 0) continue;

                var key = line[..colon].Trim();
                if (key != "MemTotal" && key != "MemFree" && key != "MemAvailable" &&
                    key != "Cached" && key != "SwapTotal" && key != "SwapFree")
                {
                    continue;
                }

                var valuePart = line[(colon + 1)..].Trim();
                var space = valuePart.IndexOf(' ');
                var numberStr = space >= 0 ? valuePart[..space] : valuePart;

                if (double.TryParse(numberStr, out var kb))
                {
                    values[key] = Math.Round(kb * kbToGib, 2);
                }
                else
                {
                    throw new FormatException($"Could not parse '{key}' value '{numberStr}'");
                }
            }

            string[] required = ["MemTotal", "MemFree", "MemAvailable", "Cached", "SwapTotal", "SwapFree"];

            var missing = required.Where(k => !values.ContainsKey(k)).ToList();
            if (missing.Count > 0)
                throw new KeyNotFoundException(
                    $"Could not find the following keys in /proc/meminfo: {string.Join(", ", missing)}");

            return new MemoryInfoSnapshot(
                MemTotal:      values["MemTotal"],
                MemFree:       values["MemFree"],
                MemAvailable:  values["MemAvailable"],
                MemUsed:       Math.Round(values["MemTotal"] - values["MemAvailable"], 2),
                Cached:        values["Cached"],
                SwapTotal:     values["SwapTotal"],
                SwapFree:      values["SwapFree"]);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Fetched memory info is missing keys");
            return new MemoryInfoSnapshot(0, 0, 0, 0, 0, 0, 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch memory info");
            return new MemoryInfoSnapshot(0, 0, 0, 0, 0, 0, 0);
        }
    }
}
