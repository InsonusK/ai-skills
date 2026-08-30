---
description: Generic pattern for unit-testing a feature's Client via TestBed and HttpTestingController — the only place HttpTestingController is used
project_name: "{Feature}"
name: "{feature}.client"
element_kind: service
change_kind: create
tags:
  - solution/app-testing
  - element/feature-client-spec-ts
---

# How this generic file is used
This is not tied to one concrete feature. Any `{feature}.client.ts` created per the "API/HTTP-слой" solution gets a matching spec file at `spec/{feature}.client.spec.ts`, next to the source file.

# Goals

- Verify the exact HTTP request shape and DTO mapping this Client produces, in isolation from every layer above it

# Implementation changes

File: `spec/{feature}.client.spec.ts`

```typescript
import { OrdersClient, OrdersConflictError } from '../orders.client';

describe('OrdersClient', () => {
  let client: OrdersClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdersClient, provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(OrdersClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  describe('addOrder', () => {
    it('maps the domain model to the expected DTO and request', async () => {
      const promise = client.addOrder({ quantity: 2 });
      const req = httpTesting.expectOne('/orders');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ qty: 2 });
      req.flush({ id: '1', qty: 2, created_at: '2026-01-01' });
      await expect(promise).resolves.toEqual({ id: '1', quantity: 2, createdAt: new Date('2026-01-01') });
    });

    it('throws a typed domain error on a 409 conflict', async () => {
      const promise = client.addOrder({ quantity: 2 });
      httpTesting.expectOne('/orders').flush('conflict', { status: 409, statusText: 'Conflict' });
      await expect(promise).rejects.toBeInstanceOf(OrdersConflictError);
    });
  });

  afterEach(() => httpTesting.verify());
});
```

# Rule changes

## MUST
- Every test in this file MUST use `HttpTestingController` to assert the exact request (method, URL, body), per [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/adr/testing-layers-and-mocking]].
- `httpTesting.verify()` MUST be called in `afterEach` to catch any unexpected or unmatched request.
- At least one test MUST assert the typed domain error thrown on a transport failure, consistent with the "API/HTTP-слой" solution's error-handling rules.
- Tests for each Client method MUST be grouped under a nested `describe('<methodName>', () => { ... })` block.

# Check list

- [ ] Every Client method has its own `describe('<methodName>', ...)` block
- [ ] Every Client method has at least one success-path test asserting the exact request/response mapping
- [ ] Every Client method has at least one failure-path test asserting the resulting typed domain error
- [ ] `httpTesting.verify()` runs after every test
