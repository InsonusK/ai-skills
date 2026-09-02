---
name: plateau-online-monolith--class-feature-integration-spec
description: Generic pattern for a feature-level integration test spanning Signal Store, Facade, and Client together, using MSW to intercept at the network boundary — online-monolith plateau
domain: skill
type: template
whenToUse: when creating or editing this class in the online-monolith plateau, or another artifact that plays the same role
plateau: online-monolith
artifact_type: spec
version: 20260902000000
tags:
  - skill/template/class
  - plateau/online-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create.md|{feature}.integration.spec.ts.create]]


# How this generic file is used
This applies only when a test deliberately exercises more than one architectural layer together (Signal Store → Facade → Client → network) rather than a single unit in isolation — the exception to the "fake the layer directly below" rule, reserved for scenarios worth verifying end-to-end at the module level without a real backend.

Create this file as `spec/{feature}.integration.spec.ts` inside the feature library, next to `{feature}.store.ts`.

# Goals

- Verify that Signal Store, Facade, and Client are wired together correctly as a whole, using network-level mocks that can also be shared with Storybook/local dev tooling if needed

# Implementation changes

File: `spec/{feature}.integration.spec.ts`

```typescript
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { OrdersStore } from '../orders.store';
import { OrdersFacade } from '../orders.facade';
// Import the Client from the feature's data-access library (use the workspace path alias in real code).
import { OrdersClient } from '../../../../data-access/src/lib/orders.client';

const server = setupServer(
  http.post('/orders', () => HttpResponse.json({ id: '1', qty: 2, created_at: '2026-01-01' })),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Orders feature integration', () => {
  describe('addOrder', () => {
    it('adds an order through the real Store -> Facade -> Client chain', async () => {
      TestBed.configureTestingModule({
        providers: [OrdersStore, OrdersFacade, OrdersClient, provideHttpClient()],
      });
      const store = TestBed.inject(OrdersStore);

      await store.addOrder({ quantity: 2 });

      expect(store.orders()).toEqual([{ id: '1', quantity: 2, createdAt: new Date('2026-01-01') }]);
    });
  });
});
```

# Rule changes

## MUST
- This pattern must only be used when the test's purpose is genuinely to verify multiple layers wired together — a test that could be satisfied by faking one layer below the unit under test must use the corresponding unit-test pattern instead, not this one.
- MSW request handlers used here may be shared with Storybook or local dev-mode mocking, but must never be duplicated with equivalent `HttpTestingController` expectations elsewhere for the same endpoint.
- Tests for each Signal Store method exercised by this integration spec must be grouped under a nested `describe('<methodName>', () => { ... })` block.

## SHOULD
- **Reaching for this integration pattern as the default way to test a Facade or Signal Store** — Consequence: every such test becomes slower and more complex than necessary, and reintroduces the duplicated-mock risk this solution's ADR exists to avoid, since the endpoint may now be mocked both here and in the Client's own `HttpTestingController` test — Instead: default to the narrower unit-test pattern (faking the layer directly below); reserve this pattern for scenarios that genuinely need multiple real layers wired together

# Check list

- [ ] This pattern is used only for genuine cross-layer scenarios, not as a substitute for narrower unit tests
- [ ] MSW handlers here do not duplicate assertions already covered by the Client's own `HttpTestingController` tests
- [ ] Every Signal Store method exercised by this integration spec has its own `describe('<methodName>', ...)` block

# Unittest TestCases

- [ ] WHEN the Signal Store's method is called in this integration test THEN
  - [ ] the real Facade validates and the real Client performs the (MSW-intercepted) request, producing correctly mapped state in the Store
