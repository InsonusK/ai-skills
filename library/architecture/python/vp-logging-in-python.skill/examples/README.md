# todo

The base plateau in Python with the logging variation point attached: the same records as the
base plateau's, but with the payload of every call - the command's parameters here; a
request's query, headers, and body once an API is attached - written masked, cut to
`LOG_PAYLOAD_MAX_BYTES`, and with the `request_id` of the call.

```bash
make run
# {"level":"INFO","msg":"command called","command":"PlanItems","payload":{"titles":["buy milk","walk the dog"]}}
LOG_PAYLOAD=errors make run      # payloads of failed calls only
```

| Setting | Default | Meaning |
|---|---|---|
| `LOG_PAYLOAD` | `always` | `always`, `errors` (only records with `failed=True`), `off` |
| `LOG_PAYLOAD_MAX_BYTES` | `4096` | a payload longer than this, as JSON, is cut |
| `LOG_PAYLOAD_UNMASKED` | `false` | `true` writes payloads unmasked, to debug; only with `LOG_LEVEL=debug`, warned at start and once per request |
| `LOG_LEVEL` | `info` | `debug`, `info`, `warn`, `error` |

A secret field is marked in its type - `pin: Annotated[str, "log:mask"]`, `avatar:
Annotated[bytes, "log:omit"]` - in a dataclass or a pydantic model; a field named like a secret
(`password`, `token`, ...) is masked though nobody marked it. A body the mask cannot read - not
JSON, or a proto `Any` of an unknown type - is written as `<unparsed body, N bytes>`, and one
`WARN` record `log payload not parsed` names where it was. The scenarios of
`src/todo/log/masked/features/` other than `python-payload.feature` are copies of the shared ones
every stack runs; they are changed at their source, not here.

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # run the startup command
make test    # every scenario
make lint    # the type check (mypy --strict)
```

| Package | Layer |
|---|---|
| `src/todo/main.py` | composition root |
| `src/todo/log/masked` | the logging variation point: the masked formatter, the mask, its settings |
| `src/todo/api/startup`, `src/todo/api/core/entry` | the startup entry; the runner of the entry points and the exit codes |
| `src/todo/command` | the commands, their results and handlers |
| `src/todo/command/pipeline` | the handler contract, the middleware, the error kinds re-exported — fixed code |
| `src/todo/domain/entity`, `src/todo/domain/service` | entities with the error kinds and rule errors; domain services |
| `src/todo/log` | the logger, its attributes, the request id — fixed code |
| `src/todo/config` | the base settings (`LOG_LEVEL`) |

Every package keeps its scenarios in `features/` and their steps in `test/` beside its code.
