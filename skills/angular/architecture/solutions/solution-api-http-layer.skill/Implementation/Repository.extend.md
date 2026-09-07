---
description: Add libs/shared/http-core (base HTTP service with common concerns) and formalize the Facade/Client/Mapper structure inside every feature's data-access lib
element_kind: repository
change_kind: extend
tags:
  - solution/api-http-layer
  - element/monolith-repository
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

### Multiple facets

When a feature's `data-access` lib has several distinct data facets, each facet keeps its own Facade/Client/Mapper trio and files are grouped by role under sub-folders:

```
libs/{feature}/data-access/src/lib
- facade/
  - {feature}_1.facade.ts
  - {feature}_2.facade.ts
- client/
  - {feature}_1.client.ts
  - {feature}_2.client.ts
- mapper/
  - {feature}_1.mapper.ts
  - {feature}_2.mapper.ts
```

`index.ts` still exports every Facade and public error types, and never exports any Client or Mapper.

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/http-core | Base HTTP service wrapping `HttpClient` with common concerns: base URL resolution, timeout, retry policy. Every feature's Client is built on top of this, not on raw `HttpClient` directly. Tagged `type:util`, `scope:shared`. |
| /libs/{feature}/data-access/src/lib/{feature}.facade.ts | Public API: business validation/orchestration, calls the Client, may enrich or coordinate across multiple Client calls. Exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/facade/{feature}_N.facade.ts | Same as above, used when a feature has multiple distinct data facets. Exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.client.ts | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error from `{feature}.errors.ts`. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/client/{feature}_N.client.ts | Same as above, used when a feature has multiple distinct data facets. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.mapper.ts | Internal: hand-written `dtoToModel`/`modelToDto` functions, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy.md|dto-mapping-strategy]]. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/mapper/{feature}_N.mapper.ts | Same as above, used when a feature has multiple distinct data facets. Never exported from `index.ts`. |
| /libs/{feature}/data-access/src/lib/{feature}.errors.ts | Domain error types for this feature's operations. The Facade may re-export these from `index.ts` so callers can narrow on them. |

# Nx tag taxonomy — extension

No new tag values are introduced; `libs/shared/http-core` uses the existing `type:util`/`scope:shared` combination. The `@nx/enforce-module-boundaries` allow-list from `solution-repository-structure` already permits `type:data-access` to depend on `type:util` with `scope:shared`, which covers this addition without further changes.

# Rules

## MUST
- A feature's `{feature}.client.ts` is never exported from that feature's `index.ts` — only the Facade (and its domain error types) is public.
  - Risk: an exported Client lets a consumer skip the Facade and its business validation.
  - Fix: `index.ts` re-exports the Facade + errors only; the Client is an internal file.
- A feature with multiple data facets groups files under `facade/`, `client/`, `mapper/` as `{feature}_N.{kind}.ts`; every Facade is exported, no Client or Mapper is.
  - Risk: a flat pile of `orders.client.ts`, `orders2.client.ts` … is unnavigable and blurs which Facade owns which Client.
  - Fix: one folder per layer, numbered files per facet, a barrel that exports only the Facades.
- Every Client builds its HTTP calls on `libs/shared/http-core`'s base service — never `HttpClient` directly.
  - Risk: base-URL, auth, and retry policy get re-implemented (inconsistently) per feature.
  - Fix: `inject(BaseHttpService)` and call its verbs.
- A Client catches every `HttpErrorResponse` it can produce and rethrows a typed domain error from `{feature}.errors.ts` — a raw `HttpErrorResponse` never escapes.
  - Risk: transport-shaped errors reach the store/component, which then branch on HTTP status codes far from the request.
  - Fix: `catchError` in the Client maps status → a `{Feature}...Error`; per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|error-handling-strategy]].
- For feature-scoped operations the calling Signal Store method calls the Facade directly — no Action/Reducer/Effect for feature-level data.
  - Risk: classical NgRx boilerplate for state only one feature ever reads, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/facade-client-layering.md|facade-client-layering]].
  - Fix: `store` method → `Facade` → `Client`. Global/cross-cutting state keeps its Effect → Facade → Client chain.
- A component or Signal Store method never imports a feature's Client directly, bypassing the Facade.
  - Risk: the Facade's business-rule validation is skipped entirely.
  - Fix: always route through the Facade; only the Facade constructs a Client call.
# Unittest TestCases

- [ ] WHEN a feature's `index.ts` is inspected THEN
  - [ ] it exports the Facade and domain error types only, never the Client or Mapper
- [ ] WHEN a Client method's HTTP call fails THEN
  - [ ] the Client rethrows a typed domain error, never the original `HttpErrorResponse`
- [ ] WHEN a feature-scoped operation is inspected THEN
  - [ ] no Action, Reducer, or Effect exists for it — only a Signal Store method calling the Facade

## SHOULD
- **A Signal Store method calling the feature's Client directly, skipping the Facade** — Consequence: bypasses business-rule validation the Facade exists to enforce, and duplicates that validation elsewhere or omits it entirely — Instead: the store always goes through the Facade; only the Facade calls the Client
- **A Client method letting a raw `HttpErrorResponse` propagate uncaught** — Consequence: feature/business code ends up branching on HTTP status codes instead of a meaningful domain error, coupling it to backend transport details (see [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|error-handling-strategy]]) — Instead: catch every possible transport error inside the Client and rethrow the feature's typed domain error
