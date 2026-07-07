---
description: Generic pattern for a feature-level integration test spanning Signal Store, Facade, and Client together, using MSW to intercept at the network boundary
project_name: "{Feature}"
name: "{feature}.integration"
artifact_type: component
change_kind: create
---

# How this generic file is used
This applies only when a test deliberately exercises more than one architectural layer together (Signal Store → Facade → Client → network) rather than a single unit in isolation — the exception to the "fake the layer directly below" rule, reserved for scenarios worth verifying end-to-end at the module level without a real backend.

# Goals

- Verify that Signal Store, Facade, and Client are wired together correctly as a whole, using network-level mocks that can also be shared with Storybook/local dev tooling if needed

# Implementation changes

```code example
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';

const server = setupServer(
  http.post('/orders', () => HttpResponse.json({ id: '1', qty: 2, created_at: '2026-01-01' })),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Orders feature integration', () => {
  it('adds an order through the real Store -> Facade -> Client chain', async () => {
    TestBed.configureTestingModule({
      providers: [OrdersStore, OrdersFacade, OrdersClient, provideHttpClient()],
    });
    const store = TestBed.inject(OrdersStore);

    await store.addOrder({ quantity: 2 });

    expect(store.orders()).toEqual([{ id: '1', quantity: 2, createdAt: new Date('2026-01-01') }]);
  });
});
```

# Rule changes

## MUST
- This pattern MUST only be used when the test's purpose is genuinely to verify multiple layers wired together — a test that could be satisfied by faking one layer below the unit under test MUST use the corresponding unit-test pattern instead, not this one.
- MSW request handlers used here MAY be shared with Storybook or local dev-mode mocking, but MUST NOT be duplicated with equivalent `HttpTestingController` expectations elsewhere for the same endpoint.

# Anti-patterns

- **Reaching for this integration pattern as the default way to test a Facade or Signal Store**
  - Consequence: every such test becomes slower and more complex than necessary, and reintroduces the duplicated-mock risk this solution's ADR exists to avoid, since the endpoint may now be mocked both here and in the Client's own `HttpTestingController` test
  - Instead: default to the narrower unit-test pattern (faking the layer directly below); reserve this pattern for scenarios that genuinely need multiple real layers wired together

# Check list

- [ ] This pattern is used only for genuine cross-layer scenarios, not as a substitute for narrower unit tests
- [ ] MSW handlers here do not duplicate assertions already covered by the Client's own `HttpTestingController` tests

# Unittest TestCases

- [ ] WHEN the Signal Store's method is called in this integration test THEN
  - [ ] the real Facade validates and the real Client performs the (MSW-intercepted) request, producing correctly mapped state in the Store
