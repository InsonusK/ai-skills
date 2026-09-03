---
name: plateau-async-monolith--class-feature-store
description: Generic pattern for a feature-level NgRx Signal Store — applies to any {Feature}/feature lib, unit-tested by faking its Facade — async-monolith plateau
domain: skill
type: template
plateau: async-monolith
artifact_type: store
version: 20260902160000
tags:
  - skill/template/class
  - plateau/async-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.store.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Own all state and derived data specific to one feature, colocated with that feature's components
- Avoid NgRx actions/reducers/effects boilerplate for state that has no cross-cutting audit requirement
- Test business/state orchestration in isolation, without needing to mock HTTP at all

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Genuinely global reads (e.g. current user) come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally
- For feature-scoped operations, this store's methods call the feature's data-access Facade directly — no Action/Reducer/Effect is introduced
- A Signal Store test fakes its Facade directly and never reaches further down to fake the Client or mock HTTP

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Feature-level Signal Store | `{Feature}Store` | `OrdersStore` | `{feature}.store.ts` | `orders.store.ts` |
| Store spec | — | — | `{feature}.store.spec.ts` | `orders.store.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-store
// Plateau: async-monolith
// Version: 20260711180000

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

```typescript
// orders.store.spec.ts — fakes the Facade, no HTTP or Client involved
describe('OrdersStore', () => {
  let store: InstanceType<typeof OrdersStore>;
  const facadeMock = { addOrder: vi.fn() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdersStore, { provide: OrdersFacade, useValue: facadeMock }],
    });
    store = TestBed.inject(OrdersStore);
  });

  it('sets loading while addOrder is in flight, then clears it on success', async () => {
    facadeMock.addOrder.mockResolvedValue({ id: '1', quantity: 2 });
    const promise = store.addOrder({ quantity: 2 });
    expect(store.loading()).toBe(true);
    await promise;
    expect(store.loading()).toBe(false);
  });
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Rules

## MUST
- The feature Signal Store must be the only place that feature's components read/write feature-owned state — components must never call the feature's `data-access` Facade directly.
- All HTTP/data access invoked from the store must go through that feature's own `data-access` Facade — never directly via `HttpClient`, and never via that feature's Client, in the store.
- For feature-scoped operations, the store method must call the Facade directly — no Action/Reducer/Effect is introduced.
- A Signal Store test must fake its Facade directly — it must never reach further down to fake the Client or mock HTTP.

## SHOULD
- Genuinely global reads (e.g. current user) should come from `libs/shared/state` selectors, injected into the feature store, rather than duplicated locally.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]


- Apply SEVERAL plateau templates per class/artifact
- **A feature Signal Store re-implementing auth/session state locally instead of reading `libs/shared/state`**
  - Consequence: duplicated, potentially stale source of truth for cross-cutting state
  - Instead: inject and select from the shared global store for anything cross-cutting; keep only feature-owned data in the feature store
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce
  - Instead: the store always goes through the Facade; only the Facade calls the Client
- **A Signal Store test faking the Client instead of the Facade**
  - Consequence: skips exercising the Facade's own business validation, and couples the store test to an implementation detail (the Client) two layers below what it's actually testing
  - Instead: always fake the layer directly beneath the unit under test — never skip a layer

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Check list

- [ ] Feature components read/write state only through this store, never through a facade or HttpClient directly
- [ ] No cross-cutting state (auth, notifications, offline-sync) is duplicated inside this store
- [ ] No Action, Reducer, or Effect exists for any feature-scoped operation — only this store calling the Facade
- [ ] The store's spec fakes the Facade only, never `HttpTestingController` or a real Client

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN `load()` is called THEN
  - [ ] `loading` becomes `true` synchronously
  - [ ] on success, `orders` is populated and `loading` becomes `false`
  - [ ] on failure, `error` is set and `loading` becomes `false`
- [ ] WHEN the Signal Store's method is called THEN
  - [ ] `loading` transitions correctly around the (faked) Facade call, and state reflects the result

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create.md|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]
