// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

using ZenMonitor.Core.Hosting;

namespace ZenMonitor.Core.Debug;

internal sealed class Program
{
    #region Init
    internal static async Task<int> Main(string[]? args)
    {
        LogEventLevel logLevel;
        if (args == null || args.Length == 0)
        {
            Console.WriteLine(
            """
            Select log level, pass either of these values...
                t | trace
                d | debug
                i | info
                w | warning
                e | error
                c | critical
            """);
            logLevel = LogEventLevel.Information;
        }
        else
        {
            logLevel = ParseSerilogLevel(args[0]);
        }

        const string logFilePath = "logs/ZenMonitor.Core.log";

        ConfigureLogging(logLevel, logFilePath);

        await using var serviceProvider = BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                logger.LogWarning("[Ctrl+C detected] Shutting down...");
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            var engine = serviceProvider.GetRequiredService<Monitor>();
            await engine.InitMonitor(1000, cts.Token);

            logger.LogInformation("Application finished, bye bye!");

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n[Ctrl+C detected] Shutting down... Bye bye!");
            return 0;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
    #endregion

    #region Dependency Injection
    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // All ZenMonitor platform services (Linux/Win detection, GPU auto-detect)
        services.AddZenMonitor();

        services.AddTransient<Monitor>();

        return services.BuildServiceProvider();
    }
    #endregion

    #region Logging Config
    private static void ConfigureLogging(LogEventLevel logLevel, string logFilePath)
    {
        Directory.CreateDirectory("logs");
        File.WriteAllText(logFilePath, string.Empty);

        var loggerConfig = new LoggerConfiguration().MinimumLevel.Is(logLevel);

        loggerConfig.WriteTo.File(
            logFilePath,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

        Log.Logger = loggerConfig.CreateLogger();
    }

    private static LogEventLevel ParseSerilogLevel(string? level)
    {
        return level?.ToLowerInvariant() switch
        {
            "t" or "trace" => LogEventLevel.Verbose,
            "d" or "debug" => LogEventLevel.Debug,
            "i" or "info" => LogEventLevel.Information,
            "w" or "warning" => LogEventLevel.Warning,
            "e" or "error" => LogEventLevel.Error,
            "c" or "critical" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
    #endregion
}

