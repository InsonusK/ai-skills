# todo

The base plateau in Python with the HTTP client and the client retry variation points attached: the startup command
`PlanRemoteItems` plans its titles with another service's todo API (`POST /v1/items/plan` of
`docs/integration/client/todo/openapi.json`) through the service `RemotePlanner` and the client
`todo.infrastructure.client.todo_api`, built on the fixed client code `todo.infrastructure.client.core`
(`httpclient` on httpx2); the client middleware `todo.infrastructure.client.middleware.retry` sends a call of a
retryable operation again while the API does not answer.

```bash
make init                                             # once: the virtual environment
make run                                              # against the todo API's emulator (TODO_API_HOST=mock)
TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run   # against the HTTP example of vp-http-api-in-python
TODO_API_HOST=localhost TODO_API_HTTP_PORT=1 make run      # nothing listens: three attempts, two retried warnings
make test                                             # every scenario
make lint                                             # mypy --strict
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
