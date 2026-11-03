// Source: skill base-plateau-in-dotnet

namespace App.Host.Api.Core.Entry;

/// <summary>
/// One entry point of the application.
/// </summary>
/// <param name="Name">How logs name the entry point, e.g. "http".</param>
/// <param name="RunAsync">
/// Serves until the token is cancelled and then returns (or throws
/// <see cref="OperationCanceledException"/>), or throws the error that stopped
/// it. A one-shot entry (the startup command) returns when its work is done.
/// </param>
/// <param name="ReportsErrors">
/// True for an entry that shows its own error to whoever called it - a CLI
/// prints "Error: ..." on stderr - so the runner does not log that error a
/// second time. The error still sets the exit code.
/// </param>
public sealed record Entry(string Name, Func<CancellationToken, Task> RunAsync, bool ReportsErrors = false);
