---
name: plateau-online-monolith--class-feature-client
description: Generic pattern for a feature's Client — the internal transport layer of its data-access lib, never exported outside it, unit-tested via HttpTestingController — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
artifact_type: service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/online-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.client.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Map between DTOs and domain models, and perform the actual HTTP call via `libs/shared/http-core`, entirely hidden behind the Facade
- Verify the exact HTTP request shape and DTO mapping this Client produces, in isolation from every layer above it — the only place `HttpTestingController` is used

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- This is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error — no caller further up the chain ever sees it
- `HttpTestingController` is used only inside this Client's own spec — no other test in the workspace uses it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | --------------- | -------------------- | --------- |
| Client | `{Feature}Client` | `OrdersClient` | `{feature}.client.ts` | `orders.client.ts` |
| Multiple facets | `{Feature}_{Facet}Client` | `Orders_PaymentClient` | `client/{feature}_{facet}.client.ts` | `client/orders_payment.client.ts` |
| Client spec | — | — | `{feature}.client.spec.ts` | `orders.client.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-client
// Plateau: online-monolith
// Version: 20260711180000

@Injectable({ providedIn: 'root' })
export class OrdersClient {
  constructor(private readonly http: BaseHttpService) {}

  async addOrder(input: AddOrderInput): Promise<Order> {
    const dto = orderModelToDto(input);
    try {
      const responseDto = await firstValueFrom(this.http.post<OrderDto>('/orders', dto));
      return orderDtoToModel(responseDto);
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 409) {
        throw new OrdersConflictError(input.id, { cause: error });
      }
      throw new OrdersAddError('unexpected error adding order', { cause: error });
    }
  }
}
```

```typescript
// orders.client.spec.ts — the only place HttpTestingController is used
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

  afterEach(() => httpTesting.verify());
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]

# Rules

## MUST
- The Client must never be exported from the feature's `index.ts` — it is only ever called by that feature's own Facade.
- The Client must call `libs/shared/http-core`'s base service, not `HttpClient` directly.
- The Client must catch every `HttpErrorResponse` its calls can produce and rethrow one of this feature's typed domain errors — never let the raw error escape.
- DTO ↔ domain model mapping must go through this feature's `{feature}.mapper.ts` hand-written functions — never inlined ad hoc inside the Client method.
- Every test in this Client's spec must use `HttpTestingController` to assert the exact request (method, URL, body).
- `httpTesting.verify()` must be called in `afterEach` to catch any unexpected or unmatched request.
- At least one test must assert the typed domain error thrown on a transport failure.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]


- Apply SEVERAL plateau templates per class/artifact
- **Letting an `HttpErrorResponse` propagate out of a Client method uncaught**
  - Consequence: callers end up branching on HTTP status codes instead of a meaningful domain error
  - Instead: catch every failure path and rethrow a typed domain error specific to this feature
- **Inlining ad hoc field mapping inside the HTTP call instead of using the feature's mapper functions**
  - Consequence: mapping logic is duplicated or inconsistent across multiple Client methods
  - Instead: always route through `{feature}.mapper.ts`'s functions
- **Omitting `httpTesting.verify()` from `afterEach`**
  - Consequence: an unexpected or unmatched request silently goes undetected
  - Instead: always call `httpTesting.verify()` after every test

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]

# Check list

- [ ] The Client is never exported from the feature's `index.ts`
- [ ] Every HTTP call goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] Every possible error path ends in a typed domain error, never a raw `HttpErrorResponse`
- [ ] All DTO/model conversion goes through `{feature}.mapper.ts`
- [ ] Every Client method has at least one success-path and one failure-path test
- [ ] `httpTesting.verify()` runs after every test
- [ ] When a feature has multiple data facets, every Client stays internal and named `{Feature}_{Facet}Client` under `client/`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN the backend returns a 409 conflict THEN
  - [ ] the Client throws `{Feature}ConflictError`, not the raw `HttpErrorResponse`
- [ ] WHEN the backend returns a successful response THEN
  - [ ] the Client returns a correctly mapped domain model, using `{feature}.mapper.ts`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create.md|Testing/{feature}.client.spec.ts.create]]
