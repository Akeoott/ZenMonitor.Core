// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

using CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

using ZenMonitor.Core.Hosting;

namespace ZenMonitor.Core.Debug;

internal static class Program
{
    internal static int Main(string[]? args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
        return 0;
    }

    private static void RunOptions(Options opts)
    {
        const string logTemplate = "[{Timestamp:HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        var loggerConfig = new LoggerConfiguration().MinimumLevel.Is(ParseSerilogLevel(opts.LogLevel));
        loggerConfig.WriteTo.Console(outputTemplate: logTemplate);
        Log.Logger = loggerConfig.CreateLogger();

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        services.AddZenMonitor();
        services.AddTransient<MethodExecuter>();

        using var serviceProvider = services.BuildServiceProvider();
        try
        {
            var executer = serviceProvider.GetRequiredService<MethodExecuter>();
            executer.Execute(opts);
            Log.Information("Application finished, bye bye!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        Log.Error("Arguments are invalid: {}", errs);
    }

    private static LogEventLevel ParseSerilogLevel(string? level)
    {
        return level?.ToLowerInvariant() switch
        {
            "v" or "verbose" => LogEventLevel.Verbose,
            "d" or "debug" => LogEventLevel.Debug,
            "i" or "info" or "information" => LogEventLevel.Information,
            "w" or "warn" or "warning" => LogEventLevel.Warning,
            "e" or "error" => LogEventLevel.Error,
            "f" or "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
