// Source: skill vp-grpc-api-in-dotnet

using App.Host.Api.Core.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc.Pipeline;

/// <summary>
/// The fixed part of the gRPC entry point, copied from templates and never
/// edited: the settings, this server, and one interceptor per step around
/// every call (CallLog, ErrorAnswers). Like every entry point it knows only
/// the modules' Commands and <c>ISender</c> - never a handler or the domain.
/// The services are the application's: <c>Api/Grpc/Services.cs</c> passes them in.
/// It serves no grpc.health.v1: the probes of every service go to the
/// management port's HTTP endpoints.
/// </summary>
public static class GrpcServer
{
    /// <summary>The category of every record of the gRPC entry point.</summary>
    public const string LogCategory = "App.Host.Api.Grpc";

    /// <summary>
    /// The gRPC entry point's one call from the composition root: reads the
    /// settings, builds the server, and serves until <paramref name="stopping"/>
    /// is cancelled - then stops accepting calls and waits up to 10 seconds for
    /// those in flight. Returns after the stop, or throws the error that stopped it.
    /// </summary>
    public static Task RunAsync(IReadOnlyList<GrpcService> services, ILoggerFactory loggers, CancellationToken stopping) =>
        ServeAsync(GrpcConfig.Load(), services, loggers, stopping);

    /// <summary>Serves with the given settings; tests call it, the composition root calls <see cref="RunAsync"/>.</summary>
    public static Task ServeAsync(GrpcConfig config, IReadOnlyList<GrpcService> services, ILoggerFactory loggers, CancellationToken stopping) =>
        WebHost.RunAsync(Build(config, services, loggers), "grpc", loggers.CreateLogger(LogCategory), stopping);

    /// <summary>
    /// Builds the application: the call log outside the error answers around
    /// every method, the contract's services, and, when enabled, reflection.
    /// <paramref name="configureHost"/> lets a test swap Kestrel for a test server.
    /// </summary>
    public static WebApplication Build(GrpcConfig config, IReadOnlyList<GrpcService> services, ILoggerFactory loggers, Action<IWebHostBuilder>? configureHost = null)
    {
        var builder = WebHost.CreateBuilder(config.ListenPort, loggers, HttpProtocols.Http2);
        var logger = loggers.CreateLogger(LogCategory);
        builder.Services.AddGrpc(grpc =>
        {
            // In this order: the call log sees the status ErrorAnswers chose.
            grpc.Interceptors.Add<CallLog>(logger);
            grpc.Interceptors.Add<ErrorAnswers>(logger);
        });
        foreach (var service in services)
        {
            builder.Services.AddSingleton(service.Type, service.Implementation);
        }
        if (config.ReflectionEnabled)
        {
            builder.Services.AddGrpcReflection();
        }
        configureHost?.Invoke(builder.WebHost);

        var app = builder.Build();
        foreach (var service in services)
        {
            service.Map(app);
        }
        if (config.ReflectionEnabled)
        {
            app.MapGrpcReflectionService();
        }
        return app;
    }
}
