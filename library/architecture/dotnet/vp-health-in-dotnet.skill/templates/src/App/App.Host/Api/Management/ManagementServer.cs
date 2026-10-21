// Source: skill vp-health-in-dotnet

using App.Host.Api.Hosting;
using App.Host.Version;
using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Management;

/// <summary>
/// One dependency the service waits for before it takes traffic: an
/// <see cref="IHealthCheck"/> - written for it, or one of the ready-made
/// checks (AspNetCore.HealthChecks.NpgSql, ...Redis, ...) - under the name a
/// failure is reported by. The management entry point has none of its own: a
/// service variation point adds its check in the composition root.
/// </summary>
public sealed record ReadinessCheck(string Name, IHealthCheck Check);

/// <summary>
/// The service's management entry point, copied from templates and never
/// edited: a small HTTP server on a port of its own that answers the
/// orchestrator's probes - /health/live, /health/ready, /health - whatever the
/// service's other entry points are (HTTP, gRPC, a listener, none). Kept off
/// the API port, it is never published through the ingress. It sends no command.
/// </summary>
public static class ManagementServer
{
    /// <summary>The category of every record of the management entry point.</summary>
    public const string LogCategory = "App.Host.Api.Management";

    /// <summary>
    /// The management entry point's one call from the composition root: reads
    /// the settings, builds the server, and serves until
    /// <paramref name="stopping"/> is cancelled. <paramref name="checks"/> are
    /// the readiness checks the service variation points add.
    /// </summary>
    public static Task RunAsync(ILoggerFactory loggers, CancellationToken stopping, params ReadinessCheck[] checks) =>
        ServeAsync(ManagementConfig.Load(), loggers, stopping, checks);

    /// <summary>Serves with the given settings; tests call it, the composition root calls <see cref="RunAsync"/>.</summary>
    public static Task ServeAsync(ManagementConfig config, ILoggerFactory loggers, CancellationToken stopping, params ReadinessCheck[] checks) =>
        WebHost.RunAsync(Build(config, AppVersion.Value, loggers, null, checks), "management", loggers.CreateLogger(LogCategory), stopping);

    /// <summary>
    /// Builds the application with the three health endpoints;
    /// <paramref name="version"/> is what /health reports, and
    /// <paramref name="configureHost"/> lets a test swap Kestrel for a test server.
    /// </summary>
    public static WebApplication Build(ManagementConfig config, string version, ILoggerFactory loggers, Action<IWebHostBuilder>? configureHost, params ReadinessCheck[] checks)
    {
        var builder = WebHost.CreateBuilder(config.ListenPort, loggers);
        configureHost?.Invoke(builder.WebHost);
        var app = builder.Build();
        var logger = loggers.CreateLogger(LogCategory);

        // Whether the process answers at all; it checks no dependency, so an
        // outage of a dependency never restarts the service.
        app.MapGet("/health/live", () => Results.Json(new { status = "ok" }));

        // 200 when every readiness check passes, 503 naming the checks that failed.
        app.MapGet("/health/ready", async (CancellationToken cancellationToken) =>
        {
            var failed = new List<string>();
            foreach (var check in checks)
            {
                if (!await PassesAsync(check, logger, cancellationToken))
                {
                    failed.Add(check.Name);
                }
            }
            return failed.Count == 0
                ? Results.Json(new { status = "ok" })
                : Results.Json(new { status = "unavailable", failed }, statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        // For people and clients, never for a probe: the status and the
        // version of the running binary.
        app.MapGet("/health", () => Results.Json(new { status = "ok", version }));
        return app;
    }

    // A check passes unless it reports Unhealthy or throws; Degraded still
    // takes traffic. A failure is logged with its reason.
    private static async Task<bool> PassesAsync(ReadinessCheck check, ILogger logger, CancellationToken cancellationToken)
    {
        var context = new HealthCheckContext { Registration = new HealthCheckRegistration(check.Name, check.Check, HealthStatus.Unhealthy, null) };
        try
        {
            var result = await check.Check.CheckHealthAsync(context, cancellationToken);
            if (result.Status != HealthStatus.Unhealthy)
            {
                return true;
            }
            logger.Write(LogLevel.Warning, "readiness check failed", result.Exception,
                LogAttr.Of("check", check.Name), LogAttr.Of("reason", result.Description));
        }
        catch (Exception error)
        {
            logger.Write(LogLevel.Warning, "readiness check failed", error, LogAttr.Of("check", check.Name));
        }
        return false;
    }
}
