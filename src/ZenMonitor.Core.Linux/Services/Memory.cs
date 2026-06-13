// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="IMemory"/> that reads memory metrics
/// from <c>/proc/meminfo</c>.
/// </summary>
[SupportedOSPlatform("linux")]
public class Memory(ILogger<Memory>? logger, IFileSystem fileSystem) : IMemory
{
    private readonly ILogger<Memory> _logger = logger ?? NullLogger<Memory>.Instance;
    private MemoryInfoSnapshot _snapshot = new(0, 0, 0, 0, 0, 0, 0);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchMemoryInfo();

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
            _logger.LogTrace("Fetching all Memory info...");

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

            foreach (var key in required)
            {
                if (!values.ContainsKey(key))
                    throw new KeyNotFoundException($"Could not find '{key}' in /proc/meminfo");
            }

            return new MemoryInfoSnapshot(
                values["MemTotal"], values["MemFree"], values["MemAvailable"],
                Math.Round(values["MemTotal"] - values["MemAvailable"], 2),
                values["Cached"], values["SwapTotal"], values["SwapFree"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch memory info");
            return new MemoryInfoSnapshot(0, 0, 0, 0, 0, 0, 0);
        }
    }
}
