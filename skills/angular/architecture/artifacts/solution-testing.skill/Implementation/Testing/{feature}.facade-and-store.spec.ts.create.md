---
description: Generic pattern for unit-testing a Facade (faking its Client) and a feature Signal Store (faking its Facade) via TestBed, without any HTTP mocking
project_name: "{Feature}"
name: "{feature}"
artifact_type: service
change_kind: create
---

# How this generic file is used
This is not tied to one concrete feature. It applies to `{feature}.facade.ts` specs (faking the Client) and `{feature}.store.ts` specs (faking the Facade), created per the "API/HTTP-слой" and "State management" solutions respectively.

# Goals

- Test business validation/orchestration (Facade) and state orchestration (Signal Store) in isolation, without needing to mock HTTP at all

# Implementation changes

```code example
// orders.facade.spec.ts — fakes the Client, no HttpTestingController involved
describe('OrdersFacade', () => {
  let facade: OrdersFacade;
  const clientMock = { addOrder: vi.fn() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdersFacade, { provide: OrdersClient, useValue: clientMock }],
    });
    facade = TestBed.inject(OrdersFacade);
  });

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
```

```code example
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

# Rule changes

## MUST
- A Facade test MUST fake its Client directly (e.g. `{ provide: {Feature}Client, useValue: clientMock }`) — it MUST NOT use `HttpTestingController` or MSW.
- A Signal Store test MUST fake its Facade directly — it MUST NOT reach further down to fake the Client or mock HTTP.

# Anti-patterns

- **A Signal Store test faking the Client instead of the Facade**
  - Consequence: skips exercising the Facade's own business validation, and couples the store test to an implementation detail (the Client) two layers below what it's actually testing
  - Instead: always fake the layer directly beneath the unit under test — never skip a layer

# Check list

- [ ] Every Facade test fakes the Client, never HTTP directly
- [ ] Every Signal Store test fakes the Facade, never the Client or HTTP

# Unittest TestCases

- [ ] WHEN the Facade is given invalid business input THEN
  - [ ] it rejects without calling the (faked) Client
- [ ] WHEN the Signal Store's method is called THEN
  - [ ] `loading` transitions correctly around the (faked) Facade call, and state reflects the result
