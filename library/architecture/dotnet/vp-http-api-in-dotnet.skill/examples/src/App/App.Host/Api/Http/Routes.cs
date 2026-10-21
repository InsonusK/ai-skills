using App.Host.Api.Http.Endpoint;
using App.Host.Api.Http.Pipeline;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Http;

/// <summary>
/// The resources this API serves, one per file of Endpoint/. The composition
/// root fills them. A new resource adds a parameter here and a line to
/// <see cref="Map"/>; a new command of an existing resource changes only that
/// resource's file.
/// </summary>
public sealed record HttpEndpoints(ItemsHandler Items)
{
    /// <summary>
    /// The operations of the published contract (docs/integration/api/todo/openapi.json):
    /// the routes of every resource. The contract and the routes are kept in
    /// step by one scenario.
    /// </summary>
    public void Map(IEndpointRouteBuilder routes)
    {
        Items.Map(routes);
    }
}

/// <summary>The HTTP entry point's one call from the composition root.</summary>
public static class HttpApi
{
    /// <summary>Serves the routes of every resource until <paramref name="stopping"/> is cancelled (see <see cref="HttpServer.RunAsync"/>).</summary>
    public static Task RunAsync(HttpEndpoints endpoints, ILoggerFactory loggers, CancellationToken stopping) =>
        HttpServer.RunAsync(endpoints.Map, loggers, stopping);
}
