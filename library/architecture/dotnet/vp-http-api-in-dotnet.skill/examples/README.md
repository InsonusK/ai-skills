# todo

The base plateau in dotnet with the HTTP API variation point attached: the PlanItems command is
served as `POST /v1/items/plan`, with Swagger UI. Health endpoints come from the health
variation point (`vp-health-in-dotnet`) on a port of their own.

```bash
curl -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' -d '{"titles":["buy milk","walk the dog"]}'
```

```bash
make init    # once: the packages
make run     # serve on :8080 (Swagger UI at /swagger/)
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/App/App.Host/Api/Http` | the HTTP entry point: `Pipeline/` (fixed), `Endpoint/` (one class per resource), `Routes.cs` |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |
| `docs/integration/api/todo/openapi.json` | the hand-written HTTP contract, embedded into `App.Host` |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
