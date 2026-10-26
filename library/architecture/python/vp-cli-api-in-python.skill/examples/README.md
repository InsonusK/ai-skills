# todo

The base plateau in Python with the CLI variation point attached: the `PlanItems` command is
the action `todo items plan`, which prints its result as one JSON line, or as text with
`--output text`, and exits with the code of its outcome.

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # todo items plan --title "buy milk" --title "walk the dog" --output text
make test    # every scenario
make lint    # the type check (mypy --strict)

.venv/bin/todo --help
.venv/bin/todo items plan --title "buy milk" --title "walk the dog"
```

| Exit code | When |
|---|---|
| `0` | the action ran |
| `2` | a usage error (an unknown action, a bad flag) or an error of a kind other than unavailable |
| `1` | unavailable, an error with no kind, an action that crashed |

| Setting | Default | Meaning |
|---|---|---|
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error`; the log goes to stderr |

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/api/cli` | the command-line entry point: `commands.py` (registration), `endpoint/` (one module per resource), `pipeline/` (fixed code) |
| `src/todo/api/core/entry` | the runner of the entry points and the exit codes |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
