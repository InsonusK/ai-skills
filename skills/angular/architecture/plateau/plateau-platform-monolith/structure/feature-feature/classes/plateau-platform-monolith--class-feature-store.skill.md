---
name: plateau-platform-monolith--class-feature-store
description: Generic pattern for a feature-level NgRx Signal Store — applies to any {Feature}/feature lib — platform-monolith plateau
domain: skill
type: template
plateau: platform-monolith
artifact_type: store
version: 20260711210000
tags:
  - skill/template/class
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete feature.

# Goal

- Own all state and derived data specific to one feature, colocated with that feature's components
- Avoid NgRx actions/reducers/effects boilerplate for state that has no cross-cutting audit requirement

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Genuinely global reads (current user, connectivity) come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Feature-level Signal Store | `{Feature}Store` | `OrdersStore` | `{feature}.store.ts` | `orders.store.ts` |
| Store spec | `{Feature}Store` (tested) | `OrdersStore` | `{feature}.facade-and-store.spec.ts` | `orders.facade-and-store.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-store
// Plateau: platform-monolith
// Version: 20260711210000

export const OrdersStore = signalStore(
  withState<OrdersState>({ orders: [], loading: false, error: null }),
  withComputed(({ orders }) => ({
    orderCount: computed(() => orders().length),
  })),
  withMethods((store, ordersFacade = inject(OrdersFacade)) => ({
    async addOrder(input: AddOrderInput) {
      patchState(store, { loading: true, error: null });
      try {
        const result = await ordersFacade.addOrder(input);
        // result may be a created Order, or { queued: true } if attempted offline
        patchState(store, { loading: false });
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
- A Signal Store test MUST fake its Facade directly — it MUST NOT reach further down to fake the Client or mock HTTP.

## SHOULD
- Genuinely global reads (e.g. current user, `isOnline`) SHOULD come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature Signal Store re-implementing auth/session or connectivity state locally instead of reading `libs/shared/state`**
  - Consequence: duplicated, potentially stale source of truth for cross-cutting state
  - Instead: inject and select from the shared global store for anything cross-cutting; keep only feature-owned data in the feature store
- **A Signal Store test faking the Client instead of the Facade**
  - Consequence: skips exercising the Facade's own business validation and offline-queueing decision, and couples the store test to an implementation detail two layers below what it's actually testing
  - Instead: always fake the layer directly beneath the unit under test

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]

# Check list

- [ ] Feature components read/write state only through this store, never through a facade or HttpClient directly
- [ ] No cross-cutting state (auth, connectivity, notifications) is duplicated inside this store
- [ ] Every Signal Store test fakes the Facade, never the Client or HTTP

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN `addOrder()` is called THEN
  - [ ] `loading` becomes `true` synchronously
  - [ ] on success, state reflects the result and `loading` becomes `false`
  - [ ] on failure, `error` is set and `loading` becomes `false`
- [ ] WHEN the (faked) Facade returns `{ queued: true }` THEN
  - [ ] the store transitions to a pending state rather than treating it as a completed success
- [ ] WHEN the Signal Store's method is called in a cross-layer integration test (real Facade + Client, MSW-intercepted network) THEN
  - [ ] the real chain produces correctly mapped state in the Store

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]
