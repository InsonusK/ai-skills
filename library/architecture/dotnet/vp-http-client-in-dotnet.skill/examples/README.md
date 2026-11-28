# todo

The base plateau in dotnet with the HTTP client variation point attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlannerService` and the client
`TodoClient`, built on the fixed client code `src/Client/Core`.

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
| `src/Modules/Todo/Todo.Application` | command handlers and consumer-owned service interfaces |
| `src/Service/Todo/Todo.Service` | `RemotePlannerService`, adaptation of the external Todo system |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/Client/Core/Client.Abstractions` | the outcomes every client reports — fixed code, no packages |
| `src/Client/Core/Client.Http` | the HTTP call, its settings, log, and mock — fixed code |
| `src/Client/Todo/Todo.Client.Abstractions` | `ITodoClient`, what the service sees of the todo API |
| `src/Client/Todo/Todo.Client` | `TodoClient` and its mock `Mock/TodoMock.cs` |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
