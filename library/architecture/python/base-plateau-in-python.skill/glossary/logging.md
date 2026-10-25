# Logging

Every part writes its records through the **one logger** the composition root builds (`log`), with fixed text and its values as **attributes**; the **formatter** of that logger decides which attributes reach the output.

## How it works

```
LoggingMiddleware ┐   logger.info("command called",
entry points    ─┼─►     extra=attrs(command="PlanItems", payload=command))
clients (later) ─┘                     │
                                       ▼
                 JsonFormatter (base): {"time":…,"level":"INFO","msg":"command called","command":"PlanItems"}
                                       payload dropped
                 logging VP formatter: the same line + payload masked + request_id
```

| Piece | Role |
|---|---|
| `attrs(**values)` | the `extra=` of a record: its attributes, in order, kept apart from the fields of `LogRecord` |
| `PAYLOAD_KEY`, `FAILED_KEY` | the data of a call; whether the call failed |
| `JsonFormatter` | the base formatter: `time`, `level` (`DEBUG`, `INFO`, `WARN`, `ERROR`), `msg`, the attributes but the payload, `error` and — with no kind — `exception` with the stack; a duration in nanoseconds |
| `new_logger(stream, level)` | the application's one `logging.Logger`, not a child of the root logger, writing to standard error |
| `request_id_scope(id)`, `request_id()` | the id of the call being served, in a `contextvars.ContextVar`, so each asyncio task has its own |

Each layer writes a pair of records, so the log reads top-down as the call ran: `command called` → `command handled` (with the result) or `command failed` (warning, with the command again).

## Example

```
{"time":"2026-10-22T19:55:44.694+00:00","level":"INFO","msg":"command called","command":"PlanItems"}
{"time":"2026-10-22T19:55:44.694+00:00","level":"INFO","msg":"command handled","command":"PlanItems","duration":16000,"failed":false}
```

## Related concepts

- [Handler contract](./handler-contract.md) — `LoggingMiddleware` writes the command's pair.

## Sources

- `logging` — Using LoggerAdapters and `extra`: https://docs.python.org/3/howto/logging-cookbook.html#using-loggeradapters-to-impart-contextual-information
- `contextvars`: https://docs.python.org/3/library/contextvars.html
