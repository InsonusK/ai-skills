// Source: skill base-plateau-in-dotnet

using System.Text.Json;
using App.Host.Api.Core.Entry;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.Api.Startup;

/// <summary>
/// The base plateau's only entry point: the process runs one command when it
/// starts. Like every entry point, it knows the module's Commands project
/// (the command types) and <see cref="ISender"/> (the way to the handlers),
/// and nothing below them.
///
/// It exists only in the base plateau. When an API variation point is
/// attached (HTTP, gRPC, a listener), its registration replaces
/// <see cref="AddStartupEntry"/> in the "// [entries]" section of Program.cs
/// and this folder is deleted. The
/// exit code of the process is <see cref="Entry.EntryRunner"/>'s, which stays.
/// It stays only in an application whose whole job is one command per run (a
/// job started by a scheduler), or, rarely, beside a server that must run one
/// command once at start.
/// </summary>
public static class StartupEntry
{
    /// <summary>
    /// Registers the startup entry: an <see cref="Entry"/> named "startup" that
    /// sends the command <paramref name="command"/> builds - when the entry
    /// runs, so the setting it reads is read then - and writes its result to
    /// <paramref name="output"/> (see <see cref="RunAsync"/>).
    /// </summary>
    public static IServiceCollection AddStartupEntry<TResult>(this IServiceCollection services, Func<IRequest<TResult>> command, TextWriter output) =>
        services.AddSingleton(provider => new Entry("startup", stopping =>
            RunAsync(provider.GetRequiredService<ISender>(), command(), output, stopping)));

    /// <summary>
    /// Reads the comma-separated environment variable <paramref name="name"/>
    /// as a list - the way the startup command's input is given, since the
    /// process has no other input. Unset or empty is an empty list. The startup
    /// entry owns this setting, so it disappears together with the entry.
    /// </summary>
    public static IReadOnlyList<string> EnvList(string name)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(raw) ? [] : raw.Split(',');
    }

    /// <summary>
    /// Sends <paramref name="command"/> once and writes the result to
    /// <paramref name="output"/> as one JSON line. It works with any command:
    /// <typeparamref name="TResult"/> is the result type the command declares
    /// (<c>PlanItems : IRequest&lt;PlanItemsResult&gt;</c>). On error nothing
    /// is written and the exception propagates.
    /// </summary>
    public static async Task RunAsync<TResult>(ISender sender, IRequest<TResult> command, TextWriter output, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        await output.WriteLineAsync(JsonSerializer.Serialize(result, JsonSerializerOptions.Web));
    }
}
