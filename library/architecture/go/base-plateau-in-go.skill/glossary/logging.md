# Logging

**Logging** in the base plateau is one logger, built in `main.go`, that every part writes its records through with the same keys. What a record shows is decided by one `slog.Handler`, never by the part that writes it — so attaching the logging variation point changes the output, not the code.

## Why it exists

Three kinds of part log: the command decorator (a command was called, handled, failed), each entry point (an HTTP request, a gRPC call, a CLI run arrived), and later each client (a call went out). If each formatted its own data, no one place could mask a password in all of them, and the records of one request could not be tied together.

## Who writes what

| Part | Record | Attributes | Payload |
|---|---|---|---|
| `pipeline.WithLogging` | `command called` (INFO) | `command` | the command |
| | `command handled` (INFO) / `command failed` (WARN) | `command`, `duration`, `failed`, `error` | the command, on failure |
| HTTP entry point | `http request` (INFO) | `method`, `path`, `status`, `duration`, `failed` | `query`, `headers`, `body` |
| gRPC entry point | `grpc call` (INFO) | `method`, `code`, `duration`, `failed` | the request message |
| CLI entry point | `cli call` (INFO) | `action`, `duration`, `failed` | the flags given |

Every entry point also puts a **request id** into the context (`logging.WithRequestID`): the client's (`X-Request-Id`, `x-request-id`) or a new one, sent back in the response.

## How it works

```mermaid
flowchart LR
    P["decorator / entry point / client"] -->|"logger.LogAttrs(ctx, ..., logging.Payload(v), logging.Failed(..))"| L["*slog.Logger"]
    L --> H{"slog.Handler built in main.go"}
    H -->|"logging.NewHandler (base)"| B["JSON line: who, what, how long, how it ended<br/>payload dropped"]
    H -->|"logging variation point"| V["JSON line + payload masked<br/>+ request_id"]
```

The base handler drops the payload so that no data — and no secret in it — is ever written by a service that did not attach the logging variation point, which writes the payload only after masking it.

## How it is structured

| Part | Role |
|---|---|
| `logging.NewHandler(w, level)` | the base handler: JSON lines, payload dropped |
| `logging.Payload(v)`, `logging.PayloadKey` | the one attribute the data of a call goes in |
| `logging.Failed(bool)`, `logging.FailedKey` | whether the call ended with an error |
| `logging.WithRequestID`, `RequestID`, `NewRequestID` | the request id in the context |

## Example

```json
{"level":"INFO","msg":"command called","command":"PlanItems"}
{"level":"INFO","msg":"command handled","command":"PlanItems","duration":1253,"failed":false}
{"level":"INFO","msg":"http request","method":"POST","path":"/v1/items/plan","status":200,"duration":2101,"failed":false}
```

## Related concepts

- [Handler decorator](./handler-decorator.md) — `WithLogging` writes the command's records.
- [Composition root](./composition-root.md) — builds the one logger.

## Sources

- `log/slog`, handlers and `ReplaceAttr`: https://pkg.go.dev/log/slog
