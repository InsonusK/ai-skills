// Source: skill vp-grpc-api-in-dotnet

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace App.Host.Api.Grpc.Pipeline;

/// <summary>
/// One service of the published contract as the server registers it: the
/// class that implements its generated base (<c>ItemsHandler :
/// ItemsService.ItemsServiceBase</c>), the instance the composition root
/// built, and how it is mapped.
/// </summary>
/// <param name="Type">The implementing class, e.g. <c>ItemsHandler</c>.</param>
/// <param name="Implementation">The instance every call of the service is served by.</param>
/// <param name="Map">Maps the service's methods on the server's routes.</param>
public sealed record GrpcService(Type Type, object Implementation, Action<IEndpointRouteBuilder> Map)
{
    /// <summary>
    /// The service <paramref name="implementation"/> serves.
    /// <typeparamref name="TService"/> is its class, e.g. <c>ItemsHandler</c>.
    /// </summary>
    public static GrpcService Of<TService>(TService implementation) where TService : class =>
        new(typeof(TService), implementation, routes => routes.MapGrpcService<TService>());
}
