---
name: plateau-multiuser-monolith--class-feature-facade-and-store-spec
description: Generic pattern for unit-testing a Facade (faking its Client) and a feature Signal Store (faking its Facade) via TestBed, without any HTTP mocking — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when writing or reviewing a feature's facade or store unit spec — faking the one layer directly beneath the unit under test
plateau: multiuser-monolith
artifact_type: spec
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|{feature}.facade-and-store.spec.ts.create]]


# How this generic file is used
This is not tied to one concrete feature. It applies to `spec/{feature}.facade.spec.ts` (faking the Client) and `spec/{feature}.store.spec.ts` (faking the Facade), created per `solution-api-http-layer` and "State management" solutions respectively.

# Goals

- Test business validation/orchestration (Facade) and state orchestration (Signal Store) in isolation, without needing to mock HTTP at all

# Implementation changes

## `spec/{feature}.facade.spec.ts`

```typescript
import { OrdersFacade, OrdersValidationError } from '../orders.facade';
import { OrdersClient } from '../orders.client';

describe('OrdersFacade', () => {
  let facade: OrdersFacade;
  const clientMock = { addOrder: vi.fn() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdersFacade, { provide: OrdersClient, useValue: clientMock }],
    });
    facade = TestBed.inject(OrdersFacade);
  });

  describe('addOrder', () => {
    it('rejects with a validation error before calling the Client for invalid input', async () => {
      await expect(facade.addOrder({ quantity: 0 })).rejects.toBeInstanceOf(OrdersValidationError);
      expect(clientMock.addOrder).not.toHaveBeenCalled();
    });

    it('delegates to the Client for valid input', async () => {
      clientMock.addOrder.mockResolvedValue({ id: '1', quantity: 2 });
      await facade.addOrder({ quantity: 2 });
      expect(clientMock.addOrder).toHaveBeenCalledWith({ quantity: 2 });
    });
  });
});
```

## `spec/{feature}.store.spec.ts`

```typescript
import { OrdersStore } from '../orders.store';
import { OrdersFacade } from '../orders.facade';

describe('OrdersStore', () => {
  let store: InstanceType<typeof OrdersStore>;
  const facadeMock = { addOrder: vi.fn() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdersStore, { provide: OrdersFacade, useValue: facadeMock }],
    });
    store = TestBed.inject(OrdersStore);
  });

  describe('addOrder', () => {
    it('sets loading while addOrder is in flight, then clears it on success', async () => {
      facadeMock.addOrder.mockResolvedValue({ id: '1', quantity: 2 });
      const promise = store.addOrder({ quantity: 2 });
      expect(store.loading()).toBe(true);
      await promise;
      expect(store.loading()).toBe(false);
    });
  });
});
```

# Rule changes

## MUST
- A Facade test must fake its Client directly (e.g. `{ provide: {Feature}Client, useValue: clientMock }`) — it must never use `HttpTestingController` or MSW.
- A Signal Store test must fake its Facade directly — it must never reach further down to fake the Client or mock HTTP.
- Tests for each Facade method must be grouped under a nested `describe('<methodName>', () => { ... })` block.
- Tests for each Signal Store method must be grouped under a nested `describe('<methodName>', () => { ... })` block.

## SHOULD
- **A Signal Store test faking the Client instead of the Facade** — Consequence: skips exercising the Facade's own business validation, and couples the store test to an implementation detail (the Client) two layers below what it's actually testing — Instead: always fake the layer directly beneath the unit under test — never skip a layer

# Check list

- [ ] Every Facade test fakes the Client, never HTTP directly
- [ ] Every Signal Store test fakes the Facade, never the Client or HTTP
- [ ] Every Facade method has its own `describe('<methodName>', ...)` block
- [ ] Every Signal Store method has its own `describe('<methodName>', ...)` block

# Unittest TestCases

- [ ] WHEN the Facade is given invalid business input THEN
  - [ ] it rejects without calling the (faked) Client
- [ ] WHEN the Signal Store's method is called THEN
  - [ ] `loading` transitions correctly around the (faked) Facade call, and state reflects the result
