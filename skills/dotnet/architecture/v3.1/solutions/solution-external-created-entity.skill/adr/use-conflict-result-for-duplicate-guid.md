---
name: use-conflict-result-for-duplicate-guid
description: How should the pipeline communicate a duplicate externally-created Guid back to the API layer?
problem: External-created entity commands carry a client-generated Guid. When the Guid already exists, the pipeline must short-circuit and the API must return HTTP 409. How should the pipeline express this conflict so that the API can return the existing entity information without throwing exceptions or adding dedicated middleware?
decision: Use a custom `ConflictResult<T>` that inherits from `Ardalis.Result.Result<T>` and carries the existing entity result. The resolver returns `ConflictResult<TResponse>` and `GuidResolvingBehavior` passes it through unchanged. The API layer detects the conflict result by type and maps it to HTTP 409.
tags:
  - solution/external-created-entity
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

External-created entity commands use a client-generated `Guid` as an idempotency key. The pipeline must detect when this Guid already exists and short-circuit before the handler runs. The API layer must then return HTTP 409 with information the client can use to recover.

Key constraints:
- The solution must not use exceptions for flow control (solution-command-integration.skill principle).
- The resolver must return the same response type as the command handler so that 201 Created and 409 Conflict have a symmetric API contract.
- The 409 response body should contain only the existing entity Id.
- No dedicated HTTP middleware should be required for conflict handling.

# Selected variant

[[#Return ConflictResult<T> from the resolver]]

- Keeps the command-integration principle of "no exceptions for flow control".
- `GuidResolvingBehavior` remains a thin pass-through and does not construct response DTOs.
- 201 and 409 return the same response type (`Result<CreateEntityResult>`), making the API contract predictable.
- `ConflictResult<T>` can carry the existing entity result, which the API layer maps to a 409 body.
- Removes the need for `ConflictException` and `ConflictExceptionMiddleware`.

# Searched variants

## Return ConflictException and catch in dedicated middleware

### Description
`GuidResolvingBehavior` throws `ConflictException<TResponse>` when a duplicate Guid is detected. A dedicated `ConflictExceptionMiddleware` catches the exception and writes HTTP 409 with the existing entity body.

### Benefits
- Centralized handling of all conflict exceptions in one middleware.
- Middleware can catch conflicts from any endpoint without per-controller code.

### Costs
- Uses exceptions for expected, non-exceptional control flow (duplicate Guid is a normal idempotency outcome).
- Requires an additional middleware component and its registration in App.Host.
- Adds a cross-cutting dependency on the exception type in both Shared and BuildingBlocks.
- Violates the solution-command-integration.skill principle that handlers/behaviors should not use exceptions for flow control.

## Return plain `Result<T>.Conflict()` without a value

### Description
`GuidResolvingBehavior` returns `Result<T>.Conflict()` when a duplicate Guid is detected. The API layer maps `ResultStatus.Conflict` to HTTP 409.

### Benefits
- No custom result subclass needed.
- Works with existing `Result<T>` to `ActionResult` mappers.

### Costs
- `Result<T>.Conflict()` has no overload that accepts a value, so the existing entity Id cannot be carried in the result.
- The client receives a generic 409 with no information about which entity already exists, forcing a second lookup.
- Does not satisfy the requirement to return the existing entity Id in the 409 body.

## Return ConflictResult<T> from the resolver

### Description
`IGuidResolver<TResponse>` returns `Task<TResponse?>`. When the Guid exists, the resolver returns `ConflictResult<TResponse>` carrying the existing entity result. `GuidResolvingBehavior` returns the resolver's result directly. The API layer detects `ConflictResult<T>` by type and returns HTTP 409 with the result body.

### Benefits
- No exceptions for flow control.
- Resolver and handler return the same response type, keeping the API contract symmetric.
- The existing entity result is carried in the result value and exposed in the 409 body.
- No dedicated middleware required.
- `GuidResolvingBehavior` stays focused on resolution coordination and does not construct response DTOs.

### Costs
- Requires a small custom `ConflictResult<T>` subclass in Shared.
- The API layer must detect `ConflictResult<T>` by type rather than relying solely on `ResultStatus.Conflict`.
- The resolver must construct the same minimal result record as the handler (`CreateEntityResult`).
