---
name: plateau-online-monolith--class-feature-facade
description: Generic pattern for a feature's Facade — the public entry point of its data-access lib, owning business validation and orchestration, unit-tested by faking its Client — online-monolith plateau
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

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.facade.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Be the only public entry point into the feature's data-access lib, called directly by that feature's Signal Store methods
- Own business-rule validation and orchestration, keeping it separate from transport/mapping concerns (owned by the Client)
- Test business validation/orchestration in isolation, without needing to mock HTTP at all

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Business validation that does not require a network call happens here, before calling the Client — never duplicated inside the Client
- A Facade test fakes its Client directly — it never uses `HttpTestingController` or MSW

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Facade | `{Feature}Facade` | `OrdersFacade` | `{feature}.facade.ts` | `orders.facade.ts` |
| Multiple facets | `{Feature}_{Facet}Facade` | `Orders_PaymentFacade` | `facade/{feature}_{facet}.facade.ts` | `facade/orders_payment.facade.ts` |
| Facade spec | — | — | `{feature}.facade.spec.ts` | `orders.facade.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-facade
// Plateau: online-monolith
// Version: 20260711180000

@Injectable({ providedIn: 'root' })
export class OrdersFacade {
  constructor(private readonly client: OrdersClient) {}

  async addOrder(input: AddOrderInput): Promise<Order> {
    if (input.quantity <= 0) {
      throw new OrdersValidationError('quantity must be positive');
    }
    try {
      return await this.client.addOrder(input);
    } catch (error) {
      if (error instanceof OrdersConflictError) {
        throw new OrdersAlreadySubmittedError(input.id, { cause: error });
      }
      throw error;
    }
  }
}
```

```typescript
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

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Rules

## MUST
- The Facade must be the only class in this feature's `data-access` project exported from `index.ts`, along with the feature's domain error types.
- Business-rule validation that does not require a network call must happen in the Facade, before calling the Client — not duplicated inside the Client.
- Any error the Facade lets through to its caller must be one of this feature's typed domain errors — never a raw `HttpErrorResponse`.
- A Facade test must fake its Client directly (e.g. `{ provide: {Feature}Client, useValue: clientMock }`) — it must never use `HttpTestingController` or MSW.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]


- Apply SEVERAL plateau templates per class/artifact
- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client
- **Using `HttpTestingController` inside a Facade test "to save time faking the Client"**
  - Consequence: the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in a Facade test

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Check list

- [ ] The Facade contains all business-rule validation for this feature's data operations
- [ ] The Facade never performs DTO mapping or calls `http-core`/`HttpClient` directly
- [ ] Every error surfaced by the Facade is a typed domain error
- [ ] Every Facade test fakes the Client, never HTTP directly
- [ ] When a feature has multiple data facets, every Facade is exported and named `{Feature}_{Facet}Facade` under `facade/`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN `addOrder` is called with invalid business input (e.g. non-positive quantity) THEN
  - [ ] the Facade throws a validation error without calling the Client
- [ ] WHEN the Client throws a transport-level conflict error THEN
  - [ ] the Facade re-wraps it into a more specific business error where applicable
- [ ] WHEN the Facade is given invalid business input THEN
  - [ ] it rejects without calling the (faked) Client

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create.md|Testing/{feature}.facade-and-store.spec.ts.create]]
