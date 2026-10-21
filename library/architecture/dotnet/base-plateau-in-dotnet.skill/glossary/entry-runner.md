# Entry runner

The **entry runner** (`src/App/App.Host/Api/Entry`) starts every entry point of the application together — the startup command, an HTTP server, a gRPC server, the health endpoints — and turns how they ended into the process exit code. It is why each variation point attaches with one `Entry` in `// [entries]`.

## Why it exists

`await app.RunAsync(stopping)` of an HTTP server returns only when the process is told to stop. Awaited one after another, the second entry point would never start. Starting both and stopping the others when one fails, written by each variation point, would make every pair of variation points its own code. Written once in the base, each variation point only adds an item to a list.

## How it works

1. `[entries]` passes `RunAllAsync` one `Entry(Name, RunAsync, ReportsErrors)` per entry point.
2. `RunAllAsync` starts every `RunAsync` at once, each with the same token.
3. When the **first** one ends — a one-shot command done, a server that failed, or every server after SIGTERM — it cancels the token, so the others stop gracefully, and waits for them. An `OperationCanceledException` after that is a stop, not an error.
4. It returns the first exception any entry ended with, or `null`, and logs each failure with the entry's name (unless the entry `ReportsErrors` itself).
5. `ExitCode(error)` turns it into the exit code by the [error kind](./error-kind.md).

| Situation | Result | Exit code |
|---|---|:-:|
| startup command done | the others are stopped | `0` |
| SIGTERM | every entry stops gracefully | `0` |
| a server cannot start (port taken) | the others are stopped | `1` |
| startup command rejected (a kind other than unavailable) | the others are stopped | `2` |

## Example

```csharp
// [entries] - a service with HTTP and health: each variation point brought one Entry.
var error = await EntryRunner.RunAllAsync(logger, stopping,
    new Entry("http", stopping => HttpApi.RunAsync(httpEndpoints, loggers, stopping)),
    new Entry("management", stopping => ManagementServer.RunAsync(loggers, stopping)));
return EntryRunner.ExitCode(error);
```

## Related concepts

- [Composition root](./composition-root.md) — where the list is written.
- [Startup entry](./startup-entry.md) — the base plateau's one entry, replaced by an API variation point.

## Sources

- Task cancellation: https://learn.microsoft.com/dotnet/standard/parallel-programming/task-cancellation
- `Task.WhenAny`: https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.whenany
