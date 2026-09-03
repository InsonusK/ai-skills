---
name: plateau-multiuser-monolith--project-shared-http-core
description: Base HTTP service shared by every feature's Client (base URL, timeout, retry) plus the shared OfflineTransportError every Client throws on a network-level failure — multiuser-monolith plateau
domain: skill
type: template
plateau: multiuser-monolith
project_kind: library
version: 20260903150000
tags:
  - skill/template/project
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

# Goal

- Give every feature's Client a single, consistent place for cross-cutting HTTP concerns (base URL resolution, timeout, retry policy), instead of each Client reimplementing them

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

# Structure

## Project Structure

```
/libs/shared/http-core
  /src
    /lib
      base-http.service.ts
      offline-transport-error.ts   <- new (VP4 / solution-offline-first)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| base-http.service.ts | Thin wrapper over `HttpClient`: resolves the backend base URL from configuration, applies a default request timeout, and a default retry policy for idempotent (GET) requests. Does not know about any feature's DTOs — feature Clients call this and handle their own DTO mapping and error translation on top. | — |
| offline-transport-error.ts | The single shared `OfflineTransportError` — thrown by every feature's Client when an `HttpErrorResponse` has `status === 0` (the request never reached the server). Defined once here, never per feature, so callers catch it uniformly. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend.md|DataAccess/{feature}.client.ts.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient) | matching the Angular major version in use | Underlying HTTP transport |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

## What Does NOT Belong Here

- DTO mapping — belongs in each feature's own `{feature}.client.ts`/`{feature}.mapper.ts`
- Auth token attachment — handled globally in the future, authentication doesn't implemented in this plateau
- Business-specific retry/conflict logic (e.g. offline-sync retry queues) — belongs to a future offline-sync solution

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

## Allowed Dependencies

- None (leaf project within `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

# Rules

## MUST
- `base-http.service.ts` must never reference any feature-specific DTO or domain type — it is feature-agnostic by design.
- Retry policy applied here must only apply to idempotent requests (GET) by default; a feature's Client may opt out or apply its own policy for non-idempotent calls.
- `OfflineTransportError` must be defined exactly once, here, and re-exported from `index.ts` — never redefined inside a feature's `data-access` lib.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]


- **Adding a feature-specific special case directly into `base-http.service.ts`**
  - Consequence: turns a feature-agnostic shared service into a growing pile of one-off conditions, coupling unrelated features to each other through a shared file
  - Instead: keep this service generic; feature-specific behavior belongs in that feature's own Client

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

# Check list

- [ ] `base-http.service.ts` contains no feature-specific types or logic
- [ ] Every feature's Client calls this service rather than `HttpClient` directly

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

# Unittest TestCases

- [ ] WHEN a request exceeds the configured timeout THEN
  - [ ] the service surfaces a timeout error to the calling Client
- [ ] WHEN a GET request fails transiently THEN
  - [ ] the default retry policy retries it before surfacing a failure

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]
