---
name: class-feature-facade
description: Generic pattern for a feature's Facade — the public entry point of its data-access lib, owning business validation and orchestration
domain: skill
type: template
plateau: authenticated
artifact_type: service
version: 20260711150000
tags:
  - skill/template/class
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
---

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.facade.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Be the only public entry point into the feature's data-access lib, called directly by that feature's Signal Store methods
- Own business-rule validation and orchestration, keeping it separate from transport/mapping concerns (owned by the Client)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Business validation that does not require a network call happens here, before calling the Client — never duplicated inside the Client

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Facade | `{Feature}Facade` | `OrdersFacade` | `{feature}.facade.ts` | `orders.facade.ts` |

# Implementation

```typescript
// Skill: class-feature-facade
// Plateau: data-capable
// Version: 20260711150000

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

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Rules

## MUST
- The Facade MUST be the only class in this feature's `data-access` project exported from `index.ts`, along with the feature's domain error types.
- Business-rule validation that does not require a network call (e.g. "quantity must be positive") MUST happen in the Facade, before calling the Client — not duplicated inside the Client.
- Any error the Facade lets through to its caller MUST be one of this feature's typed domain errors (from the Client, optionally re-wrapped with business context) — never a raw `HttpErrorResponse`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation this solution exists to establish, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Check list

- [ ] The Facade contains all business-rule validation for this feature's data operations
- [ ] The Facade never performs DTO mapping or calls `http-core`/`HttpClient` directly
- [ ] Every error surfaced by the Facade is a typed domain error

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]

# Unittest TestCases

- [ ] WHEN `addOrder` is called with invalid business input (e.g. non-positive quantity) THEN
  - [ ] the Facade throws a validation error without calling the Client
- [ ] WHEN the Client throws a transport-level conflict error THEN
  - [ ] the Facade re-wraps it into a more specific business error where applicable

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
