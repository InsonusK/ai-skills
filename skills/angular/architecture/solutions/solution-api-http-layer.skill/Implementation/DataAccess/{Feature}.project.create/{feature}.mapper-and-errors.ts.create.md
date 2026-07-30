---
description: Generic pattern for a feature's hand-written DTO/domain mapper functions and its typed domain error hierarchy
project_name: "{Feature}"
name: "{feature}"
element_kind: functions
change_kind: create
---

# How this generic file is used
This is not tied to one concrete feature. It covers two small, related files every feature's `data-access` project has: `{feature}.mapper.ts` and `{feature}.errors.ts`.

# Goals

- Give every field conversion between DTO and domain model an explicit, reviewable, unit-testable function
- Give every feature a small, typed set of domain errors its Client can throw and its Facade/callers can narrow on

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ----------------- | -------------------- | --------- |
| Mapper functions | {feature}DtoToModel / {feature}ModelToDto | orderDtoToModel / orderModelToDto | {feature}.mapper.ts | orders.mapper.ts |
| Multiple facets | {feature}_{facet}DtoToModel / {feature}_{facet}ModelToDto | orders_paymentDtoToModel / orders_paymentModelToDto | mapper/{feature}_{facet}.mapper.ts | mapper/orders_payment.mapper.ts |
| Domain error classes | {Feature}{Reason}Error | OrdersConflictError | {feature}.errors.ts | orders.errors.ts |

# Implementation changes

```typescript
// orders.mapper.ts
export function orderDtoToModel(dto: OrderDto): Order {
  return {
    id: dto.id,
    quantity: dto.qty, // explicit, reviewable rename
    createdAt: new Date(dto.created_at),
  };
}

export function orderModelToDto(model: AddOrderInput, context: { tenantId: string }): AddOrderDto {
  return {
    qty: model.quantity,
    tenant_id: context.tenantId, // enrichment from context the DTO alone would not carry
  };
}
```

```typescript
// orders.errors.ts
export class OrdersConflictError extends Error {
  constructor(public readonly orderId: string, options?: ErrorOptions) {
    super(`Order ${orderId} conflicts with an existing record`, options);
  }
}

export class OrdersAddError extends Error {}
export class OrdersValidationError extends Error {}
```

# Rule changes

## MUST
- Every field conversion between a DTO and its domain model MUST go through an explicit function in `{feature}.mapper.ts` — no inline field renaming elsewhere.
- Every domain error thrown by this feature's Client or Facade MUST be one of the classes defined in `{feature}.errors.ts`.

# Anti-patterns

- **A mapper function silently dropping a DTO field with no domain equivalent, with no comment explaining why**
  - Consequence: a future reader cannot tell whether the omission was intentional or a bug
  - Instead: if a DTO field is intentionally unused, leave a short comment stating so

# Check list

- [ ] Every DTO ↔ model conversion has a corresponding function in `{feature}.mapper.ts`
- [ ] Every error type thrown by this feature's data-access code is declared in `{feature}.errors.ts`

# Unittest TestCases

- [ ] WHEN `{feature}DtoToModel` is given a valid DTO THEN
  - [ ] it returns the correctly shaped domain model
- [ ] WHEN `{feature}ModelToDto` is given a domain model and required context THEN
  - [ ] it returns a DTO with all enrichment fields correctly populated
