# todo

The base plateau in Go with the HTTP client variation point attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlanner` and the adapter
`internal/infrastructure/client/todoapi`, built on the fixed client code `internal/infrastructure/client/core/httpclient`.

```bash
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run   # against the HTTP example of vp-http-api-in-go
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
