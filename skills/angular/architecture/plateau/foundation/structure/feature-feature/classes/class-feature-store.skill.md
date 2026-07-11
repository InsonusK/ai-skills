---
name: class-feature-store
description: Generic pattern for a feature-level NgRx Signal Store — applies to any {Feature}/feature lib
domain: skill
type: template
plateau: foundation
artifact_type: store
version: 20260711120000
tags:
  - skill/template/class
  - plateau/foundation
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
---

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.store.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Own all state and derived data specific to one feature, colocated with that feature's components
- Avoid NgRx actions/reducers/effects boilerplate for state that has no cross-cutting audit requirement

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Genuinely global reads (e.g. current user) come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Feature-level Signal Store | `{Feature}Store` | `OrdersStore` | `{feature}.store.ts` | `orders.store.ts` |

# Implementation

```typescript
// Skill: class-feature-store
// Plateau: foundation
// Version: 20260711120000

export const OrdersStore = signalStore(
  withState<OrdersState>({ orders: [], loading: false, error: null }),
  withComputed(({ orders }) => ({
    orderCount: computed(() => orders().length),
  })),
  withMethods((store, ordersFacade = inject(OrdersDataAccessFacade)) => ({
    async load() {
      patchState(store, { loading: true, error: null });
      try {
        const orders = await ordersFacade.fetchOrders();
        patchState(store, { orders, loading: false });
      } catch (error) {
        patchState(store, { loading: false, error: String(error) });
      }
    },
  })),
);
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Rules

## MUST
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state — components MUST NOT call the feature's `data-access` facade directly.
- All HTTP/data access invoked from the store MUST go through that feature's own `data-access` lib, never directly via `HttpClient` in the store.

## SHOULD
- Genuinely global reads (e.g. current user) SHOULD come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Anti-patterns

- **A feature Signal Store re-implementing auth/session state locally instead of reading `libs/shared/state`**
  - Consequence: duplicated, potentially stale source of truth for cross-cutting state
  - Instead: inject and select from the shared global store for anything cross-cutting; keep only feature-owned data in the feature store

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Check list

- [ ] Feature components read/write state only through this store, never through a facade or HttpClient directly
- [ ] No cross-cutting state (auth, notifications, offline-sync) is duplicated inside this store

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Unittest TestCases

- [ ] WHEN `load()` is called THEN
  - [ ] `loading` becomes `true` synchronously
  - [ ] on success, `orders` is populated and `loading` becomes `false`
  - [ ] on failure, `error` is set and `loading` becomes `false`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
