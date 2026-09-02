---
description: Generic pattern for a feature's Facade — the public entry point of its data-access lib, owning business validation and orchestration
project_name: "{Feature}"
name: "{feature}"
element_kind: service
change_kind: create
tags:
  - solution/api-http-layer
  - element/feature-facade-ts
---

# How this generic file is used
This is not tied to one concrete feature. Any solution that creates a new `libs/{feature}/data-access` project follows this pattern, substituting `{Feature}`/`{feature}` with the real feature name.

# Goals

- Be the only public entry point into the feature's data-access lib, called directly by that feature's Signal Store methods (per the "State management" solution)
- Own business-rule validation and orchestration, keeping it separate from transport/mapping concerns (owned by the Client)

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Facade | {Feature}Facade | OrdersFacade | {feature}.facade.ts | orders.facade.ts |
| Multiple facets | {Feature}_{Facet}Facade | Orders_PaymentFacade | facade/{feature}_{facet}.facade.ts | facade/orders_payment.facade.ts |

# Implementation changes

```typescript
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

# Rule changes

## MUST
- The Facade MUST be the only class in this feature's `data-access` project exported from `index.ts`, along with the feature's domain error types.
- Business-rule validation that does not require a network call (e.g. "quantity must be positive") MUST happen in the Facade, before calling the Client — not duplicated inside the Client.
- Any error the Facade lets through to its caller MUST be one of this feature's typed domain errors (from the Client, optionally re-wrapped with business context) — never a raw `HttpErrorResponse`, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|error-handling-strategy]].

## SHOULD
- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade** — Consequence: blurs the Facade/Client separation this solution exists to establish, making business logic and transport concerns hard to test independently — Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client

# Check list

- [ ] The Facade contains all business-rule validation for this feature's data operations
- [ ] The Facade never performs DTO mapping or calls `http-core`/`HttpClient` directly
- [ ] Every error surfaced by the Facade is a typed domain error

# Unittest TestCases

- [ ] WHEN `addOrder` is called with invalid business input (e.g. non-positive quantity) THEN
  - [ ] the Facade throws a validation error without calling the Client
- [ ] WHEN the Client throws a transport-level conflict error THEN
  - [ ] the Facade re-wraps it into a more specific business error where applicable
