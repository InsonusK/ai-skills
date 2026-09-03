---
name: registry-feature-client-ts
description: Conflict Detection result for the `feature-client-ts` element in the plateau-offline-read-monolith plateau
tags:
  - concern/architecture
  - stack/typescript
  - element/feature-client-ts
---

# Element
`{feature}.client.ts` — a feature's internal transport layer in `libs/{feature}/data-access/src/lib/`, never exported outside the data-access lib, the single place a raw `HttpErrorResponse` is caught.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] (VP3, `.create` — `DataAccess/{Feature}.project.create/{feature}.client.ts.create` — the Client: DTO↔model mapping via the feature mapper, HTTP via `libs/shared/http-core`, every `HttpErrorResponse` caught and rethrown as a typed feature domain error)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4, `.extend` — `DataAccess/{feature}.client.ts.extend` — adds one catch branch: an `HttpErrorResponse` with `status === 0` (no response received) throws the shared `OfflineTransportError` from `libs/shared/http-core`, checked *before* any feature status-code branch)

# Classification
`TMN` — Constraint `T` (VP4 `requires` VP3 in the Variability Map — `solution-offline-first` also declares `depends_on solution-api-http-layer`). Category `M` (code change to the Client's catch block). Kind `N` (independent): the `.extend` adds a *new, earlier* branch to a `catch` the `.create` defined; it does not modify the existing 4xx/5xx → domain-error branches. A real server response still maps to the feature's own typed error, unchanged.

# Ordering
`source: constraint` — VP4 requires VP3, so `solution-api-http-layer` (create) is always composed before `solution-offline-first` (extend). The ordering is already carried by the `requires` edge / `depends_on`; nothing extra to record.

# Resolution
**Canonical — no resolver.** The `.extend` is a single-direction refine of a method the `.create` owns. `OfflineTransportError` is defined once, in `libs/shared/http-core`, never per feature. The `plateau-offline-read-monolith` example's `orders.client.spec.ts` pins all four cases: mapped DTO, 409 → `OrdersConflictError`, `status 0` → `OfflineTransportError`, 500 → `OrdersTransportError` (not the offline error).
