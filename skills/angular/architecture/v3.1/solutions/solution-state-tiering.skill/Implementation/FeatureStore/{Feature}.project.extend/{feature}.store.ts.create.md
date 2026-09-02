---
description: Generic pattern for a feature-level NgRx Signal Store — applies to any {Feature}/feature lib created by a future feature-owning solution
project_name: "{Feature}"
name: "{feature}"
element_kind: store
change_kind: create
tags:
  - solution/state-tiering
  - element/feature-store-ts
---

# How this generic file is used
This is not tied to one concrete feature. Any solution that creates a new `libs/{feature}/feature` project (see `solution-repository-structure`'s structure) follows this pattern for that feature's own state, substituting `{Feature}`/`{feature}` with the real feature name.

# Goals

- Own all state and derived data specific to one feature, colocated with that feature's components
- Avoid NgRx actions/reducers/effects boilerplate for state that has no cross-cutting audit requirement

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Feature-level Signal Store | {Feature}Store | OrdersStore | {feature}.store.ts | orders.store.ts |

# Implementation changes

```typescript
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

# Rule changes

## MUST
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state — components MUST NOT call the feature's `data-access` facade directly.
- All HTTP/data access invoked from the store MUST go through that feature's own `data-access` lib (see the future `solution-api-http-layer`), never directly via `HttpClient` in the store.

## SHOULD
- Genuinely global reads (e.g. current user) SHOULD come from `libs/shared/state` selectors, injected into the feature store, rather than being duplicated locally (see [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create]]).

- **A feature Signal Store re-implementing auth/session state locally instead of reading `libs/shared/state`** — Consequence: duplicated, potentially stale source of truth for cross-cutting state — Instead: inject and select from the shared global store for anything cross-cutting; keep only feature-owned data in the feature store
# Check list

- [ ] Feature components read/write state only through this store, never through a facade or HttpClient directly
- [ ] No cross-cutting state (auth, notifications, offline-sync) is duplicated inside this store

# Unittest TestCases

- [ ] WHEN `load()` is called THEN
  - [ ] `loading` becomes `true` synchronously
  - [ ] on success, `orders` is populated and `loading` becomes `false`
  - [ ] on failure, `error` is set and `loading` becomes `false`
