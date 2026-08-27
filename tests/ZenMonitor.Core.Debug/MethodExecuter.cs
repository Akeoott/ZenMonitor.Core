// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json;
using System.Threading;

using ZenMonitor.Core.Abstractions;

namespace ZenMonitor.Core.Debug;

internal class MethodExecuter(ITelemetryAggregate telemetry)
{
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly Dictionary<string, (Action Update, Func<object> GetSnapshot)> _telemetryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cpu"]     = (telemetry.CpuTel.Update,     telemetry.CpuTel.GetSnapshot),
        ["memory"]  = (telemetry.MemoryTel.Update,  telemetry.MemoryTel.GetSnapshot),
        ["gpu"]     = (telemetry.GpuTel.Update,     telemetry.GpuTel.GetSnapshot),
        ["system"]  = (telemetry.SystemTel.Update,  telemetry.SystemTel.GetSnapshot),
        ["drive"]   = (telemetry.DriveTel.Update,   telemetry.DriveTel.GetSnapshot),
        ["network"] = (telemetry.NetworkTel.Update, telemetry.NetworkTel.GetSnapshot),
        ["process"] = (telemetry.ProcessTel.Update, telemetry.ProcessTel.GetSnapshot),
    };

    public void Execute(Options opts)
    {
        if (string.IsNullOrEmpty(opts.DumpCategory))
            return;

        Console.WriteLine("\n\n--- Updating Snapshots ---\n\n");
        var snapshot = GetSnapshotForCategory(opts.DumpCategory);
        Console.WriteLine("\n\n--- Output ---\n\n");
        Console.WriteLine(JsonSerializer.Serialize(snapshot, _serializerOptions));
    }

    private object GetSnapshotForCategory(string category)
    {
        if (category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            telemetry.UpdateAll();
            DelayWithSpace();
            telemetry.UpdateAll();

            return new
            {
                Cpu     = telemetry.CpuTel.GetSnapshot(),
                Memory  = telemetry.MemoryTel.GetSnapshot(),
                Gpu     = telemetry.GpuTel.GetSnapshot(),
                System  = telemetry.SystemTel.GetSnapshot(),
                Drive   = telemetry.DriveTel.GetSnapshot(),
                Network = telemetry.NetworkTel.GetSnapshot(),
                Process = telemetry.ProcessTel.GetSnapshot(),
            };
        }

        if (!_telemetryMap.TryGetValue(category, out var entry))
            throw new ArgumentException($"Unknown category: {category}");

        entry.Update();
        DelayWithSpace();
        entry.Update();
        return entry.GetSnapshot();
    }

    private static void DelayWithSpace()
    {
        Console.WriteLine("\n\n--- Separating Updates ---\n\n");
        Thread.Sleep(1000);
    }
}
