# todo

The base plateau in Python with the gRPC variation point attached: the `PlanItems` command is
served as `todo.v1.ItemsService/PlanItems`, with server reflection when switched on, and the
server stops gracefully on SIGTERM. Health endpoints come from the health variation point
(`vp-health-in-python`), not from gRPC.

```bash
make init       # once: a virtual environment with the package and its dev tools
make run        # serve on :9090 with reflection on
make proto-gen  # regenerate src/todo/gen/api/ after editing the proto file
make test       # every scenario
make lint       # the type check (mypy --strict)

grpcurl -plaintext -d '{"titles":["buy milk","walk the dog"]}' localhost:9090 todo.v1.ItemsService/PlanItems
```

| Setting | Default | Meaning |
|---|---|---|
| `GRPC_LISTEN_PORT` | `9090` | the port the API listens on |
| `GRPC_REFLECTION_ENABLED` | `false` | serve gRPC server reflection, for grpcurl and Postman |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/api/grpc` | the gRPC entry point: `services.py` (registration), `endpoint/` (one module per service), `middleware/` (the fixed call log and answer of failures, one package each), `pipeline/` (fixed code) |
| `src/todo/api/core/entry`, `src/todo/api/core/rpcstatus` | the runner of the entry points; the table from error kind to code |
| `src/todo/gen/api/todo` | the code generated from the contract by `make proto-gen`, committed |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |
| `docs/integration/api/todo/todo.proto` | the published contract, written by hand |

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
