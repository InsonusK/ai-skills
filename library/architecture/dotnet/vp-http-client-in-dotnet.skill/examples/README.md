# todo

The base plateau in dotnet with the HTTP client variation point attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlannerService` and the adapter
`TodoApiClient`, built on the fixed client code `src/Infrastructure/Client/Core`.

```bash
make init                                             # once: the packages
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run   # against the HTTP example of vp-http-api-in-dotnet
make test                                             # every scenario
```

| Setting | Default | Meaning |
|---|---|---|
| `TODO_API_HOST` | required | the host of the todo API; `mock` answers from its emulator inside the process |
| `TODO_API_HTTP_PORT` | `443` with TLS, `80` without | its port |
| `TODO_API_TLS` | `false` | call it over https |
| `TODO_API_TIMEOUT` | `5s` | the longest one call may take |
| `STARTUP_TITLES` | (none) | the titles to plan, comma-separated |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

| Outcome | Exit code |
|---|---|
| planned | `0` |
| the todo API rejected the titles (`INVALID_ARGUMENT`) | `2` |
| the todo API is unavailable, or answered anything else | `1` |

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers and the services (`RemotePlannerService`) |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/Infrastructure/Client/Core/Client.Abstractions` | the outcomes every client reports — fixed code, no packages |
| `src/Infrastructure/Client/Core/Client.Http` | the HTTP call, its settings, log, and mock — fixed code |
| `src/Infrastructure/Client/TodoApi/TodoApi.Abstractions` | `ITodoApi`, what the service sees of the todo API |
| `src/Infrastructure/Client/TodoApi/TodoApi` | `TodoApiClient` and its mock `Mock/TodoApiMock.cs` |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
