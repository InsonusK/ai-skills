# todo

The base plateau in dotnet with the gRPC API variation point attached: the PlanItems command is
served as `todo.v1.ItemsService/PlanItems` on port 9090, with server reflection when
`GRPC_REFLECTION_ENABLED=true`. Health endpoints come from the health variation point
(`vp-health-in-dotnet`) on a port of their own.

```bash
grpcurl -plaintext -d '{"titles":["buy milk","walk the dog"]}' localhost:9090 todo.v1.ItemsService/PlanItems
```

```bash
make init    # once: the packages
make run     # serve on :9090 with reflection
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/App/App.Host/Api/Grpc` | the gRPC entry point: `Pipeline/` (fixed), `Endpoint/` (one class per proto service), `Services.cs` |
| `src/Modules/Todo/Todo.Interfaces` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |
| `docs/integration/api/todo/todo.proto` | the gRPC contract; `Grpc.Tools` generates its C# at build time |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
