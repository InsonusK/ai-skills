// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another file.

using System.Runtime.InteropServices;
using Access;
using App.Host.Api.Core.Entry;
using App.Host.Api.Http;
using App.Host.Api.Http.Access;
using App.Host.Config;
using App.Host.Version;
using BuildingBlocks.Logging;
using BuildingBlocks.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        AccessConfig accessConfig;
        try
        {
            config = AppConfig.Load();
            accessConfig = AccessConfig.Load(); // the access variation point's own settings
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

        // [services] - infrastructure and the services over it; none in the base
        // plateau. The access variation point's settings, for its decorators
        // and its middleware: the callers' tokens and the batch limit.
        services.AddSingleton(accessConfig);

        // [handlers] - the handler of every command of each module, and the
        // decorators wrapped around every one of them, the first the
        // outermost. A variation point adds its decorator to this list at the
        // place its skill names.
        services.AddTodoModule();
        services.AddCommandDecorators(
            typeof(LoggingBehavior<,>),
            typeof(AccessCheckBehavior<,>), // by the command's mark: IRestricted
            typeof(BatchLimitBehavior<,>)); // by a value of the command: IBatch

        // [entries] - every entry point, registered as an Entry and run
        // together by EntryRunner until the first one ends. The HTTP variation
        // point replaced the startup entry with its own; the health variation
        // point adds one.
        // The HTTP API: its contract routes wrapped by these middlewares, the
        // first the outermost, inside the request log.
        services.AddHttpApi(middlewares: [typeof(Authentication)]);

        await using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("App.Host");
        logger.Write(LogLevel.Information, "starting", LogAttr.Of("version", AppVersion.Value));
        var error = await EntryRunner.RunAllAsync(logger, stopping, [.. provider.GetServices<Entry>()]);
        return EntryRunner.ExitCode(error);
    }
}
