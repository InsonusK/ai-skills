---
name: project-shared-http-core
description: Base HTTP service shared by every feature's Client — common concerns (base URL, timeout, retry) and the shared OfflineTransportError type
domain: skill
type: template
plateau: platform-monolith
project_kind: library
version: 20260711210000
tags:
  - skill/template/project
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

# Goal

- Give every feature's Client a single, consistent place for cross-cutting HTTP concerns (base URL resolution, timeout, retry policy), instead of each Client reimplementing them
- Host the single, shared `OfflineTransportError` type every feature's Client throws for a network-level failure

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

# Core Principles

- Feature-agnostic by design — never references a feature-specific DTO or domain type
- Retry policy applies only to idempotent (GET) requests by default

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

# Structure

## Project Structure

```
/libs/shared/http-core
  /src
    /lib
      base-http.service.ts
      offline-transport.error.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| base-http.service.ts | Thin wrapper over `HttpClient`: base URL resolution, default timeout, default retry policy for GET requests | — |
| offline-transport.error.ts | `OfflineTransportError` — the single, shared type every feature's Client throws when a request never reaches the server (`status === 0`) | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient) | matching the Angular major version in use | Underlying HTTP transport |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

## What Does NOT Belong Here

- DTO mapping — belongs in each feature's own `{feature}.client.ts`/`{feature}.mapper.ts`
- Auth token attachment — handled globally by `authInterceptor`, registered independently of this service
- Any durable, persisted retry/queue mechanism for failed mutations — belongs in `libs/shared/offline-sync`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- None (leaf)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

# Rules

## MUST
- `base-http.service.ts` MUST NOT reference any feature-specific DTO or domain type.
- Retry policy applied here MUST only apply to idempotent requests (GET) by default.
- `OfflineTransportError` MUST be defined once here, not redefined per feature, so callers across every feature can catch it uniformly.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Anti-patterns

- **Adding a feature-specific special case directly into `base-http.service.ts`**
  - Consequence: turns a feature-agnostic shared service into a growing pile of one-off conditions
  - Instead: keep this service generic; feature-specific behavior belongs in that feature's own Client
- **Redefining `OfflineTransportError` separately inside a feature's own `{feature}.errors.ts`**
  - Consequence: callers can no longer catch one shared type across every feature
  - Instead: import the single type defined here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Check list

- [ ] `base-http.service.ts` contains no feature-specific types or logic
- [ ] Every feature's Client calls this service rather than `HttpClient` directly
- [ ] `OfflineTransportError` is defined exactly once, here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
