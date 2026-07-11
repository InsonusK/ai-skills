---
name: class-feature-store
description: Generic pattern for a feature-level NgRx Signal Store — applies to any {Feature}/feature lib
domain: skill
type: template
plateau: observable
artifact_type: store
version: 20260711160000
tags:
  - skill/template/class
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
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
- For feature-scoped operations, this store's methods call the feature's data-access Facade directly — no Action/Reducer/Effect is introduced

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Feature-level Signal Store | `{Feature}Store` | `OrdersStore` | `{feature}.store.ts` | `orders.store.ts` |

# Implementation

```typescript
// Skill: class-feature-store
// Plateau: data-capable
// Version: 20260711160000

export const OrdersStore = signalStore(
  withState<OrdersState>({ orders: [], loading: false, error: null }),
  withComputed(({ orders }) => ({
    orderCount: computed(() => orders().length),
  })),
  withMethods((store, ordersFacade = inject(OrdersFacade)) => ({
    async load() {
      patchState(store, { loading: true, error: null });
      try {
        const orders = await ordersFacade.fetchOrders();
        patchState(store, { orders, loading: false });
      } catch (error) {
        patchState(store, { loading: false, error: String(error) });
      }
    },
    async addOrder(input: AddOrderInput) {
      try {
        const order = await ordersFacade.addOrder(input);
        patchState(store, { orders: [...store.orders(), order] });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  })),
);
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state — components MUST NOT call the feature's `data-access` Facade directly.
- All HTTP/data access invoked from the store MUST go through that feature's own `data-access` Facade — never directly via `HttpClient`, and never via that feature's Client, in the store.
- For feature-scoped operations, the store method MUST call the Facade directly — no Action/Reducer/Effect is introduced.

## SHOULD
- Genuinely global reads (e.g. current user) SHOULD come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature Signal Store re-implementing auth/session state locally instead of reading `libs/shared/state`**
  - Consequence: duplicated, potentially stale source of truth for cross-cutting state
  - Instead: inject and select from the shared global store for anything cross-cutting; keep only feature-owned data in the feature store
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce
  - Instead: the store always goes through the Facade; only the Facade calls the Client

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] Feature components read/write state only through this store, never through a facade or HttpClient directly
- [ ] No cross-cutting state (auth, notifications, offline-sync) is duplicated inside this store
- [ ] No Action, Reducer, or Effect exists for any feature-scoped operation — only this store calling the Facade

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN `load()` is called THEN
  - [ ] `loading` becomes `true` synchronously
  - [ ] on success, `orders` is populated and `loading` becomes `false`
  - [ ] on failure, `error` is set and `loading` becomes `false`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
