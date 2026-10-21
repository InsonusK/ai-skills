using App.Host.Api.Grpc.Endpoint;
using App.Host.Api.Grpc.Pipeline;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc;

/// <summary>
/// The services of the proto contract this API serves, one per file of
/// Endpoint/. The composition root fills them. A new service adds a parameter
/// here and a line to <see cref="Services"/>; a new rpc of an existing
/// service changes only that service's file.
/// </summary>
public sealed record GrpcEndpoints(ItemsHandler Items)
{
    /// <summary>The services of the published contract (docs/integration/api/todo/todo.proto).</summary>
    public IReadOnlyList<GrpcService> Services =>
    [
        GrpcService.Of(Items),
    ];
}

/// <summary>The gRPC entry point's one call from the composition root.</summary>
public static class GrpcApi
{
    /// <summary>Serves every service until <paramref name="stopping"/> is cancelled (see <see cref="GrpcServer.RunAsync"/>).</summary>
    public static Task RunAsync(GrpcEndpoints endpoints, ILoggerFactory loggers, CancellationToken stopping) =>
        GrpcServer.RunAsync(endpoints.Services, loggers, stopping);
}
