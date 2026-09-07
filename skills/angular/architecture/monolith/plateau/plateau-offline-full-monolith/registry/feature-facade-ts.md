---
name: registry-feature-facade-ts
description: Conflict Detection result for the `feature-facade-ts` element in the plateau-offline-full-monolith plateau
tags:
  - concern/architecture
  - stack/typescript
  - element/feature-facade-ts
---

# Element
`{feature}.facade.ts` — a feature's Facade in `libs/{feature}/data-access/src/lib/`, the only public entry point of the data-access lib, owning business validation and orchestration.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] (VP3, `.create` — `DataAccess/{Feature}.project.create/{feature}.facade.ts.create` — the Facade: business validation before the Client call, re-wrap a transport error into a feature domain error)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5, `.extend` — `DataAccess/{feature}.facade.ts.extend` — for an operation the Facade explicitly marks queueable, catch `OfflineTransportError` and `MutationQueueService.enqueue` instead of throwing, returning a value that distinguishes "queued" from "done")

# Classification
`TMN` — Constraint `T` (VP5 `requires` VP4 `requires` VP3 in the Variability Map; `solution-offline-sync` declares `depends_on solution-api-http-layer` and `solution-offline-first`). Category `M` (code change to the Facade's `catch`). Kind `N` (independent): the `.extend` adds a *new, earlier* branch (`error instanceof OfflineTransportError`) to the `catch` the `.create` defined; it does not modify the existing conflict / re-wrap branches, and it only fires for operations the Facade opts in. Business validation still runs and fails first, unchanged.

# Ordering
`source: constraint` — VP5 requires VP4 requires VP3, so `solution-api-http-layer` (create) is always composed before `solution-offline-sync` (extend). The order is carried by the `requires` / `depends_on` edges; nothing extra to record.

# Resolution
**Canonical — no resolver.** Single-direction refine of a method the `.create` owns. The offline error is caught *before* any feature status-code branch (mirroring the `feature-client-ts` group). The `plateau-offline-full-monolith` example's `orders.facade.spec.ts` pins: validation fails without enqueue, `OfflineTransportError` → enqueue + `{ queued: true }`, a genuine server error re-throws without enqueue.
