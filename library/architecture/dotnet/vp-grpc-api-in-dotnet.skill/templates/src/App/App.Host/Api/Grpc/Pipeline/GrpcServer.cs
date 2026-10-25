// Source: skill vp-grpc-api-in-dotnet

using App.Host.Api.Core.Hosting;
using App.Host.Api.Grpc.Middleware.CallLog;
using App.Host.Api.Grpc.Middleware.AnswerFailures;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc.Pipeline;

/// <summary>
/// The fixed part of the gRPC entry point, copied from templates and never
/// edited: the settings, this server, and one interceptor per step around
/// every call (CallLogMiddleware, AnswerFailuresMiddleware). Like every entry point it knows only
/// the modules' Commands and <c>ISender</c> - never a handler or the domain.
/// The services are the application's: <c>Api/Grpc/Services.cs</c> passes them
/// in, with the application's interceptors - the entry decorators of skill
/// base-plateau-extension, which do their step around every call and reject
/// one by throwing an <c>ErrorKindException</c>, answered by its kind.
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
    public static Task RunAsync(
        IReadOnlyList<GrpcService> services, ILoggerFactory loggers, CancellationToken stopping, IReadOnlyList<Interceptor>? interceptors = null) =>
        ServeAsync(GrpcConfig.Load(), services, loggers, stopping, interceptors);

    /// <summary>Serves with the given settings; tests call it, the composition root calls <see cref="RunAsync"/>.</summary>
    public static Task ServeAsync(
        GrpcConfig config, IReadOnlyList<GrpcService> services, ILoggerFactory loggers, CancellationToken stopping,
        IReadOnlyList<Interceptor>? interceptors = null) =>
        WebHost.RunAsync(Build(config, services, loggers, interceptors: interceptors), "grpc", loggers.CreateLogger(LogCategory), stopping);

    /// <summary>
    /// Builds each of <paramref name="interceptors"/> - <see cref="Interceptor"/>
    /// types, in the order given - from the application's services, so an
    /// interceptor's constructor takes what the composition root registered.
    /// </summary>
    /// <exception cref="ArgumentException">A type that is no <see cref="Interceptor"/>.</exception>
    public static IReadOnlyList<Interceptor> Activate(IServiceProvider services, IEnumerable<Type> interceptors) =>
        interceptors.Select(type => typeof(Interceptor).IsAssignableFrom(type)
            ? (Interceptor)ActivatorUtilities.CreateInstance(services, type)
            : throw new ArgumentException($"{type.Name} is not an Interceptor", nameof(interceptors))).ToList();

    /// <summary>
    /// Builds the application: the call log outside the error answers around
    /// every method, then <paramref name="interceptors"/>, the first the
    /// outermost, the contract's services, and, when enabled, reflection.
    /// <paramref name="configureHost"/> lets a test swap Kestrel for a test server.
    /// </summary>
    public static WebApplication Build(
        GrpcConfig config, IReadOnlyList<GrpcService> services, ILoggerFactory loggers,
        Action<IWebHostBuilder>? configureHost = null, IReadOnlyList<Interceptor>? interceptors = null)
    {
        var builder = WebHost.CreateBuilder(config.ListenPort, loggers, HttpProtocols.Http2);
        var logger = loggers.CreateLogger(LogCategory);
        builder.Services.AddGrpc(grpc =>
        {
            // In this order: the call log sees the status AnswerFailuresMiddleware chose,
            // and AnswerFailuresMiddleware answers what an interceptor of the application
            // throws.
            grpc.Interceptors.Add<CallLogMiddleware>(logger);
            grpc.Interceptors.Add<AnswerFailuresMiddleware>(logger);
            foreach (var interceptor in interceptors ?? [])
            {
                grpc.Interceptors.Add<ApplicationInterceptor>(interceptor);
            }
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

    // Hands every call, of each of the four kinds, to an interceptor the
    // application built, so gRPC runs that instance - two of one type stay
    // two - instead of building one of its type.
    private sealed class ApplicationInterceptor(Interceptor inner) : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation) =>
            inner.UnaryServerHandler(request, context, continuation);

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation) =>
            inner.ClientStreamingServerHandler(requestStream, context, continuation);

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation) =>
            inner.ServerStreamingServerHandler(request, responseStream, context, continuation);

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation) =>
            inner.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
    }
}
