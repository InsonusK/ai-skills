# todo

The base plateau in Go with the logging variation point attached: the same records as the base
plateau's, but with the payload of every call - the command's parameters here; a request's
query, headers, and body once an API is attached - written masked, cut to
`LOG_PAYLOAD_MAX_BYTES`, and with the `request_id` of the call.

```bash
STARTUP_TITLES="buy milk,walk the dog" make run
# {"level":"INFO","msg":"command called","command":"PlanItems","payload":{"Titles":["buy milk","walk the dog"]}}
LOG_PAYLOAD=errors make run      # payloads of failed calls only
```

| Setting | Default | Meaning |
|---|---|---|
| `LOG_PAYLOAD` | `always` | `always`, `errors` (only records with `failed=true`), `off` |
| `LOG_PAYLOAD_MAX_BYTES` | `4096` | a payload longer than this, as JSON, is cut |

```bash
make init    # once: module dependencies
make run     # build and run the startup command
make test    # every scenario
```
