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
/// Linux implementation of <see cref="IProcess"/> that reads process information
/// from the <c>/proc</c> filesystem.
/// </summary>
[SupportedOSPlatform("linux")]
public class Process(ILogger<Process> logger, IFileSystem fileSystem, IUtilsLinux utils) : IProcess
{
    private ProcessInfoSnapshot _snapshot = new(0, []);
    private readonly Dictionary<int, CpuCacheEntry> _cpuCache = new();
    private Dictionary<int, string> _userMap = new();
    private DateTime _lastUserMapUpdate = DateTime.MinValue;

    /// <inheritdoc />
    public void Update() => _snapshot = FetchProcessInfo();

    /// <inheritdoc />
    public ProcessInfoSnapshot GetSnapshot() => _snapshot;

    /// <inheritdoc />
    public int GetTotalProcesses() => _snapshot.TotalProcesses;

    /// <inheritdoc />
    public ReadOnlySpan<ProcessDetail> GetProcesses() => _snapshot.ProcessDetails;

    private ProcessInfoSnapshot FetchProcessInfo()
    {
        var utcNow = utils.UtcNow;
        var newProcessList = new List<ProcessDetail>();
        var activePids = new HashSet<int>();

        // Refresh user map lazily once every 10 minutes
        if ((utcNow - _lastUserMapUpdate).TotalMinutes > 10)
        {
            BuildUserMap();
            _lastUserMapUpdate = utcNow;
        }

        try
        {
            if (!fileSystem.Directory.Exists("/proc")) return new ProcessInfoSnapshot(0, []);

            foreach (var dir in fileSystem.Directory.EnumerateDirectories("/proc"))
            {
                var dirName = Path.GetFileName(dir);
                if (!int.TryParse(dirName, out var pid))
                    continue;

                var statusPath = Path.Combine(dir, "status");
                var statPath = Path.Combine(dir, "stat");
                var cmdlinePath = Path.Combine(dir, "cmdline");

                if (!fileSystem.File.Exists(statusPath))
                    continue;

                _ = activePids.Add(pid);

                ParseStatusFile(statusPath, out var programName, out var state, out var threads, out var uid,
                    out var memUsageKb);

                // Fallback to directory name if Name wasn't found
                if (string.IsNullOrEmpty(programName))
                    programName = dirName;

                var commandLine = ReadCommandLine(cmdlinePath);
                var userName = _userMap.GetValueOrDefault(uid, "N/A");
                var memUsage = (int)Math.Round(memUsageKb / 1024.0);
                var cpuUsage = ComputeCpuUsage(pid, statPath, utcNow);

                newProcessList.Add(new ProcessDetail(
                    pid,
                    programName,
                    commandLine,
                    userName,
                    state,
                    threads,
                    memUsage,
                    cpuUsage
                ));
            }

            return new ProcessInfoSnapshot(newProcessList.Count, newProcessList.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enumerating processes");
            return new ProcessInfoSnapshot(0, []);
        }
        finally
        {
            // Clean up dead processes from the CPU cache
            var deadPids = _cpuCache.Keys.Where(p => !activePids.Contains(p)).ToList();
            foreach (var deadPid in deadPids)
            {
                _ = _cpuCache.Remove(deadPid);
            }
        }
    }

    private void ParseStatusFile(string path, out string programName, out ProcessState state, out int threads,
        out int uid, out int vmRssKb)
    {
        programName = string.Empty;
        state = ProcessState.Unknown;
        threads = 1;
        uid = 0;
        vmRssKb = 0;

        try
        {
            var lines = fileSystem.File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (line.StartsWith("Name:", StringComparison.Ordinal))
                {
                    programName = line["Name:".Length..].Trim();
                }
                else if (line.StartsWith("State:", StringComparison.Ordinal))
                {
                    var stateStr = line["State:".Length..].Trim();
                    if (stateStr.Length > 0)
                        state = ParseStateChar(stateStr[0]);
                }
                else if (line.StartsWith("Threads:", StringComparison.Ordinal))
                {
                    _ = int.TryParse(line["Threads:".Length..].Trim(), out threads);
                }
                else if (line.StartsWith("Uid:", StringComparison.Ordinal))
                {
                    // Uid line contains: Real, Effective, Saved, File System UIDs separated by tabs
                    var parts = line["Uid:".Length..].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        _ = int.TryParse(parts[0].Trim(), out uid);
                }
                else if (line.StartsWith("VmRSS:", StringComparison.Ordinal))
                {
                    // VmRSS line contains something like: "VmRSS:     17960 kB"
                    var parts = line["VmRSS:".Length..].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        _ = int.TryParse(parts[0], out vmRssKb);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Something unexpected happened.");
            // Fail "silently" and return defaults if a single file read locks up
        }
    }

    private double ComputeCpuUsage(int pid, string statPath, DateTime utcNow)
    {
        try
        {
            if (!fileSystem.File.Exists(statPath)) return 0;

            var content = fileSystem.File.ReadAllText(statPath);

            // To isolate numeric fields safely, skip past the last closing parenthesis
            // since process names can contain spaces like: (sd-pam)
            var lastParen = content.LastIndexOf(')');
            if (lastParen == -1 || lastParen + 2 >= content.Length) return 0;

            var numericPart = content[(lastParen + 2)..];
            var fields = numericPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // In the truncated numeric part:
            // Field 3 (State) is at index 0.
            // Field 14 (utime) is at index 11.w
            // Field 15 (stime) is at index 12.
            if (fields.Length <= 12 ||
                !double.TryParse(fields[11], out var utime) ||
                !double.TryParse(fields[12], out var stime)) return 0;

            var totalCpuTime = utime + stime;

            if (!_cpuCache.TryGetValue(pid, out var prev))
            {
                _cpuCache[pid] = new CpuCacheEntry { TotalCpuTime = totalCpuTime, Time = utcNow };
                return 0;
            }

            var deltaTime = (utcNow - prev.Time).TotalSeconds;
            var deltaCpu = totalCpuTime - prev.TotalCpuTime;

            prev.Time = utcNow;
            prev.TotalCpuTime = totalCpuTime;
            _cpuCache[pid] = prev;

            return deltaTime > 0 && deltaCpu >= 0
                ? Math.Round(deltaCpu / deltaTime / utils.ProcessorCount * 100.0, 2) : 0;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Something unexpected happened.");
            return 0;
        }
    }

    private string ReadCommandLine(string path)
    {
        if (!fileSystem.File.Exists(path)) return string.Empty;
        try
        {
            var text = fileSystem.File.ReadAllText(path);
            return string.IsNullOrEmpty(text) ? string.Empty : text.Replace('\0', ' ').Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Something unexpected happened.");
            return string.Empty;
        }
    }

    private void BuildUserMap()
    {
        const string passwdPath = "/etc/passwd";
        if (!fileSystem.File.Exists(passwdPath)) return;

        try
        {
            var newMap = new Dictionary<int, string>();
            var lines = fileSystem.File.ReadAllLines(passwdPath);

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var uid))
                {
                    newMap[uid] = parts[0];
                }
            }

            _userMap = newMap;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read /etc/passwd");
        }
    }

    private static ProcessState ParseStateChar(char c) => c switch
    {
        'R' => ProcessState.Running,
        'S' or 'I' => ProcessState.Sleeping,
        'D' => ProcessState.DiskSleep,
        'Z' => ProcessState.Zombie,
        'T' => ProcessState.Stopped,
        't' => ProcessState.TracingStop,
        'X' or 'x' => ProcessState.Dead,
        _ => ProcessState.Unknown
    };

    private struct CpuCacheEntry
    {
        public double TotalCpuTime;
        public DateTime Time;
    }
}
