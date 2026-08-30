---
name: solution-domain-behaviour
description: Defines how domain entities change state — every method or setter that mutates state validates via its own local predicate first and throws DomainException on failure — and how bulky or multi-step logic is extracted into static domain service extension methods without losing that guarantee.
whenToUse: when adding or changing an Entity method that mutates state, or when entity behavior logic grows too large or multi-step to stay readable inside the Entity itself.
domain: skill
type: architecture
version: 20260820
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - entity
  - behavior
  - invariants
  - concern/architecture
  - solution/domain-behaviour
creates:
  - "{Module}.Domain.Services.{Behavior}Service.cs"
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
depends_on:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]]"
---

# Goal
- Define how entities mutate state while keeping invalid states unreachable
- Ensure every state change validates before assignment, using a condition this solution owns and writes locally
- Allow bulky or multi-step domain logic to be extracted from entities without scattering property mutation points
- Keep entity behavior as the single source of truth for invariant enforcement

# Capabilities
- Enforced invariant validation on every entity state change
- Single source of truth for entity validity
- Safe extraction of complex logic into reusable static domain services
- Prevention of invalid states reaching persistence
- Consistent `DomainException`-based error model

# Core Principles
- Entity is the single point of truth for its own state validity
- Every method or setter that changes state must validate before changing, using a condition written locally in that method or in a `private static` helper on the same class
- This solution does not require a shared rules abstraction: the condition is owned by the Entity/Service that enforces it. A later, optional `solution-domain-rules` may centralize a condition that turns out to be duplicated elsewhere, but every Entity method here already works standalone
- Invalid state must never be reachable — throw `DomainException` if attempted
- Bulky behavior is extracted to static domain service extension methods in `{Module}.Domain/Services`
- A property must not have multiple uncoordinated mutation points

# Boundaries
- `DomainException` thrown on invariant violation is not caught by this solution — some global exception-handling mechanism is expected to catch it. `solution-mediator-exception-handler` currently does this when applied, but this solution does not require it.
- Whether an Entity method's local condition agrees with the same concept's DTO/Command-side validation (`solution-dto-property-validators`) is not guaranteed or checked by this solution — the two are written independently today.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts entities and domain services
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]] - entity pattern extended with behavior methods
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]] - Value Objects used by entity properties and behavior methods

NUGET:
- None — relies only on patterns defined by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - extend - Add Services folder for static domain service extensions
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]] - extend - Add behavior methods that validate via a local condition before mutating state
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]] - create - Static extension methods for bulky entity behavior

# Rules

## MUST
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend#MUST|{Module}.Domain.csproj]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create#MUST|{Behavior}Service.cs]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST|{EntityName}.cs]]

## SHOULD
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#SHOULD|{EntityName}.cs]]

## MUST NOT
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend#MUST NOT|{Module}.Domain.csproj]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create#MUST NOT|{Behavior}Service.cs]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST NOT|{EntityName}.cs]]

# Check list
- [ ] Entity prevents invalid state
- [ ] Every mutation validates before assigning, using a locally-owned condition
- [ ] `DomainException` thrown on invariant violation
- [ ] Complex logic extracted to `{Module}.Domain/Services` static extension methods
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions mutate state only through entity methods or guarded setters
