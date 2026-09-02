---
description: Add libs/shared/http-core (base HTTP service with common concerns) and formalize the Facade/Client/Mapper structure inside every feature's data-access lib
element_kind: repository
change_kind: extend
tags:
  - solution/api-http-layer
  - element/repository
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
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and, if useful, the feature's domain error types) is part of the public API.
- When a feature has multiple distinct data facets, each facet's files MUST be grouped under `facade/`, `client/`, and `mapper/` with names `{feature}_N.{kind}.ts`; every Facade is exported from `index.ts`, but no Client or Mapper is exported.
- Every Client MUST build its HTTP calls on top of `libs/shared/http-core`'s base service, never call `HttpClient` directly.
- A Client MUST catch every `HttpErrorResponse` it can produce and rethrow a typed domain error from that feature's `{feature}.errors.ts`, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|error-handling-strategy]] — a raw `HttpErrorResponse` MUST NOT escape the Client.
- For feature-scoped operations, the calling Signal Store method MUST call the Facade directly — no Action/Reducer/Effect is introduced for feature-level data operations, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/facade-client-layering.md|facade-client-layering]]. This does not apply to global/cross-cutting state, which keeps its existing classical NgRx chain (Effect → Facade → Client) from the "State management" and `solution-authentication`s.

- a component or Signal Store method must never import a feature's Client directly, bypassing the Facade — business validation would be skipped.
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
