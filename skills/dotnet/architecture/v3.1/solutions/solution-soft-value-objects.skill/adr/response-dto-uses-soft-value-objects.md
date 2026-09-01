---
name: response-dto-uses-soft-value-objects
description: Whether a ResponseDto property may be typed as the domain {ValueObject} directly, or must use Soft{ValueObject}/primitives declared in {Module}.Interfaces
problem: A ResponseDto in {Module}.Interfaces needs to expose a value that the domain models as a Value Object. Typing the DTO property as the domain {ValueObject} directly would require {Module}.Interfaces to reference {Module}.Domain, or the {ValueObject} class itself to move into {Module}.Interfaces.
decision: All {ValueObject} stays sealed in {Module}.Domain. ResponseDto (and RequestDto) properties use Soft{ValueObject} — or the underlying primitive when no Soft{ValueObject} exists — declared in {Module}.Interfaces. The Application layer maps {ValueObject} to Soft{ValueObject}/primitive when producing a DTO.
tags:
  - solution/value-objects
  - concern/documentation
  - concern/documentation/adr
---

# Problem

A DTO property that represents a value-object concept (e.g. `Email`, `Money`) needs a type. Two constraints collide:

- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-value-objects.skill|solution-value-objects]] requires the domain `{ValueObject}` to live in `{Module}.Domain` and be the only layer that contains entity/value-object definitions.
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] requires `{Module}.Interfaces` to stay declarations-only, to depend on nothing but `Shared`, and forbids any other module from referencing `{Module}.Domain`.

Typing a `ResponseDto` property as the domain `{ValueObject}` needs one of these to give way: either `{Module}.Interfaces` gains a dependency on `{Module}.Domain` (or a project reference exception is carved out for VO types), or the `{ValueObject}` class itself relocates from Domain into Interfaces.

Decide where a DTO-exposed value-object-shaped property gets its type from, without breaking either rule.

# Selected variant

**Selected variant:** [[#ResponseDto uses Soft{ValueObject} or primitive, mapped from the domain {ValueObject}]]

`{ValueObject}` stays sealed in `{Module}.Domain` and keeps throwing `DomainException` on invalid construction, per [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-value-objects.skill|solution-value-objects]]. `{Module}.Interfaces` exposes only `Soft{ValueObject}`, or a plain primitive when no `Soft{ValueObject}` exists for the concept. `{Module}.Application` maps `{ValueObject}` to `Soft{ValueObject}`/primitive when assembling a `ResponseDto`; for single-property VOs this is typically the existing implicit conversion operator.

This keeps the dependency direction intact: `{Module}.Interfaces` depends on nothing, `{Module}.Domain` is referenced by nobody outside the module, and consumers keep validating received shapes through `Soft{ValueObject}` without inheriting the producer's throw-on-construct semantics.

# Searched variants

## Move {ValueObject} from Domain to Interfaces

### Description

Relocate the strict `{ValueObject}` class itself into `{Module}.Interfaces` so DTOs can reference it directly, removing the mapping step entirely.

### Benefits

- No duplicate type and no mapping step between Domain and Interfaces
- `ResponseDto` gets the same invariant-enforcement guarantee "for free"

### Costs

- Breaks "`{Module}.Interfaces` contains only declarations" and "Domain is the only layer that contains entity definitions" — both explicit MUST rules
- Forces every consumer of `{Module}.Interfaces` (other modules, other systems reusing the contract) to accept `DomainException` throw-on-construct semantics even when they only want to read a shape — exactly the coupling `Soft{ValueObject}` was introduced to avoid
- Reintroduces EF Core materialization concerns (private parameterless constructor, `OwnsOne`) into a project that must stay free of persistence-shaped constraints
- Collapses the two-type split (`Soft{ValueObject}` in Interfaces, strict `{ValueObject}` in Domain) that [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/soft-and-strict-value-object-split|soft-and-strict-value-object-split]] deliberately kept, reopening a question that ADR already settled

## Reference {Module}.Domain from {Module}.Interfaces for VO types only

### Description

Keep `{ValueObject}` physically in `{Module}.Domain`, but add a project reference from `{Module}.Interfaces` to `{Module}.Domain` so `ResponseDto` can use the Domain type without moving the file.

### Benefits

- No code duplication, no mapping step
- Minimal file movement

### Costs

- Directly violates "Dependencies flow inward" and "Interfaces reference Domain..." MUST NOT rule
- Every other module that references this module's `{Module}.Interfaces` transitively pulls in `{Module}.Domain`, defeating "Other modules reference only `{ModuleName}.Interfaces`"
- One exception here erodes the rule for every future module — there is no principled way to allow it for VO types only

## ResponseDto uses Soft{ValueObject} or primitive, mapped from the domain {ValueObject}

### Description

`{ValueObject}` stays sealed in `{Module}.Domain`. `ResponseDto`/`RequestDto` properties in `{Module}.Interfaces` are typed as `Soft{ValueObject}` — or the underlying primitive when no `Soft{ValueObject}` exists — never as the domain `{ValueObject}`. `{Module}.Application` maps between the two when building a DTO.

### Benefits

- Preserves every previously accepted dependency-direction rule with no exceptions
- `{Module}.Interfaces` remains a stable, versioned, side-effect-free contract
- Consumers keep validating through `Soft{ValueObject}` without inheriting throw semantics they did not ask for

### Costs

- Requires a mapping step (`{ValueObject}` → `Soft{ValueObject}`/primitive) wherever a domain VO is projected into a DTO — typically a one-line conversion via the VO's existing implicit operator for single-property VOs, more explicit for multi-property VOs
