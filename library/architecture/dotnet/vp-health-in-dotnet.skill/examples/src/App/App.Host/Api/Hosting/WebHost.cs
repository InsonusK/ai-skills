// Source: skill vp-http-api-in-dotnet, vp-health-in-dotnet

using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Hosting;

/// <summary>
/// The web host every web entry point is built on - the HTTP API, the
/// management endpoints, later gRPC: one small <see cref="WebApplication"/>
/// per entry point, on a port of its own, run inside its Entry. Each variation
/// point that brings a web entry point adds this same file; the repository's
/// attach check fails when two of them differ.
/// </summary>
public static class WebHost
{
    /// <summary>
    /// A builder whose application listens on <paramref name="port"/>, logs
    /// through the application's <paramref name="loggers"/> (ASP.NET's own
    /// records from warning up only), and is started and stopped by the entry
    /// runner alone - not by a SIGTERM handler of its own.
    /// </summary>
    public static WebApplicationBuilder CreateBuilder(int port, ILoggerFactory loggers)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory,
        });
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.ListenAnyIP(port));
        builder.Services.AddSingleton<IHostLifetime, EntryLifetime>();
        builder.Services.Configure<HostOptions>(host => host.ShutdownTimeout = TimeSpan.FromSeconds(10));
        builder.Logging.ClearProviders()
            .AddProvider(new ForwardingLoggerProvider(loggers))
            .AddFilter("Microsoft", LogLevel.Warning)
            // A start that fails is logged once, by the entry runner.
            .AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
        return builder;
    }

    /// <summary>
    /// Starts <paramref name="app"/>, serves until <paramref name="stopping"/>
    /// is cancelled (SIGTERM, or another entry point ended), then stops
    /// accepting requests and waits up to 10 seconds for those in flight.
    /// Returns after a graceful stop, or throws the error that stopped it (a
    /// port already taken). <paramref name="name"/> names the server in the
    /// log: "http", "management".
    /// </summary>
    public static async Task RunAsync(WebApplication app, string name, ILogger logger, CancellationToken stopping)
    {
        await using (app)
        {
            await app.StartAsync(CancellationToken.None);
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
            logger.Write(LogLevel.Information, $"{name} server listening", LogAttr.Of("addr", string.Join(",", addresses)));
            try
            {
                await Task.Delay(Timeout.Infinite, stopping);
            }
            catch (OperationCanceledException)
            {
                // the stop the entry runner asked for
            }
            await app.StopAsync(CancellationToken.None);
            logger.Write(LogLevel.Information, $"{name} server stopped");
        }
    }

    // The entry runner owns the process lifetime: the host neither waits for
    // nor reacts to SIGTERM itself.
    private sealed class EntryLifetime : IHostLifetime
    {
        public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // Hands ASP.NET's loggers out of the application's factory, so they write
    // through the one provider of the composition root. The root owns it.
    private sealed class ForwardingLoggerProvider(ILoggerFactory loggers) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => loggers.CreateLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
