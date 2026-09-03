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
This is not tied to one concrete feature. Any `{feature}.client.ts` created per `solution-api-http-layer` gets a matching spec file at `spec/{feature}.client.spec.ts`, next to the source file.

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
- Every test uses `HttpTestingController` to assert the exact request (method, URL, body).
  - Risk: a Client test that only checks the return value can pass while the Client hits the wrong endpoint or omits a field.
  - Fix: `httpTesting.expectOne({ method: 'POST', url: '/api/orders' })` and assert `req.request.body`; per [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/testing-layers-and-mocking.md|testing-layers-and-mocking]].
- `httpTesting.verify()` is called in `afterEach`.
  - Risk: an unexpected or unmatched request goes unnoticed, so a stray call the Client makes is never caught.
  - Fix: `afterEach(() => httpTesting.verify())`.
- At least one test asserts the typed domain error thrown on a transport failure.
  - Risk: the Client's `catchError` → typed-error mapping is the whole point of the layer, and it is untested.
  - Fix: `req.flush(null, { status: 409, statusText: 'Conflict' })` then `expect(...).rejects.toBeInstanceOf({Feature}ConflictError)`.
- Tests for each Client method are grouped under a nested `describe('<methodName>', ...)` block.
  - Risk: a flat list of `it`s across methods is hard to scan and to see coverage gaps in.
  - Fix: one `describe` per public method.

# Check list

- [ ] Every Client method has its own `describe('<methodName>', ...)` block
- [ ] Every Client method has at least one success-path test asserting the exact request/response mapping
- [ ] Every Client method has at least one failure-path test asserting the resulting typed domain error
- [ ] `httpTesting.verify()` runs after every test
