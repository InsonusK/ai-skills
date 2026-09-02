---
description: Generic pattern for a feature's Client — the internal transport layer of its data-access lib, never exported outside it
project_name: "{Feature}"
name: "{feature}"
element_kind: service
change_kind: create
tags:
  - solution/api-http-layer
  - element/feature-client-ts
---

# How this generic file is used
This is not tied to one concrete feature. Any solution that creates a new `libs/{feature}/data-access` project follows this pattern for its internal transport layer.

# Goals

- Map between DTOs and domain models, and perform the actual HTTP call via `libs/shared/http-core`, entirely hidden behind the Facade

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | --------------- | -------------------- | --------- |
| Client | {Feature}Client | OrdersClient | {feature}.client.ts | orders.client.ts |
| Multiple facets | {Feature}_{Facet}Client | Orders_PaymentClient | client/{feature}_{facet}.client.ts | client/orders_payment.client.ts |

# Implementation changes

```typescript
@Injectable({ providedIn: 'root' })
export class OrdersClient {
  constructor(
    private readonly http: BaseHttpService,
    private readonly store: Store, // only when a mapping needs enrichment from shared state
  ) {}

  async addOrder(input: AddOrderInput): Promise<Order> {
    const tenantId = this.store.selectSignal(selectTenantId)(); // enrichment example
    const dto = orderModelToDto(input, { tenantId });
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

# Rule changes

## MUST
- The Client MUST NOT be exported from the feature's `index.ts` — it is only ever called by that feature's own Facade.
- The Client MUST call `libs/shared/http-core`'s base service, not `HttpClient` directly.
- The Client MUST catch every `HttpErrorResponse` its calls can produce and rethrow one of this feature's typed domain errors from `{feature}.errors.ts` — never let the raw error escape, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|error-handling-strategy]].
- DTO ↔ domain model mapping MUST go through this feature's `{feature}.mapper.ts` hand-written functions, per [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy.md|dto-mapping-strategy]] — never inlined ad hoc inside the Client method.

## SHOULD
- **Letting an `HttpErrorResponse` propagate out of a Client method uncaught** — Consequence: callers (Facade, and transitively Signal Store methods/effects) end up branching on HTTP status codes instead of a meaningful domain error — Instead: catch every failure path and rethrow a typed domain error specific to this feature
- **Inlining ad hoc field mapping inside the HTTP call instead of using the feature's mapper functions** — Consequence: mapping logic is duplicated or inconsistent across multiple Client methods, and harder to unit test in isolation from HTTP — Instead: always route through `{feature}.mapper.ts`'s `dtoToModel`/`modelToDto` functions

# Check list

- [ ] The Client is never exported from the feature's `index.ts`
- [ ] Every HTTP call goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] Every possible error path ends in a typed domain error, never a raw `HttpErrorResponse`
- [ ] All DTO/model conversion goes through `{feature}.mapper.ts`

# Unittest TestCases

- [ ] WHEN the backend returns a 409 conflict THEN
  - [ ] the Client throws `{Feature}ConflictError`, not the raw `HttpErrorResponse`
- [ ] WHEN the backend returns a successful response THEN
  - [ ] the Client returns a correctly mapped domain model, using `{feature}.mapper.ts`
