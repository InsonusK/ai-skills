# todo

The base plateau in dotnet with the HTTP client and the client retry variation points attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlannerService` and the adapter
`TodoClient`, built on the fixed client code `src/Client/Core`; the client middleware
`src/Client/Middleware/Retry` sends a call of a retryable operation again while the API does not answer.

```bash
make init                                             # once: the packages
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run   # against the HTTP example of vp-http-api-in-dotnet
TODO_API_HOST=localhost TODO_API_HTTP_PORT=1 make run      # nothing listens: three attempts, two retried warnings
make test                                             # every scenario
```

| Setting | Default | Meaning |
|---|---|---|
| `TODO_API_HOST` | required | the host of the todo API; `mock` answers from its emulator inside the process |
| `TODO_API_HTTP_PORT` | `443` with TLS, `80` without | its port |
| `TODO_API_TLS` | `false` | call it over https |
| `TODO_API_TIMEOUT` | `5s` | the longest one attempt of a call may take |
| `TODO_API_RETRY_ATTEMPTS` | `3` | the attempts a retryable call gets in all, the first included |
| `TODO_API_RETRY_BACKOFF` | `100ms` | the wait before the second attempt, doubled before each next one, ±50% |
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
| `src/Client/Core/Client.Pipeline` | the client middleware run around the attempts of each call — fixed code, no packages |
| `src/Client/Middleware/Retry` | the retry of a call the API did not answer — fixed code |
| `src/Client/Core/Client.Http` | the HTTP call, its settings, log, and mock — fixed code |
| `src/Client/Todo/Todo.Client.Abstractions` | `ITodoClient`, what the service sees of the todo API |
| `src/Client/Todo/Todo.Client` | `TodoClient` and its mock `Mock/TodoMock.cs` |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
