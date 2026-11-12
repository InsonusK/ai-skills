---
name: stdlib logging
description: Which logging library the Python base plateau writes its records through — the standard library's logging with a formatter of its own, or structlog
problem: Every part logs through one logger; the base plateau must write one JSON line per record with the fields of the other stacks and drop the payload attribute, and the logging variation point must later mask it; which library does that?
decision: The standard library's logging - a Logger of its own built by new_logger, values as attributes through extra=attrs(...), a JsonFormatter writing the fields of every stack; the logging variation point replaces the formatter.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go base plateau writes `{"time","level","msg",...attributes}` and drops the `payload` attribute; dotnet writes the same fields. Libraries the variation points bring (uvicorn, grpc) log through the standard library whatever the application uses.

# Selected variant
[[#Standard library logging]]
- Owner decision, 2026-10-22.

# Searched variants

## Standard library logging

**Selected.**

### Description
`new_logger(stream, level)` returns a `logging.Logger` with one handler and `JsonFormatter`; a part writes `logger.info("command called", extra=attrs(command=name, payload=command))`; the formatter writes `time`, `level`, `msg`, the attributes but the payload, and the error.

### Benefits
- No package; the same model as `slog` in Go and the own provider in dotnet: the parts write attributes, the formatter decides.
- A library's records reach the same handler without a bridge.

### Costs
- About one hundred lines kept in the catalog; masking is written with the logging variation point.
- `extra=attrs(...)` is longer than structlog's keyword arguments.

## structlog

### Description
`structlog.get_logger().info("command called", command=name, payload=command)` with a JSON renderer.

### Benefits
- Keyword attributes; processors, one of which could mask the payload.

### Costs
- A dependency of the base for what one module does; the standard library's records of uvicorn and grpc still need a bridge into it.
