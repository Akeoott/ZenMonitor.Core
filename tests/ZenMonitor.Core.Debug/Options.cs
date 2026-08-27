// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using CommandLine;

using Serilog.Events;

namespace ZenMonitor.Core.Debug;

internal class Options
{
    [Option('d', "dump", Required = true,
        HelpText = "Dump telemetry snapshot for a category\n(cpu, memory, gpu, system, drive, network, process, all)")]
    public required string DumpCategory { get; set; }

    [Option('v', "verbosity", HelpText = "Configure log level\n(verbose, debug, info, warn, error, fatal)")]
    public string? LogLevel { get; set; } = "info";
}
