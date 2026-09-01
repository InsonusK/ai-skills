---
name: soft-and-strict-value-object-split
description: Where to place a domain concept's permissive and strict Value Object representations, and how they relate to each other
problem: Other modules and DTOs need to hold a value-object-shaped value without depending on {Module}.Domain or accepting throw-on-construct semantics, while {Module}.Domain still needs a self-validating, invariant-enforcing type for its own use.
decision: Place Soft{ValueObject} declarations in {Module}.Interfaces (permissive, no validation), and {ValueObject} in {Module}.Domain (strict, inherits from Soft{ValueObject}, validates via Check()).
tags:
  - solution/soft-value-objects
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

Other modules need to reference a value-object-shaped type owned by this module — in a DTO, a Command, or their own code — without referencing `{Module}.Domain` or a concrete FluentValidation validator type. At the same time, `{Module}.Domain` needs a self-validating type so invalid domain state cannot exist.

Adapted from the original `soft-value-objects-and-application-validators` ADR; this version covers only the Value Object placement/inheritance decision — validator placement is a separate decision, owned by the sibling solution `solution-dto-property-validators`.

# Selected variant

**Selected variant:** [[#Soft{ValueObject} in Interfaces, strict {ValueObject} in Domain, one inherits the other]]

- `{Module}.Interfaces` remains declarations-only and exposes the `Soft{ValueObject}` shape — a plain record, no validation
- `{Module}.Domain.ValueObjects.{ValueObject}` inherits from `Soft{ValueObject}` and enforces invariants by calling `Check()`

# Searched variants

## Keep validators in {Module}.Interfaces

### Description
Validators live next to `Soft{ValueObject}` in `{Module}.Interfaces`.

### Benefits
- Consumers can instantiate validators directly without DI

### Costs
- `{Module}.Interfaces` is no longer declarations-only, must reference FluentValidation
- Out of scope for this ADR anyway — validator placement is a separate decision (see `solution-dto-property-validators`)

## Single VO type, no Soft/strict split

### Description
Keep one `{ValueObject}` type, always strict, and require every consumer (including DTOs and other modules) to accept its throw-on-construct semantics.

### Benefits
- No duplicate type, no mapping step

### Costs
- Forces every consumer to accept `DomainException` throw-on-construct semantics even when they only want to read a shape
- A DTO carrying invalid client data cannot be constructed at all, defeating collect-all validation at the transport boundary
- `{Module}.Interfaces` would need to reference `{Module}.Domain` (or the type would need to move there), breaking the module's declarations-only contract

## Soft{ValueObject} in Interfaces, strict {ValueObject} in Domain, one inherits the other (selected)

### Description
`Soft{ValueObject}` is a plain, permissive record in `{Module}.Interfaces`. `{ValueObject}` in `{Module}.Domain` inherits from it and adds invariant enforcement via `Check()`. A `Soft{ValueObject}` does not require a matching `{ValueObject}` — some values only ever need the permissive strength.

### Benefits
- `{Module}.Interfaces` stays declarations-only, validation-agnostic, side-effect-free
- `{Module}.Domain` gets a self-enforcing invariant type without duplicating the shape
- A DTO with invalid client data can still be constructed and then validated through the collect-all mechanism, instead of failing at deserialization

### Costs
- Two types per concept instead of one, with an inheritance relationship to keep in mind
- `{Module}.Domain` must reference `{Module}.Interfaces` for the base type
