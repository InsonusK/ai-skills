// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another file.

using System.Runtime.InteropServices;
using App.Host.Api.Core.Entry;
using App.Host.Api.Cli;
using App.Host.Config;
using App.Host.Version;
using BuildingBlocks.Logging;
using BuildingBlocks.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Middleware.Logging;
using Middleware.StoreFailures;
using Postgres;
using Todo.Application;

namespace App.Host;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // stopping is cancelled on SIGINT or SIGTERM, so a long-running entry
        // point (an HTTP server, a listener) stops gracefully.
        using var stopping = new CancellationTokenSource();
        void Stop(PosixSignalContext signal)
        {
            signal.Cancel = true; // the entry points stop, then Main returns the exit code
            stopping.Cancel();
        }
        using var onInterrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, Stop);
        using var onTerminate = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Stop);
        return await RunAsync(args, Console.Out, Console.Error, stopping.Token);
    }

    public static async Task<int> RunAsync(string[] args, TextWriter stdout, TextWriter stderr, CancellationToken stopping)
    {
        AppConfig config;
        PostgresConfig postgres;
        try
        {
            config = AppConfig.Load();
            postgres = PostgresConfig.Load();
        }
        catch (FormatException configError)
        {
            using var bootLogging = LoggerFactory.Create(logging => logging.AddProvider(new JsonLogProvider(stderr)));
            bootLogging.CreateLogger("App.Host").Write(LogLevel.Error, "startup failed", configError);
            return EntryRunner.ExitFailure;
        }

        var services = new ServiceCollection();
        // The provider decides what reaches the log: the base plateau's leaves
        // out every payload; the logging variation point replaces this line.
        services.AddLogging(logging => logging.SetMinimumLevel(config.LogLevel).AddProvider(new JsonLogProvider(stderr)));

        // [services] - infrastructure and the services over it. One context per
        // command scope, and the repository of every entity scoped with it.
        services.AddPostgres(postgres);

        // [handlers] - the handler of every command of each module, and the
        // command middleware wrapped around every one of them, the first the
        // outermost. A variation point adds its middleware to this list at
        // the place its skill names.
        services.AddTodoModule();
        services.AddCommandScope();
        services.AddCommandMiddleware(
            typeof(LoggingMiddleware<,>),
            typeof(StoreFailuresMiddleware<,>));   // after the logging: it logs the kind, and the store's failure as the cause

        // [entries] - one CLI action, then the process ends.
        services.AddCliEntry("todo", AppVersion.Value, args, stdout, stderr);

        // ValidateScopes: a scoped service taken outside a command scope stops the
        // call, instead of living as long as the process with its context.
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("App.Host");
        logger.Write(LogLevel.Information, "starting", LogAttr.Of("version", AppVersion.Value));
        if (postgres.MigrateOnStart)
        {
            try
            {
                await PostgresMigrations.ApplyAsync(postgres, stopping);
            }
            catch (Exception migrationError)
            {
                logger.Write(LogLevel.Error, "startup failed", migrationError);
                return EntryRunner.ExitFailure;
            }
        }
        var error = await EntryRunner.RunAllAsync(logger, stopping, [.. provider.GetServices<Entry>()]);
        return EntryRunner.ExitCode(error);
    }
}
