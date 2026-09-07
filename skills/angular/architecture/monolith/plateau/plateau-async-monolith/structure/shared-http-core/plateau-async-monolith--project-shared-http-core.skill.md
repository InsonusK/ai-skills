---
name: plateau-async-monolith--project-shared-http-core
description: Base HTTP service shared by every feature's Client — common concerns (base URL, timeout, retry) — async-monolith plateau
domain: skill
type: template
whenToUse: when editing the base HTTP service or the shared transport error types in libs/shared/http-core
plateau: async-monolith
project_kind: library
version: 20260902160000
tags:
  - skill/template/project
  - plateau/async-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"

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
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| base-http.service.ts | Thin wrapper over `HttpClient`: resolves the backend base URL from configuration, applies a default request timeout, and a default retry policy for idempotent (GET) requests. Does not know about any feature's DTOs — feature Clients call this and handle their own DTO mapping and error translation on top. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]

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
