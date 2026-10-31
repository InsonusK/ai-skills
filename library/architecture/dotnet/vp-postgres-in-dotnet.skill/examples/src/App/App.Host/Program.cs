// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another file.

using System.Runtime.InteropServices;
using App.Host.Api.Core.Entry;
using App.Host.Api.Grpc;
using App.Host.Api.Http;
using App.Host.Config;
using App.Host.Version;
using BuildingBlocks.Logging;
using BuildingBlocks.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Middleware.Logging;
using Postgres;
using Postgres.Abstractions;
using Postgres.Stores;
using Todo.Application;

namespace App.Host;

public static class Program
{
    public static async Task<int> Main()
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
        return await RunAsync(Console.Out, Console.Error, stopping.Token);
    }

    public static async Task<int> RunAsync(TextWriter stdout, TextWriter stderr, CancellationToken stopping)
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
        // command scope, and every store over it scoped with it.
        services.AddPostgres(postgres);
        services.AddScoped<IListStore, PostgresListStore>();

        // [handlers] - the handler of every command of each module, and the
        // command middleware wrapped around every one of them, the first the
        // outermost. A variation point adds its middleware to this list at
        // the place its skill names.
        services.AddTodoModule();
        services.AddCommandScope();
        services.AddCommandMiddleware(
            typeof(LoggingMiddleware<,>));

        // [entries] - every entry point, registered as an Entry and run
        // together by EntryRunner until the first one ends. The gRPC variation
        // point replaced the startup entry with its own, the HTTP variation
        // point added the HTTP form of the same contract - both over the same
        // handlers; the health variation point adds one more.
        services.AddGrpcApi(interceptors: []);
        services.AddHttpApi(middlewares: []);

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
