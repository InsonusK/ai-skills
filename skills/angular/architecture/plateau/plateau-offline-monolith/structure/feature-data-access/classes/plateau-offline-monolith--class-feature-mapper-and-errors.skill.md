---
name: plateau-offline-monolith--class-feature-mapper-and-errors
description: Generic pattern for a feature's hand-written DTO/domain mapper functions and its typed domain error hierarchy — offline-monolith plateau
domain: skill
type: template
plateau: offline-monolith
artifact_type: functions
version: 20260723041000
tags:
  - skill/template/class
  - plateau/offline-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
---

> Generic pattern, not tied to one concrete feature. Covers two small, related files every feature's `data-access` project has: `{feature}.mapper.ts` and `{feature}.errors.ts`.

# Goal

- Give every field conversion between DTO and domain model an explicit, reviewable, unit-testable function
- Give every feature a small, typed set of domain errors its Client can throw and its Facade/callers can narrow on

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- No inline field renaming anywhere else — every conversion goes through `{feature}.mapper.ts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ----------------- | -------------------- | --------- |
| Mapper functions | `{feature}DtoToModel` / `{feature}ModelToDto` | `orderDtoToModel` / `orderModelToDto` | `{feature}.mapper.ts` | `orders.mapper.ts` |
| Multiple facets | `{feature}_{facet}DtoToModel` / `{feature}_{facet}ModelToDto` | `orders_paymentDtoToModel` / `orders_paymentModelToDto` | `mapper/{feature}_{facet}.mapper.ts` | `mapper/orders_payment.mapper.ts` |
| Domain error classes | `{Feature}{Reason}Error` | `OrdersConflictError` | `{feature}.errors.ts` | `orders.errors.ts` |

# Implementation

```typescript
// Skill: class-feature-mapper-and-errors
// Plateau: offline-monolith
// Version: 20260711200000

// orders.mapper.ts
export function orderDtoToModel(dto: OrderDto): Order {
  return {
    id: dto.id,
    quantity: dto.qty, // explicit, reviewable rename
    createdAt: new Date(dto.created_at),
  };
}

export function orderModelToDto(model: AddOrderInput): AddOrderDto {
  return { qty: model.quantity };
}

// orders.errors.ts
export class OrdersConflictError extends Error {
  constructor(public readonly orderId: string, options?: ErrorOptions) {
    super(`Order ${orderId} conflicts with an existing record`, options);
  }
}

export class OrdersAddError extends Error {}
export class OrdersValidationError extends Error {}
export class OrdersAlreadySubmittedError extends Error {
  constructor(public readonly orderId: string, options?: ErrorOptions) {
    super(`Order ${orderId} was already submitted`, options);
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Rules

## MUST
- Every field conversion between a DTO and its domain model MUST go through an explicit function in `{feature}.mapper.ts`.
- Every domain error thrown by this feature's Client or Facade MUST be one of the classes defined in `{feature}.errors.ts` (or the shared `OfflineTransportError` from `libs/shared/http-core`).

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A mapper function silently dropping a DTO field with no domain equivalent, with no comment explaining why**
  - Consequence: a future reader cannot tell whether the omission was intentional or a bug
  - Instead: if a DTO field is intentionally unused, leave a short comment stating so

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Check list

- [ ] Every DTO ↔ model conversion has a corresponding function in `{feature}.mapper.ts`
- [ ] Every error type thrown by this feature's data-access code is declared in `{feature}.errors.ts`
- [ ] When a feature has multiple data facets, every mapper lives under `mapper/` with names `{feature}_{facet}DtoToModel` / `{feature}_{facet}ModelToDto`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]

# Unittest TestCases

- [ ] WHEN `{feature}DtoToModel` is given a valid DTO THEN
  - [ ] it returns the correctly shaped domain model
- [ ] WHEN `{feature}ModelToDto` is given a domain model THEN
  - [ ] it returns a correctly shaped DTO

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create]]
