# todo

The base plateau in Go with the HTTP client and the client retry variation points attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlanner` and the adapter
`internal/infrastructure/client/todoapi`, built on the fixed client code `internal/infrastructure/client/core/httpclient`; the client middleware
`internal/infrastructure/client/middleware/retry` sends a call of a retryable operation again while the API does
not answer.

```bash
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run   # against the HTTP example of vp-http-api-in-go
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
