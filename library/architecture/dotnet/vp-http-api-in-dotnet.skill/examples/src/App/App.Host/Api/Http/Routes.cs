using App.Host.Api.Core.Entry;
using App.Host.Api.Http.Endpoint;
using App.Host.Api.Http.Pipeline;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Http;

/// <summary>
/// The resources this API serves, one per file of Endpoint/. A new resource
/// adds a parameter here and a line each to the constructor from the sender
/// and to <see cref="Map"/>; a new command of an existing resource changes
/// only that resource's file.
/// </summary>
public sealed record HttpEndpoints(ItemsHandler Items)
{
    /// <summary>Every resource, sending its commands through <paramref name="sender"/>.</summary>
    public HttpEndpoints(ISender sender)
        : this(Items: new ItemsHandler(sender))
    {
    }

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

/// <summary>The HTTP entry point as the composition root registers it.</summary>
public static class HttpApi
{
    /// <summary>
    /// Registers the HTTP entry point: an <see cref="Entry"/> named "http" that
    /// serves the routes of every resource over the modules' commands until
    /// the process stops, each route of the contract wrapped by
    /// <paramref name="middlewares"/> - <see cref="IMiddleware"/> types built
    /// from the application's services, the first the outermost, inside the
    /// request log and the error answers (see <see cref="HttpServer.Build"/>).
    /// </summary>
    public static IServiceCollection AddHttpApi(this IServiceCollection services, params Type[] middlewares) =>
        services.AddSingleton(provider => new Entry("http", stopping => HttpServer.RunAsync(
            new HttpEndpoints(provider.GetRequiredService<ISender>()).Map,
            provider.GetRequiredService<ILoggerFactory>(),
            stopping,
            HttpServer.Activate(provider, middlewares))));
}
