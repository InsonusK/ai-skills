// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another file.

using System.Runtime.InteropServices;
using App.Host.Api.Core.Entry;
using App.Host.Api.Grpc;
using App.Host.Config;
using App.Host.Version;
using BuildingBlocks.Logging;
using BuildingBlocks.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Middleware.Logging;
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
        try
        {
            config = AppConfig.Load();
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

        // [services] - infrastructure and the services over it; none in the base plateau.

        // [handlers] - the handler of every command of each module, and the
        // command middleware wrapped around every one of them, the first the
        // outermost. A variation point adds its middleware to this list at
        // the place its skill names.
        services.AddTodoModule();
        services.AddCommandMiddleware(
            typeof(LoggingMiddleware<,>));

        // [entries] - every entry point, registered as an Entry and run
        // together by EntryRunner until the first one ends. The gRPC variation
        // point replaced the startup entry with its own; the health variation
        // point adds one.
        // The gRPC API: its calls wrapped by these interceptors, the first the
        // outermost, inside the call log.
        services.AddGrpcApi(interceptors: []);

        await using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("App.Host");
        logger.Write(LogLevel.Information, "starting", LogAttr.Of("version", AppVersion.Value));
        var error = await EntryRunner.RunAllAsync(logger, stopping, [.. provider.GetServices<Entry>()]);
        return EntryRunner.ExitCode(error);
    }
}
