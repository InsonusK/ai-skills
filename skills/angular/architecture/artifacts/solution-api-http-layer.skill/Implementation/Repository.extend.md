---
description: Add libs/shared/http-core (base HTTP service with common concerns) and formalize the Facade/Client/Mapper structure inside every feature's data-access lib
element_kind: repository
change_kind: extend
---

# Structure

## Workspace Structure

```
/libs
  /shared
    /ui
    /util
    /state
    /http-core        <- new
  /{feature}
    /feature
    /data-access
      /src/lib
        {feature}.facade.ts     <- public API
        {feature}.client.ts     <- internal, not exported
        {feature}.mapper.ts     <- internal, not exported
        {feature}.errors.ts     <- internal, not exported (error types Facade may re-export)
      index.ts                  <- exports only the Facade and its public error types
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/http-core | Base HTTP service wrapping `HttpClient` with common concerns: base URL resolution, timeout, retry policy. Every feature's Client is built on top of this, not on raw `HttpClient` directly. Tagged `type:util`, `scope:shared`. |
| /libs/{feature}/data-access/src/lib/{feature}.facade.ts | Public API: business validation/orchestration, calls the Client, may enrich or coordinate across multiple Client calls. Exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.client.ts | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error from `{feature}.errors.ts`. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.mapper.ts | Internal: hand-written `dtoToModel`/`modelToDto` functions, per [[../adr/dto-mapping-strategy.md]]. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.errors.ts | Domain error types for this feature's operations. The Facade may re-export these from `index.ts` so callers can narrow on them. |

# Nx tag taxonomy — extension

No new tag values are introduced; `libs/shared/http-core` uses the existing `type:util`/`scope:shared` combination. The `@nx/enforce-module-boundaries` allow-list from solution #1 already permits `type:data-access` to depend on `type:util` with `scope:shared`, which covers this addition without further changes.

# Rules

## MUST
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and, if useful, the feature's domain error types) is part of the public API.
- Every Client MUST build its HTTP calls on top of `libs/shared/http-core`'s base service, never call `HttpClient` directly.
- A Client MUST catch every `HttpErrorResponse` it can produce and rethrow a typed domain error from that feature's `{feature}.errors.ts`, per [[../adr/error-handling-strategy.md]] — a raw `HttpErrorResponse` MUST NOT escape the Client.
- For feature-scoped operations, the calling Signal Store method MUST call the Facade directly — no Action/Reducer/Effect is introduced for feature-level data operations, per [[../adr/facade-client-layering.md]]. This does not apply to global/cross-cutting state, which keeps its existing classical NgRx chain (Effect → Facade → Client) from the "State management" and "Аутентификация" solutions.

## MUST NOT
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade — business validation would be skipped.

# Anti-patterns

- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce, and duplicates that validation elsewhere or omits it entirely
  - Instead: the store always goes through the Facade; only the Facade calls the Client

- **A Client method letting a raw `HttpErrorResponse` propagate uncaught**
  - Consequence: feature/business code ends up branching on HTTP status codes instead of a meaningful domain error, coupling it to backend transport details (see [[../adr/error-handling-strategy.md]])
  - Instead: catch every possible transport error inside the Client and rethrow the feature's typed domain error

# Unittest TestCases

- [ ] WHEN a feature's `index.ts` is inspected THEN
  - [ ] it exports the Facade and domain error types only, never the Client or Mapper
- [ ] WHEN a Client method's HTTP call fails THEN
  - [ ] the Client rethrows a typed domain error, never the original `HttpErrorResponse`
- [ ] WHEN a feature-scoped operation is inspected THEN
  - [ ] no Action, Reducer, or Effect exists for it — only a Signal Store method calling the Facade
