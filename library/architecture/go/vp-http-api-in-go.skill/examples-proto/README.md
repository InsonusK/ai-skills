# todo

The base plateau in Go with the gRPC API and its HTTP form attached: one proto contract
(`docs/integration/api/todo/todo.proto`) served over gRPC on :9090 and over HTTP on :8080, the
OpenAPI spec generated from the proto's `google.api.http` annotations. The example keeps lists
of items in memory, so it shows every case of the HTTP form:

| rpc | HTTP | Shows |
|---|---|---|
| `PlanItems` | `POST /v1/items/plan` | body `*`, 200 |
| `AddItem` | `POST /v1/lists/{listId}/items` | a path parameter, 201, 400, 409 |
| `ListItems` | `GET /v1/lists/{listId}/items` | a GET without a body, 404 |
| `ClearList` | `POST /v1/lists/{listId}/clear` | a deprecated method: 501 / `UNIMPLEMENTED` |

```bash
curl -X POST localhost:8080/v1/lists/home/items -d '{"title":"buy milk"}'   # 201
curl localhost:8080/v1/lists/home/items                                    # 200
grpcurl -plaintext -d '{"listId":"home"}' localhost:9090 todo.v1.ItemsService/ListItems
```

```bash
make init       # once: module dependencies
make run        # build and serve gRPC on :9090 and HTTP on :8080 (Swagger UI at /swagger/)
make proto-gen  # regenerate gen/api/ and todo.swagger.json after editing the proto file
make test       # every scenario
```
