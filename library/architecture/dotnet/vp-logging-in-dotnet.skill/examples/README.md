# todo

The base plateau in dotnet with the logging variation point attached: the same records as the
base plateau's, but with the payload of every call - the command's parameters here; a request's
query, headers, and body once an API is attached - written masked, cut to
`LOG_PAYLOAD_MAX_BYTES`, and with the `request_id` of the call.

```bash
STARTUP_TITLES="buy milk,walk the dog" make run
# {"level":"INFO","msg":"command called","command":"PlanItems","payload":{"titles":["buy milk","walk the dog"]}}
LOG_PAYLOAD=errors make run      # payloads of failed calls only
```

| Setting | Default | Meaning |
|---|---|---|
| `LOG_PAYLOAD` | `always` | `always`, `errors` (only records with `failed=true`), `off` |
| `LOG_PAYLOAD_MAX_BYTES` | `4096` | a payload longer than this, as JSON, is cut |
| `LOG_PAYLOAD_UNMASKED` | `false` | `true` writes payloads unmasked, to debug; only with `LOG_LEVEL=debug`, warned at start and once per request |

A body the mask cannot read - not JSON, or a proto `Any` of an unknown type - is written as
`<unparsed body, N bytes>`, and one `WARN` record `log payload not parsed` names where it was.
The scenarios of `src/MaskedLogging.Tests/features/` other than `Payload.feature` are copies of
the shared ones every stack runs; they are changed at their source, not here.

```bash
make init    # once: the packages
make run     # build and run the startup command
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/MaskedLogging` | the logging variation point's provider — fixed code |
| `src/Shared` | the error kinds and the log marks — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
