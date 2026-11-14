using App.Host.Api.Cli.Endpoint;
using App.Host.Api.Cli.Pipeline;
using App.Host.Api.Core.Entry;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Cli;

/// <summary>The resources of the command line, one property per resource file of Endpoint - the same resources as an HTTP API of the application would have. A new resource adds a property here and a line to <see cref="Resources"/>; a new action of an existing resource changes only that resource's file.</summary>
public sealed record CliEndpoints(ItemsHandler Items)
{
    /// <summary>The resources of the command line: `todo {resource} {action}`.</summary>
    public IReadOnlyList<Resource> Resources() => [Items.Resource()];
}

/// <summary>
/// The command-line entry point of the application as the composition root
/// lists it: it runs the action the arguments name over the command handlers
/// and ends, and the process ends with it. It shows its error itself
/// ("Error: ..." on stderr, ReportsErrors), so the runner does not log it a
/// second time; the exit code is EntryRunner's. The command line has no
/// middleware of its own: a step on every action is a command middleware.
/// </summary>
public static class Commands
{
    /// <summary>
    /// Registers the CLI entry point: an <see cref="Entry"/> named "cli" that
    /// runs the action <paramref name="args"/> name and ends. It replaces the
    /// startup entry of the base plateau.
    /// </summary>
    public static IServiceCollection AddCliEntry(this IServiceCollection services, string name, string version, string[] args, TextWriter stdout, TextWriter stderr) =>
        services.AddSingleton(provider => new Entry("cli", async stopping =>
        {
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("App.Host.Api.Cli");
            var endpoints = new CliEndpoints(new ItemsHandler(provider.GetRequiredService<ISender>()));
            var error = await Run(logger, name, version, args, endpoints, stdout, stderr, stopping);
            if (error is not null)
            {
                throw error;
            }
        }, ReportsErrors: true));

    /// <summary>Runs the action <paramref name="args"/> name over the resources and returns its error (see <see cref="Pipeline.Cli.RunAsync"/>); the scenarios call it, the composition root lists <see cref="AddCliEntry"/>.</summary>
    public static Task<Exception?> Run(ILogger logger, string name, string version, IReadOnlyList<string> args, CliEndpoints endpoints, TextWriter stdout, TextWriter stderr, CancellationToken stopping = default) =>
        Pipeline.Cli.RunAsync(logger, name, version, args, endpoints.Resources(), stdout, stderr, stopping);
}
