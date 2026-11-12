# todo

The base plateau in Python with the HTTP variation point attached: the HTTP API serves the
`PlanItems` command at `POST /v1/items/plan`, publishes its OpenAPI contract and Swagger UI, and
stops gracefully on SIGTERM.

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # serve on :8080, Swagger UI at http://localhost:8080/swagger/
make test    # every scenario
make lint    # the type check (mypy --strict)

curl -s -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' \
  -d '{"titles":["buy milk","walk the dog"]}'
```

| Setting | Default | Meaning |
|---|---|---|
| `HTTP_LISTEN_PORT` | `8080` | the port the API listens on |
| `HTTP_BASE_PATH` | (none) | the prefix the service is published under; every URL the docs emit carries it |
| `HTTP_API_DOCS_ENABLED` | `false` | serve `/openapi.json` and Swagger UI at `/swagger/` |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/api/http` | the HTTP entry point: `routes.py` (registration), `endpoint/` (one module per resource), `pipeline/` (fixed code) |
| `src/todo/api/core/entry`, `src/todo/api/core/rpcstatus` | the runner of the entry points; the table from error kind to code and status |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |
| `docs/integration/api/todo/openapi.json` | the published contract, served as written |

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
