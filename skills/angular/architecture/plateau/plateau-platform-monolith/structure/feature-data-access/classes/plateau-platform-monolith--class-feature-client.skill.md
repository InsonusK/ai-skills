---
name: plateau-platform-monolith--class-feature-client
description: Generic pattern for a feature's Client — the internal transport layer of its data-access lib, mapping DTOs and translating every transport failure (including offline) into a typed domain error — platform-monolith plateau
domain: skill
type: template
plateau: platform-monolith
artifact_type: service
version: 20260711210000
tags:
  - skill/template/class
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
---

> Generic pattern, not tied to one concrete feature.

# Goal

- Map between DTOs and domain models, and perform the actual HTTP call via `libs/shared/http-core`, entirely hidden behind the Facade
- Let the Facade (and the offline-sync replay orchestrator) distinguish "this failed because we're offline" from "the server rejected this request"

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Never exported from the feature's `index.ts` — only its own Facade calls it
- Every error path ends in a typed domain error; a network-level failure (`status === 0`) is checked before any status-code-specific handling and throws the shared `OfflineTransportError`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | --------------- | -------------------- | --------- |
| Client | `{Feature}Client` | `OrdersClient` | `{feature}.client.ts` | `orders.client.ts` |
| Client spec | `{Feature}Client` (tested) | `OrdersClient` | `{feature}.client.spec.ts` | `orders.client.spec.ts` |

# Implementation

```typescript
// Skill: class-feature-client
// Plateau: platform-monolith
// Version: 20260711210000

@Injectable({ providedIn: 'root' })
export class OrdersClient {
  constructor(private readonly http: BaseHttpService) {}

  async addOrder(input: AddOrderInput): Promise<Order> {
    const dto = orderModelToDto(input);
    try {
      const responseDto = await firstValueFrom(this.http.post<OrderDto>('/orders', dto));
      return orderDtoToModel(responseDto);
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 0) {
        // no response ever received — a network-level failure, not a server response
        throw new OfflineTransportError('addOrder', { cause: error });
      }
      if (error instanceof HttpErrorResponse && error.status === 409) {
        throw new OrdersConflictError(input.id, { cause: error });
      }
      throw new OrdersAddError('unexpected error adding order', { cause: error });
    }
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Rules

## MUST
- The Client MUST NOT be exported from the feature's `index.ts`.
- The Client MUST call `libs/shared/http-core`'s base service, not `HttpClient` directly.
- Every Client method's error handling MUST check for a network-level failure (`status === 0`) before any status-code-specific handling, and throw the shared `OfflineTransportError` in that case.
- DTO ↔ domain model mapping MUST go through this feature's `{feature}.mapper.ts` functions, never inlined ad hoc.
- Every test in this file's spec MUST use `HttpTestingController` to assert the exact request (method, URL, body), and `httpTesting.verify()` MUST run in `afterEach`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create|Testing/{feature}.client.spec.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Letting an `HttpErrorResponse` propagate out of a Client method uncaught**
  - Consequence: callers end up branching on HTTP status codes instead of a meaningful domain error
  - Instead: catch every failure path and rethrow a typed domain error
- **Treating a `status === 0` failure the same as any other server error**
  - Consequence: the Facade (and the offline-sync replay orchestrator) has no reliable way to tell "we're offline, retryable later" apart from "the server actively rejected this"
  - Instead: always check for the network-level failure first
- **Using `HttpTestingController` inside a Facade or Signal Store test "to save time faking the Client"**
  - Consequence: the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: `HttpTestingController` is used only inside this Client's own spec file

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create|Testing/{feature}.client.spec.ts.create]]

# Check list

- [ ] The Client is never exported from the feature's `index.ts`
- [ ] Every HTTP call goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] Every possible error path ends in a typed domain error, including `OfflineTransportError`
- [ ] Every Client method has at least one success-path and one failure-path test, each using `HttpTestingController`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create|Testing/{feature}.client.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN the backend returns a 409 conflict THEN
  - [ ] the Client throws `{Feature}ConflictError`, not the raw `HttpErrorResponse`
- [ ] WHEN the backend returns a successful response THEN
  - [ ] the Client returns a correctly mapped domain model
- [ ] WHEN a Client method's HTTP call fails with no response received (network unreachable) THEN
  - [ ] it throws the shared `OfflineTransportError`
- [ ] WHEN a Client method's HTTP call fails with an actual server response (e.g. 409, 500) THEN
  - [ ] it throws that feature's own typed domain error, unaffected by the offline extension

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|DataAccess/{Feature}.project.create/{feature}.client.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create|Testing/{feature}.client.spec.ts.create]]
