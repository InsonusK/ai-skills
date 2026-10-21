using App.Host.Api.Http.Endpoint;
using App.Host.Api.Http.Pipeline;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Http;

/// <summary>
/// The resources of the HTTP form of the proto contract, one per file of
/// Endpoint/. The composition root fills them. A new resource adds a parameter
/// here and a line to <see cref="Map"/>; a new rpc of an existing resource
/// changes only that resource's file.
/// </summary>
public sealed record HttpEndpoints(ItemsHandler Items, ListsHandler Lists)
{
    /// <summary>
    /// The operations of the published spec (docs/integration/api/todo/todo.swagger.json,
    /// generated from the proto file): the routes of every resource. The spec
    /// and the routes are kept in step by one scenario.
    /// </summary>
    public void Map(IEndpointRouteBuilder routes)
    {
        Items.Map(routes);
        Lists.Map(routes);
    }
}

/// <summary>The HTTP entry point's one call from the composition root.</summary>
public static class HttpApi
{
    /// <summary>Serves the routes of every resource until <paramref name="stopping"/> is cancelled (see <see cref="HttpServer.RunAsync"/>).</summary>
    public static Task RunAsync(HttpEndpoints endpoints, ILoggerFactory loggers, CancellationToken stopping) =>
        HttpServer.RunAsync(endpoints.Map, loggers, stopping);
}
