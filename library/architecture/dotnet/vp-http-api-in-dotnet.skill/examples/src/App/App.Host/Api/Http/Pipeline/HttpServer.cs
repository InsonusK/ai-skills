// Source: skill vp-http-api-in-dotnet

using App.Host.Api.Hosting;
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
/// the docs. Like every entry point it knows only the modules' Interfaces and
/// <c>ISender</c> - never a handler or the domain. The routes are the
/// application's: <c>Api/Http/Routes.cs</c> passes them in as
/// <paramref name="mapRoutes"/>.
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
    public static Task RunAsync(Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers, CancellationToken stopping) =>
        ServeAsync(HttpConfig.Load(), mapRoutes, loggers, stopping);

    /// <summary>Serves with the given settings; tests call it, the composition root calls <see cref="RunAsync"/>.</summary>
    public static Task ServeAsync(HttpConfig config, Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers, CancellationToken stopping) =>
        WebHost.RunAsync(Build(config, mapRoutes, loggers), "http", loggers.CreateLogger(LogCategory), stopping);

    /// <summary>
    /// Builds the application: the request log, then the error answers around
    /// every route, the contract routes, and, when enabled, the docs outside
    /// the contract. <paramref name="configureHost"/> lets a test swap Kestrel
    /// for a test server.
    /// </summary>
    public static WebApplication Build(HttpConfig config, Action<IEndpointRouteBuilder> mapRoutes, ILoggerFactory loggers, Action<IWebHostBuilder>? configureHost = null)
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
        mapRoutes(app);
        if (config.DocsEnabled)
        {
            HttpDocs.Map(app, config.BasePath);
        }
        return app;
    }
}
