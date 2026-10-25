---
name: fixed middleware inside the entry
description: Who builds an API's request log and answer-failures middleware, and where they stand - the entry point itself, from the logger the composition root gives it, outside every middleware of the composition root's list
problem: Every API logs each request it takes and answers each failure in its error format; the rest of its steps are middleware the composition root lists in order. Should the request log and the answer failures be two more elements of that list, built in the composition root, or should the entry point build them itself?
decision: Each API's entry point builds its fixed middleware itself from the logger it is given - the request log (HTTP) or the call log (gRPC) outermost, answer failures inside it - around everything the API serves; the composition root gives the entry only the logger and the list of the contract's middleware, which run inside the fixed ones and around the contract's routes only. The CLI keeps the same two steps: its run log and its answer of every failure, a panic included, as an error line and an exit code.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The command logging became a middleware like any other, listed first in the composition root. The same was asked of the API's request log and answer failures: list them in the composition root as `RequestLogMiddleware(logger)` and `AnswerFailuresMiddleware(logger)`, so the composition root shows every step and the entry no longer takes the logger for them.

# Selected variant
[[#Built by the entry, outside the list]]
- Owner decision, 2026-10-25: the request log and the answer failures stay fixed in the entry, built from its logger; recorded as this ADR, which every API variation point follows.

# Searched variants

## Built by the entry, outside the list

**Selected.**

### Description
The API's server builds the request log and the answer failures from the logger the entry factory receives and puts them around everything it serves - the contract's routes, the docs, an unknown path, a method a path does not serve. The middleware of the composition root's list run inside them and around the contract's routes only. Each lives in the API's `middleware/` directory, named like any middleware (`RequestLogMiddleware`, `AnswerFailuresMiddleware`; in Go the packages `requestlog`, `answerfailures`), and is copied from the API's templates.

```
composition root:  entry(handlers, logger, [AuthenticationMiddleware(...)])
the API's server:  request log ─▶ answer failures ─▶ [the list, contract routes only] ─▶ route
                   (built here from logger, around every request)
```

### Benefits
- The two guarantees of every API - each request is logged, each failure is answered in the API's error format (`google.rpc.Status`, a gRPC status, the CLI's error line and exit code) - hold for every application without a line of its composition root.
- No list can put an API middleware outside the request log, so a rejected request is always logged and answered like any other.
- The fixed middleware wrap what the list cannot reach: the docs, an unknown path, the framework's own answers (404, 405, a body that cannot be bound), a panic.

### Costs
- The composition root does not show the two fixed steps; the API's skill and its server's code do.
- The entry still takes the logger, which its server needs anyway for its own records ("http server listening", "stopped") and for the logs of the framework under it.

## Listed in the composition root, one list

### Description
`entry(handlers, logger, [RequestLogMiddleware(logger), AnswerFailuresMiddleware(logger), AuthenticationMiddleware(...)])`.

### Benefits
- Every step of a request visible in one list.

### Costs
- The list wraps the contract's routes only, so the docs, an unknown path, and the framework's own answers would go unlogged and in the framework's format.
- The guarantees depend on each application's list: a middleware placed first, or a fixed one left out, loses the log line or the error format.

## Listed in the composition root, two lists

### Description
`entry(handlers, logger, outer=[RequestLogMiddleware(logger), AnswerFailuresMiddleware(logger)], middlewares=[...])` - one list around everything, one around the contract.

### Benefits
- Every step visible; the two scopes kept apart.

### Costs
- The same two elements, in the same order, repeated in every application and every API; the guarantee still depends on them being there.
- In Go the answer by kind is in the endpoint and the server, in Python partly the framework's exception handlers: not every part of the answer is a middleware that could be listed.
