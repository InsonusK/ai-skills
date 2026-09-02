---
description: Base HTTP service shared by every feature's Client — common concerns (base URL, timeout, retry)
name: shared-http-core
project_kind: library
element_kind: project
change_kind: create
tags:
  - solution/api-http-layer
  - element/shared-http-core-project
---

# Goals

- Give every feature's Client a single, consistent place for cross-cutting HTTP concerns (base URL resolution, timeout, retry policy), instead of each Client reimplementing them

# Structure

## Project Structure

```
/libs/shared/http-core
  /src
    /lib
      base-http.service.ts
    index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| base-http.service.ts | Thin wrapper over `HttpClient`: resolves the backend base URL from configuration, applies a default request timeout, and a default retry policy for idempotent (GET) requests. Does not know about any feature's DTOs — feature Clients call this and handle their own DTO mapping and error translation on top. |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient) | matching the Angular major version in use | Underlying HTTP transport |

# What Does NOT Belong Here

- DTO mapping — belongs in each feature's own `{feature}.client.ts`/`{feature}.mapper.ts`
- Auth token attachment — handled globally by `authInterceptor` from `solution-authentication`, registered independently of this service
- Business-specific retry/conflict logic (e.g. offline-sync retry queues) — belongs to the future `solution-offline-sync`

# Rules

## MUST
- `base-http.service.ts` must never reference any feature-specific DTO or domain type — it is feature-agnostic by design.
- Retry policy applied here must only apply to idempotent requests (GET) by default; a feature's Client may opt out or apply its own policy for non-idempotent calls.

## SHOULD
- **Adding a feature-specific special case directly into `base-http.service.ts`** — Consequence: turns a feature-agnostic shared service into a growing pile of one-off conditions, coupling unrelated features to each other through a shared file — Instead: keep this service generic; feature-specific behavior belongs in that feature's own Client

# Check list

- [ ] `base-http.service.ts` contains no feature-specific types or logic
- [ ] Every feature's Client calls this service rather than `HttpClient` directly

# Unittest TestCases

- [ ] WHEN a request exceeds the configured timeout THEN
  - [ ] the service surfaces a timeout error to the calling Client
- [ ] WHEN a GET request fails transiently THEN
  - [ ] the default retry policy retries it before surfacing a failure
