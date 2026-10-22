// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another file.

using System.Runtime.InteropServices;
using App.Host.Api.Core.Entry;
using App.Host.Api.Grpc;
using App.Host.Api.Http;
using GrpcEndpoint = App.Host.Api.Grpc.Endpoint;
using HttpEndpoint = App.Host.Api.Http.Endpoint;
using App.Host.Config;
using App.Host.Version;
using BuildingBlocks.Logging;
using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Memory;
using Memory.Abstractions;
using Todo.Application.Commands;
using Todo.Application.Services;

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

        // [services] - infrastructure and the services over it. The lists live in
        // memory in this example; the DB variation point replaces these two lines.
        services.AddSingleton<IListStore, MemoryListStore>();
        services.AddSingleton<IItemLists, ItemListsService>();

        // [handlers] - every command handler of the modules, and the pipeline
        // behaviors that wrap each of them, outermost first.
        services.AddMediatR(mediatr =>
        {
            mediatr.RegisterServicesFromAssemblyContaining<PlanItemsHandler>();
            mediatr.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        await using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("App.Host");
        logger.Write(LogLevel.Information, "starting", LogAttr.Of("version", AppVersion.Value));
        var sender = provider.GetRequiredService<ISender>();

        // [entries] - every entry point, one Entry each, run together by
        // EntryRunner until the first one ends. The gRPC variation point
        // replaced the startup Entry with its own, the HTTP variation point
        // added the HTTP form of the same contract - both over the same
        // handlers; the health variation point adds one more.
        var loggers = provider.GetRequiredService<ILoggerFactory>();
        var grpcEndpoints = new GrpcEndpoints(Items: new GrpcEndpoint.ItemsHandler(sender));
        var httpEndpoints = new HttpEndpoints(Items: new HttpEndpoint.ItemsHandler(sender), Lists: new HttpEndpoint.ListsHandler(sender));
        var error = await EntryRunner.RunAllAsync(logger, stopping,
            new Entry("grpc", stopping => GrpcApi.RunAsync(grpcEndpoints, loggers, stopping)),
            new Entry("http", stopping => HttpApi.RunAsync(httpEndpoints, loggers, stopping)));
        return EntryRunner.ExitCode(error);
    }
}
