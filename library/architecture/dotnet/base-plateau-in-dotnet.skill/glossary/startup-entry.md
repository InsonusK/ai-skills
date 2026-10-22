# Startup entry

The **startup entry** (`src/App/App.Host/Api/Startup`) is the base plateau's only entry point: the process sends one command when it starts, writes the result to standard output as one JSON line, and ends.

## How it works

1. `[entries]` builds the command from the setting the entry owns: `new PlanItemsCommand(StartupEntry.EnvList("STARTUP_TITLES"))`.
2. `StartupEntry.RunAsync(sender, command, stdout, stopping)` sends it through `ISender`, so every [pipeline behavior](./pipeline-behavior.md) runs.
3. On success it writes `{"items":[...]}` (camelCase, `JsonSerializerOptions.Web`); on error nothing is written and the exception goes to the [entry runner](./entry-runner.md), which sets the exit code.

| Outcome | Output | Exit code |
|---|---|:-:|
| handled | one JSON line | `0` |
| rejected (a kind other than unavailable) | nothing | `2` |
| failed (no kind, or unavailable) | nothing | `1` |

An API variation point replaces the startup `Entry` and deletes `Api/Startup`.

## Related concepts

- [Entry runner](./entry-runner.md) — turns the exception into the exit code.

## Sources

- System.Text.Json web defaults: https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions.web
