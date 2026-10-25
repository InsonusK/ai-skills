// Source: skill vp-http-api-in-dotnet

using App.Host.Api.Core.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Http.Pipeline;

/// <summary>
/// The fixed part of the HTTP entry point, copied from templates and never
/// edited: the settings, this server, the request log, the error answers, and
/// the docs. Like every entry point it knows only the modules' Commands and
/// <c>ISender</c> - never a handler or the domain. The routes are the
/// application's: <c>Api/Http/Routes.cs</c> passes them in as
/// <c>mapRoutes</c>, with the application's middlewares - the entry
/// decorators of skill base-plateau-extension, ASP.NET <see cref="IMiddleware"/>s
/// that do their step around the contract's routes and reject a request by
/// throwing an <c>ErrorKindException</c>, answered like any route's error.
/// </summary>
public static class HttpServer
{
    /// <summary>The category of every record of the HTTP entry point.</summary>
    public const string LogCategory = "App.Host.Api.Http";

    /// <summary>
    /// The HTTP entry point's one call from the composition root: reads the
    /// settings, builds the server, and serves until <paramref name="stopping"/>
    /// is cancelled. Returns after a graceful stop, or throws the error that
    /// stopped the entry point.
    /// </summary>
    public static Task RunAsync(
        Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers, CancellationToken stopping, IReadOnlyList<IMiddleware>? middlewares = null) =>
        ServeAsync(HttpConfig.Load(), mapRoutes, loggers, stopping, middlewares);

    /// <summary>Serves with the given settings; tests call it, the composition root calls <see cref="RunAsync"/>.</summary>
    public static Task ServeAsync(
        HttpConfig config, Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers, CancellationToken stopping,
        IReadOnlyList<IMiddleware>? middlewares = null) =>
        WebHost.RunAsync(Build(config, mapRoutes, loggers, middlewares: middlewares), "http", loggers.CreateLogger(LogCategory), stopping);

    /// <summary>
    /// Builds each of <paramref name="middlewares"/> - <see cref="IMiddleware"/>
    /// types, in the order given - from the application's services, so a
    /// middleware's constructor takes what the composition root registered
    /// (its settings, a logger factory).
    /// </summary>
    /// <exception cref="ArgumentException">A type that is no <see cref="IMiddleware"/>.</exception>
    public static IReadOnlyList<IMiddleware> Activate(IServiceProvider services, IEnumerable<Type> middlewares) =>
        middlewares.Select(type => typeof(IMiddleware).IsAssignableFrom(type)
            ? (IMiddleware)ActivatorUtilities.CreateInstance(services, type)
            : throw new ArgumentException($"{type.Name} is not an IMiddleware", nameof(middlewares))).ToList();

    /// <summary>
    /// Builds the application: the request log, then the error answers around
    /// every route, the contract routes - each wrapped by
    /// <paramref name="middlewares"/>, the first the outermost - and, when
    /// enabled, the docs outside the contract and outside the middlewares.
    /// <paramref name="configureHost"/> lets a test swap Kestrel for a test
    /// server.
    /// </summary>
    public static WebApplication Build(
        HttpConfig config, Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers,
        Action<IWebHostBuilder>? configureHost = null, IReadOnlyList<IMiddleware>? middlewares = null)
    {
        var builder = WebHost.CreateBuilder(config.ListenPort, loggers);
        // A body that cannot be bound throws, so ErrorAnswers answers it as
        // INVALID_ARGUMENT instead of ASP.NET's empty 400.
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        configureHost?.Invoke(builder.WebHost);

        var app = builder.Build();
        var logger = loggers.CreateLogger(LogCategory);
        app.UseMiddleware<RequestLog>(logger);
        app.UseMiddleware<ErrorAnswers>(logger);
        // Routing runs here, so each request knows its route before the
        // middlewares, which run only for a route of the contract.
        app.UseRouting();
        app.UseWhen(context => context.GetEndpoint()?.Metadata.GetMetadata<ContractRoute>() is not null, contract =>
        {
            foreach (var middleware in middlewares ?? [])
            {
                contract.Use((context, next) => middleware.InvokeAsync(context, next));
            }
        });
        mapRoutes(app.MapGroup("").WithMetadata(ContractRoute.Mark));
        if (config.DocsEnabled)
        {
            HttpDocs.Map(app, config.BasePath);
        }
        return app;
    }

    // The mark of every route of the contract, as Build maps them.
    private sealed class ContractRoute
    {
        public static readonly ContractRoute Mark = new();
    }
}
