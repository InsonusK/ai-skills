# Startup entry

The **startup entry** (`src/App/App.Host/Api/Startup`) is the base plateau's only entry point: the process sends one command when it starts, writes the result to standard output as one JSON line, and ends.

## How it works

1. `[entries]` registers it with the command to send, built from the setting the entry owns when the entry runs: `services.AddStartupEntry(() => new PlanItemsCommand(StartupEntry.EnvList("STARTUP_TITLES")), stdout)`.
2. Its `Entry` calls `StartupEntry.RunAsync(sender, command, stdout, stopping)`, which sends the command through `ISender`, so every [pipeline behavior](./pipeline-behavior.md) runs.
3. On success it writes `{"items":[...]}` (camelCase, `JsonSerializerOptions.Web`); on error nothing is written and the exception goes to the [entry runner](./entry-runner.md), which sets the exit code.

| Outcome | Output | Exit code |
|---|---|:-:|
| handled | one JSON line | `0` |
| rejected (a kind other than unavailable) | nothing | `2` |
| failed (no kind, or unavailable) | nothing | `1` |

An API variation point replaces `AddStartupEntry` with its own registration and deletes `Api/Startup`.

## Related concepts

- [Entry runner](./entry-runner.md) — turns the exception into the exit code.

## Sources

- System.Text.Json web defaults: https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions.web
