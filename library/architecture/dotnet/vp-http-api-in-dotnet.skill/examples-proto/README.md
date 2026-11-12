# todo

The base plateau in dotnet with the gRPC API and its HTTP form attached: one proto contract
(`docs/integration/api/todo/todo.proto`) served over gRPC on :9090 and over HTTP on :8080, the
OpenAPI spec generated from the proto's `google.api.http` annotations by the same generator as the
Go catalog (`make proto-gen`). The example keeps lists of items in memory, so it shows every case
of the HTTP form:

| rpc | HTTP | Shows |
|---|---|---|
| `PlanItems` | `POST /v1/items/plan` | body `*`, 200 |
| `AddItem` | `POST /v1/lists/{listId}/items` | a path parameter, 201, 400, 409 |
| `ListItems` | `GET /v1/lists/{listId}/items` | a GET without a body, 404 |
| `ClearList` | `POST /v1/lists/{listId}/clear` | a deprecated method: 501 / `UNIMPLEMENTED` |

```bash
curl -X POST localhost:8080/v1/lists/home/items -H 'Content-Type: application/json' -d '{"title":"buy milk"}'   # 201
curl localhost:8080/v1/lists/home/items                                                                        # 200
grpcurl -plaintext -d '{"listId":"home"}' localhost:9090 todo.v1.ItemsService/ListItems
```

```bash
make init       # once: the packages
make run        # serve gRPC on :9090 and HTTP on :8080 (Swagger UI at /swagger/)
make proto-gen  # regenerate todo.swagger.json after editing the proto file
make test       # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/Grpc`, `Api/Http`), base settings |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers, and `Services/` — the item lists over the store |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/Infrastructure/Memory/Memory.Abstractions` | the store's interface, in its own terms |
| `src/Infrastructure/Memory/Memory` | the in-memory store; the DB variation point replaces it |
| `src/BuildingBlocks`, `src/Shared` | fixed code |
| `third_party/proto` | the googleapis and openapiv2 annotations the proto file imports |

Every `{Project}.Tests` sits beside the project it tests.
