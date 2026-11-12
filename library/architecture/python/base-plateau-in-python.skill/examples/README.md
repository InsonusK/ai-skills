# todo

The base plateau in Python: the process starts, runs its one startup command — plan a list of
to-do items from `STARTUP_TITLES` — prints the result as JSON and exits.

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # run the startup command
make test    # every scenario
make lint    # the type check (mypy --strict)
```

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/api/startup`, `src/todo/api/core/entry` | the startup entry; the runner of the entry points and the exit codes |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
