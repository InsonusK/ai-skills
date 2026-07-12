---
name: plateau-monitored-app--class-feature-facade
description: Generic pattern for a feature's Facade — the public entry point of its data-access lib, owning business validation, orchestration, and the offline-queueing decision — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
artifact_type: service
version: 20260711220000
tags:
  - skill/template/class
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]]"
---

> Generic pattern, not tied to one concrete feature.

# Goal

- Be the only public entry point into the feature's data-access lib, called directly by that feature's Signal Store methods
- Own business-rule validation and orchestration, separate from transport/mapping concerns
- Let an operation attempted while offline be queued for later sync instead of surfacing an immediate failure, for operations where that makes sense

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The only class exported from `index.ts`, alongside the feature's domain error types
- Queueing is an explicit, per-operation opt-in — never automatic for every method

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Facade | `{Feature}Facade` | `OrdersFacade` | `{feature}.facade.ts` | `orders.facade.ts` |
| Facade spec | `{Feature}Facade` (tested) | `OrdersFacade` | `{feature}.facade-and-store.spec.ts` | `orders.facade-and-store.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-facade
// Plateau: monitored-app
// Version: 20260711220000

@Injectable({ providedIn: 'root' })
export class OrdersFacade {
  constructor(
    private readonly client: OrdersClient,
    private readonly queue: MutationQueueService,
  ) {}

  async addOrder(input: AddOrderInput): Promise<Order | { queued: true }> {
    if (input.quantity <= 0) {
      throw new OrdersValidationError('quantity must be positive');
    }
    try {
      return await this.client.addOrder(input);
    } catch (error) {
      if (error instanceof OfflineTransportError) {
        await this.queue.enqueue({
          feature: 'orders',
          operationName: 'addOrder',
          payload: input,
          touchedFields: Object.keys(input),
        });
        return { queued: true };
      }
      if (error instanceof OrdersConflictError) {
        throw new OrdersAlreadySubmittedError(input.id, { cause: error });
      }
      throw error;
    }
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Rules

## MUST
- The Facade MUST be the only class in this feature's `data-access` project exported from `index.ts`, along with domain error types.
- Business-rule validation not requiring a network call MUST happen in the Facade, before calling the Client.
- Any error surfaced by the Facade MUST be one of this feature's typed domain errors, never a raw `HttpErrorResponse`.
- A Facade MUST explicitly opt an operation into queueing by catching `OfflineTransportError` and calling `MutationQueueService.enqueue` — queueing is never automatic for every method.
- A queued operation's return type MUST clearly distinguish "queued for later" from an immediate successful result.
- A Facade test MUST fake its Client directly — it MUST NOT use `HttpTestingController` or MSW.

## MUST NOT
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client
- **Enqueueing an operation whose business validation already failed**
  - Consequence: queues a command that will never succeed, wasting a replay attempt
  - Instead: business validation always runs and fails before any queueing decision is considered
- **A Facade test faking HTTP instead of the Client**
  - Consequence: reintroduces the duplicated-mock risk this layering exists to prevent
  - Instead: fake the Client directly

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Check list

- [ ] The Facade contains all business-rule validation for this feature's data operations
- [ ] The Facade never performs DTO mapping or calls `http-core`/`HttpClient` directly
- [ ] Only operations explicitly marked queueable catch `OfflineTransportError` and enqueue
- [ ] Every Facade test fakes the Client, never HTTP directly

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Unittest TestCases

- [ ] WHEN `addOrder` is called with invalid business input THEN
  - [ ] the Facade throws a validation error without calling the (faked) Client
- [ ] WHEN the Client throws a transport-level conflict error THEN
  - [ ] the Facade re-wraps it into a more specific business error where applicable
- [ ] WHEN a queueable operation's Client call throws `OfflineTransportError` THEN
  - [ ] the Facade enqueues the operation and returns a queued indicator instead of throwing
- [ ] WHEN business validation fails before the Client is called THEN
  - [ ] the Facade throws its validation error and does not enqueue anything

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|DataAccess/{Feature}.project.create/{feature}.facade.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]
