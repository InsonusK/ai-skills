---
name: project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade/Client/Mapper/Errors layering for one feature's HTTP access, offline-aware
domain: skill
type: template
plateau: offline-app
project_kind: library
version: 20260711140000
tags:
  - skill/template/project
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/data-access` project following this template. No solution produced a dedicated `{feature}.project.create.md` file for this tier; it is established implicitly by [[../repo-offline-app.skill.md|repo-offline-app]]'s directory table, then populated by `solution-api-http-layer`'s Facade/Client/Mapper/Errors layering.

# Goal

- Own one feature's HTTP access behind a single public Facade, with transport/mapping concerns fully hidden behind the Client
- Distinguish a genuine offline failure from a server-side rejection, and let the Facade decide whether a mutation is queued for later sync

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Core Principles

- Facade → Client → `libs/shared/http-core`, strictly layered, never skipped
- The Client is the only place a raw `HttpErrorResponse` is caught and turned into a typed domain error, including the shared `OfflineTransportError`
- The Facade is the only class exported from `index.ts`, alongside the feature's domain error types
- Queueing an offline mutation is an explicit Facade decision per operation, never automatic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Structure

## Project Structure

```
/libs/{feature}/data-access
  /src
    /lib
      [{feature}.facade.ts](./classes/class-feature-facade.skill.md)      <- public API
      [{feature}.client.ts](./classes/class-feature-client.skill.md)      <- internal, never exported
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/class-feature-mapper-and-errors.skill.md)   <- internal, never exported (errors may be re-exported)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| {feature}.facade.ts | Public API: business validation/orchestration, offline-aware mutation queueing decision | [[classes/class-feature-facade.skill.md\|class-feature-facade.skill]] |
| {feature}.client.ts | Internal: DTO mapping, HTTP call via `libs/shared/http-core`, typed domain errors incl. `OfflineTransportError` | [[classes/class-feature-client.skill.md\|class-feature-client.skill]] |
| {feature}.mapper.ts / {feature}.errors.ts | Internal: hand-written DTO↔model mapping functions and this feature's typed error hierarchy | [[classes/class-feature-mapper-and-errors.skill.md\|class-feature-mapper-and-errors.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## What Does NOT Belong Here

- Presentational components, routing, feature-level Signal Store state — belongs in this feature's own `feature` lib
- Any durable, persisted queue implementation — belongs in `libs/shared/offline-sync`; this lib only calls `MutationQueueService.enqueue(...)`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`)
- `libs/shared/offline-sync` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (read-only selectors, tag: `type:store`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Rules

## MUST
- [[../repo-offline-app.skill.md#MUST|repo-offline-app]]
- The Client MUST NOT be exported from `index.ts` — only the Facade (and the feature's domain error types) is public.
- The Facade MUST be the only class in this project exported from `index.ts`.
- A Facade MUST explicitly opt an operation into queueing by catching `OfflineTransportError` and calling `MutationQueueService.enqueue`.

## MUST NOT
- [[../repo-offline-app.skill.md#MUST NOT|repo-offline-app]]
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Anti-patterns

- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation, and skips the queueing decision the Facade owns
  - Instead: the store always goes through the Facade
- **Enqueueing an operation whose business validation already failed**
  - Consequence: queues a command that will never succeed, wasting a replay attempt and confusing the user with a "pending" state
  - Instead: business validation always runs and fails before any queueing decision is considered

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] Every Client method's HTTP calls go through `libs/shared/http-core`
- [ ] Every queueable operation's Facade method distinguishes "queued" from an immediate result in its return type

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]
