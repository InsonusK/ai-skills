# todo

The base plateau in Go with the gRPC client variation point attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`todo.v1.ItemsService/PlanItems` of
`docs/integration/client/todo/todo.proto`) through the service `RemotePlanner` and the client
`internal/infrastructure/client/todoapi`, which calls the code generated into `gen/client/todo` over the
fixed client code `internal/infrastructure/client/core/grpcclient`.

```bash
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_GRPC_PORT=9090 make run   # against the gRPC example of vp-grpc-api-in-go
make proto-gen                                        # regenerate gen/client/ after replacing a consumed proto
make test                                             # every scenario
```

| Setting | Default | Meaning |
|---|---|---|
| `TODO_API_HOST` | required | the host of the todo API; `mock` answers from its emulator inside the process |
| `TODO_API_GRPC_PORT` | `443` with TLS, `80` without | its port |
| `TODO_API_TLS` | `false` | connect over TLS |
| `TODO_API_TIMEOUT` | `5s` | the longest one call may take |
| `STARTUP_TITLES` | (none) | the titles to plan, comma-separated |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

| Outcome | Exit code |
|---|---|
| planned | `0` |
| the todo API rejected the titles (`INVALID_ARGUMENT`) | `2` |
| the todo API is unavailable, or answered anything else | `1` |
