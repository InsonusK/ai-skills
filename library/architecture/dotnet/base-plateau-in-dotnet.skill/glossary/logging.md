# Logging

Every part writes its records through `ILogger` with **fixed text and attributes** (`src/BuildingBlocks/Logging`); the one **provider** the composition root adds decides which attributes reach the output.

## How it works

```
LoggingMiddleware ─┐   logger.Write(Information, "command called",
entry points    ─┼─►     LogAttr.Of("command", "PlanItems"), LogAttr.Payload(command))
clients (later) ─┘                     │
                                       ▼
                 JsonLogProvider (base): {"time":…,"level":"INFO","msg":"command called","command":"PlanItems"}
                                         payload dropped
                 MaskedLogProvider (logging VP): the same line + payload masked + request_id
                 both write the line through JsonLogLine, so the fields are the same
```

| Piece | Role |
|---|---|
| `logger.Write(level, message, attrs...)` | one record: the message is fixed text, every value an attribute |
| `LogAttr.Of`, `LogAttr.Payload`, `LogAttr.Failed` | an attribute; the data of a call; whether the call failed |
| `JsonLogLine` | one record as one JSON line: `time`, `level`, `msg`, the attributes given, the error; every provider writes through it |
| `JsonLogProvider` | the base provider: every attribute but the payload, through `JsonLogLine` |
| `RequestId.Begin(id)` | the id of the call being served, carried along its async calls |

Each layer writes a pair of records, so the log reads top-down as the call ran: `command called` → `command handled` (with the result) or `command failed` (warning, with the command again).

## Example

```
{"time":"2026-10-21T19:07:09.606+00:00","level":"INFO","msg":"command called","command":"PlanItems"}
{"time":"2026-10-21T19:07:09.608+00:00","level":"INFO","msg":"command handled","command":"PlanItems","duration":1632200,"failed":false}
```

## Related concepts

- [Command middleware](./command-middleware.md) — `LoggingMiddleware` writes the command's pair.

## Sources

- Logging in .NET: https://learn.microsoft.com/dotnet/core/extensions/logging
- Implement a custom logging provider: https://learn.microsoft.com/dotnet/core/extensions/custom-logging-provider
