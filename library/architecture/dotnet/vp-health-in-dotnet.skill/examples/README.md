# todo

The base plateau in dotnet with the health variation point attached: beside the startup command,
the management entry point answers `/health/live`, `/health/ready`, and `/health` on port 8081.
The startup command ends at once, and the runner stops the management server with it; with an
API variation point attached, both serve until SIGTERM.

```bash
make init    # once: the packages
make run     # build and run the startup command
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/App/App.Host/Api/Management` | the management entry point: health endpoints, readiness checks |
| `src/Modules/Todo/Todo.Interfaces` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
