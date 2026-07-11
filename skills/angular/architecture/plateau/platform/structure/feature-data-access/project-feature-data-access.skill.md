---
name: project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade/Client/Mapper/Errors layering for one feature's HTTP access, offline-aware
domain: skill
type: template
plateau: platform
project_kind: library
version: 20260711150000
tags:
  - skill/template/project
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

> Generic pattern, not tied to one concrete feature. No solution produced a dedicated `{feature}.project.create.md` file for this tier; it is established implicitly by [[../repo-platform.skill.md|repo-platform]]'s directory table.

# Goal

- Own one feature's HTTP access behind a single public Facade, with transport/mapping concerns fully hidden behind the Client
- Distinguish a genuine offline failure from a server-side rejection, and let the Facade decide whether a mutation is queued for later sync

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Core Principles

- Facade → Client → `libs/shared/http-core`, strictly layered, never skipped
- The Client is the only place a raw `HttpErrorResponse` is caught and turned into a typed domain error, including `OfflineTransportError`
- The Facade is the only class exported from `index.ts`
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
      [{feature}.facade.ts](./classes/class-feature-facade.skill.md)
      [{feature}.client.ts](./classes/class-feature-client.skill.md)
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/class-feature-mapper-and-errors.skill.md)
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
- Any durable, persisted queue implementation — belongs in `libs/shared/offline-sync`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

## Allowed Dependencies

- `libs/shared/http-core`, `libs/shared/offline-sync`, `libs/shared/state` (read-only selectors), `libs/shared/util`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Rules

## MUST
- [[../repo-platform.skill.md#MUST|repo-platform]]
- The Client MUST NOT be exported from `index.ts`.
- The Facade MUST be the only class in this project exported from `index.ts`.
- A Facade MUST explicitly opt an operation into queueing by catching `OfflineTransportError` and calling `MutationQueueService.enqueue`.

## MUST NOT
- [[../repo-platform.skill.md#MUST NOT|repo-platform]]
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Anti-patterns

- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation and the queueing decision the Facade owns
  - Instead: the store always goes through the Facade
- **Enqueueing an operation whose business validation already failed**
  - Consequence: queues a command that will never succeed
  - Instead: business validation always runs and fails before any queueing decision is considered

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only
- [ ] Every Client method's HTTP calls go through `libs/shared/http-core`
- [ ] Every queueable operation's Facade method distinguishes "queued" from an immediate result

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]
