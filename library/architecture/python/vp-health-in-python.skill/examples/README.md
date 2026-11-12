# todo

The base plateau in Python with the health variation point attached: beside the startup command,
the management entry point answers `/health/live`, `/health/ready`, and `/health` on its own
port (`MANAGEMENT_LISTEN_PORT`, default 8081). The startup command is one-shot, so the process
still ends when the command is done, and the management server stops with it; in a service an
API entry point keeps the process - and the health endpoints - running.

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # run the startup command beside the health endpoints
make test    # every scenario
make lint    # the type check (mypy --strict)
```

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/api/management` | the health endpoints on the management port — fixed code, no dependency |
| `src/todo/api/startup`, `src/todo/api/core/entry` | the startup entry; the runner of the entry points and the exit codes |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |

| Setting | Default | Meaning |
|---|---|---|
| `MANAGEMENT_LISTEN_PORT` | `8081` | the port the probes call |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

```bash
curl localhost:8081/health/ready   # while a long-running entry point keeps the process up
```

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
