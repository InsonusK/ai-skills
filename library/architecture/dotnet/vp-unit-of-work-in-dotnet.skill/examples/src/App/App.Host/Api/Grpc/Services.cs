using App.Host.Api.Core.Entry;
using App.Host.Api.Grpc.Endpoint;
using App.Host.Api.Grpc.Pipeline;
using Grpc.Core.Interceptors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc;

/// <summary>
/// The services of the proto contract this API serves, one per file of
/// Endpoint/. A new service adds a parameter here and a line each to the
/// constructor from the sender and to <see cref="Services"/>; a new rpc of an
/// existing service changes only that service's file.
/// </summary>
public sealed record GrpcEndpoints(ItemsHandler Items)
{
    /// <summary>Every service, sending its commands through <paramref name="sender"/>.</summary>
    public GrpcEndpoints(ISender sender)
        : this(Items: new ItemsHandler(sender))
    {
    }

    /// <summary>The services of the published contract (docs/integration/api/todo/todo.proto).</summary>
    public IReadOnlyList<GrpcService> Services =>
    [
        GrpcService.Of(Items),
    ];
}

/// <summary>The gRPC entry point as the composition root registers it.</summary>
public static class GrpcApi
{
    /// <summary>
    /// Registers the gRPC entry point: an <see cref="Entry"/> named "grpc" that
    /// serves every service over the modules' commands until the process
    /// stops, every call wrapped by <paramref name="interceptors"/> -
    /// <see cref="Interceptor"/> types built from the application's services,
    /// the first the outermost, inside the call log and the error answers (see
    /// <see cref="GrpcServer.Build"/>).
    /// </summary>
    public static IServiceCollection AddGrpcApi(this IServiceCollection services, params Type[] interceptors) =>
        services.AddSingleton(provider => new Entry("grpc", stopping => GrpcServer.RunAsync(
            new GrpcEndpoints(provider.GetRequiredService<ISender>()).Services,
            provider.GetRequiredService<ILoggerFactory>(),
            stopping,
            GrpcServer.Activate(provider, interceptors))));
}
