---
name: error-handling-strategy
description: How errors flow from the Client (HTTP/DTO layer) through the Facade to the caller (Signal Store method or NgRx effect)
problem: Whether errors should be communicated as a Result<T,E> discriminated union, as thrown/rejected raw errors, or as thrown/rejected typed domain errors
decision: The Client always catches transport-level errors and rethrows a typed domain error; it never lets a raw HttpErrorResponse escape. The Facade may add business context but preserves the throw/reject channel.
tags:
  - solution/api-http-layer
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Once an HTTP call fails, something has to decide how that failure is represented to the code that called the Facade — a feature's Signal Store method (via `try/catch`, per the "State management" solution's examples) or a global NgRx effect (via `catchError`, per the "State management" and "Аутентификация" solutions' examples). We need one convention that both existing call-site styles can use without rework, while giving callers something more useful than a raw, untyped HTTP error.

# Selected variant

**Selected variant:** [[#Client throws a typed domain error; Facade preserves the throw/reject channel]]

The Client is the only place a raw `HttpErrorResponse` is ever caught. It is always converted into a typed, per-feature domain error (e.g. `OrdersAddError`) before being rethrown. The Facade may catch that domain error to add business context (e.g. wrap a transport-level "conflict" into a more specific "order already submitted" error) but still communicates failure via throw/reject — it does not switch to a `Result<T,E>` return type.

# Searched variants

## Client throws a typed domain error; Facade preserves the throw/reject channel

### Description

`OrdersClient.addOrder()` wraps its `HttpClient` call; any `HttpErrorResponse` it catches is mapped to a typed `OrdersAddError` (a discriminated union or class hierarchy specific to that operation) and rethrown — never left as a raw `HttpErrorResponse`. `OrdersFacade.addOrder()` calls the Client; if it needs to add business-level context, it catches the domain error and rethrows an enriched version, still via throw. The Signal Store method (`try/catch`) or NgRx effect (`catchError`) at the call site handles the typed domain error exactly as it already does today.

### Benefits

- Compatible with both call-site styles already used elsewhere in the architecture (`try/catch` in Signal Store methods, `catchError` in NgRx effects) — no rework needed in the "State management" or "Аутентификация" solutions
- Callers get a strongly-typed, predictable error shape instead of a raw `HttpErrorResponse` with transport-specific fields (status codes, response bodies) leaking into feature/business code
- Keeps the door open for the future "Синхронизация offline-данных" solution to distinguish "offline, retry later" from "server rejected this request" using the same typed error shape
- Minimal conceptual overhead: still "throw an error," just always a well-defined one

### Costs

- TypeScript's type system does not force a caller to handle the thrown error (unlike `Result<T,E>`, where the discriminated union makes the error case impossible to ignore) — discipline is still required at each call site
- Every Client method needs an explicit mapping step from `HttpErrorResponse` to its domain error type, which is boilerplate that has to be written once per operation (though it can follow a small, repeatable pattern)

## Result<T, E> discriminated union

### Description

Every Client/Facade method returns `{ ok: true; value: T } | { ok: false; error: E }` instead of throwing; callers must check `.ok` before accessing `.value`.

### Benefits

- The type system makes it structurally impossible to use a result without having branched on success/failure first
- No unhandled promise rejections or silently-uncaught RxJS stream errors

### Costs

- Does not compose naturally with RxJS's `catchError`, which is built around the error channel — the NgRx effects already written in the "State management" and "Аутентификация" solutions would need to be rewritten to check `.ok` instead of using `catchError`
- Does not compose naturally with plain `try/catch` either — call sites would still need to unwrap the `Result` after the `await`, so the ergonomic benefit over a typed thrown error is smaller in an async/await-heavy codebase than in a purely synchronous one

## Client lets raw transport errors propagate unmapped

### Description

The Client does not catch or translate `HttpErrorResponse` at all; it propagates directly to the Facade and then to the caller.

### Benefits

- Zero mapping code to write per operation
- Full fidelity of the original HTTP error is preserved all the way to the caller, if that level of detail is ever needed

### Costs

- Feature and global-state code ends up branching on transport details (HTTP status codes, response body shape) instead of a meaningful domain concept, coupling business logic to the specific backend's error format
- Any change in the backend's error response shape becomes a breaking change for every caller that inspects it directly, instead of being absorbed once at the Client boundary
