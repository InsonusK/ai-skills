---
name: exceptions carry the kind
description: How a dotnet command tells its caller why it failed — an exception with an error kind, or a Result type
problem: A dotnet handler must tell an entry point that a command was rejected and why (one of the error kinds), and a failure of the service must reach the same entry point too; should the rejection travel as an exception or as a Result value?
decision: An ErrorKindException carrying an ErrorKind; the handler signature returns the plain result, and every behavior and entry point catches one kind of thing. Result types are rejected.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go returns errors as values, and the Go base plateau wraps each with its kind. dotnet has two habits: exceptions, and a `Result<T>` (the previous dotnet plateau used Ardalis.Result). The base plateau needs one, because every pipeline behavior and every entry point is written against it.

# Selected variant
[[#Exception with a kind]]
- Owner decision, 2026-10-21, after weighing the cost of a throw (below).

# Searched variants

## Exception with a kind

**Selected.**

### Description
`ErrorKindException(ErrorKind kind, string message)`; errors are factory methods of a module's errors class; `ErrorKindException.KindOf(error)` reads the kind through inner exceptions. Handlers return `Task<TResult>`.

### Benefits
- One channel: a rule error, a database outage, and a bug arrive at an entry point the same way, so it maps one `ErrorKind?` (null = `INTERNAL`).
- A pipeline behavior (logging, later a unit of work rolling back) is a `try/catch` that works with any `TResult`.
- Entities and domain services stay plain: `throw` in a constructor, no propagation code in a loop over items.
- ASP.NET exception handlers and gRPC interceptors work with it as they are.

### Costs
- A throw with an async stack costs some 5–50 µs, against nanoseconds for a returned value. A rejected request happens at most once per request, which already costs milliseconds (a database round trip is 0.5–2 ms), so 1,000 rejected requests per second take a few milliseconds of CPU. A flood of invalid requests is answered by a rate limit (resource exhausted), not by the error type.
- A signature does not show that a call can fail.

## Result type

### Description
Every handler returns `Result<TResult>` (Ardalis.Result or an own type) with the kind as its status; exceptions remain for failures of the service.

### Benefits
- The signature shows the failure; a returned value costs nanoseconds.

### Costs
- Two channels: every entry point maps the `Result` statuses and, separately, the exceptions that still happen.
- A generic behavior needs `TResponse : IResult` and builds a failed `TResponse` by reflection (the previous dotnet plateau's `ExceptionHandlingBehavior` did).
- C# has no operator to pass a failure up, so every call of an entity or a domain service that can fail is followed by `if (result.IsFailure) return ...`.
