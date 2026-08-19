---
name: solution-domain-behaviour
description: Defines how domain entities change state, enforce invariants through domain rules, and extract bulky logic into static domain service extension methods
domain: skill
type: architecture
version: 20260819
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - entity
  - behavior
  - invariants
  - concern/architecture
  - solution/domain-behaviour

triggers:
  - implement entity behavior
  - entity invariant enforcement
  - entity domain method
  - extract bulky logic from entity
  - add domain service
creates:
  - "{Module}.Domain.Services.{Behavior}Service.cs"
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]"
---

# Goal
- Define how entities mutate state while keeping invalid states unreachable
- Ensure every state change is validated through domain rules before assignment
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
- Every method or setter that changes state must validate before changing
- Invalid state must never be reachable — throw `DomainException` if attempted
- Domain rules are the only source of business predicates used during validation
- Bulky behavior is extracted to static domain service extension methods in `{Module}.Domain/Services`
- A property must not have multiple uncoordinated mutation points

# Boundaries
- `DomainException` thrown on invariant violation is not caught by this solution — some global exception-handling mechanism is expected to catch it. `solution-mediator-exception-handler` currently does this when applied, but this solution does not require it.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts entities and domain services
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]] - entity pattern extended with behavior methods
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - provides Value Object and domain rule patterns
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]] - domain rules used to validate state changes
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]] - immutable Value Objects used by entity behavior

NUGET:
- None — relies only on patterns defined by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - extend - Add Services folder for static domain service extensions
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]] - extend - Add behavior methods that enforce invariants through domain rules
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]] - create - Static extension methods for bulky entity behavior

# Rules

## MUST:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create#MUST|{Behavior}Service.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST|{EntityName}.cs]]

## SHOULD:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend#SHOULD|{Module}.Domain.csproj]]

## MUST NOT:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend#MUST NOT|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create#MUST NOT|{Behavior}Service.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST NOT|{EntityName}.cs]]

# Anti-patterns
- Entity has several points changing the same property with separate validation
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside entity methods instead of calling rules from `{Module}.Domain/Rules`
- Service extension holds state or depends on infrastructure
- Same business condition checked in controller, validator, entity, and service separately

# Check list
- [ ] Entity prevents invalid state
- [ ] Every mutation validates before assigning
- [ ] `DomainException` thrown on invariant violation
- [ ] Domain rules from `{Module}.Domain/Rules` used for all validation
- [ ] Complex logic extracted to `{Module}.Domain/Services` static extension methods
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions mutate state only through entity methods or guarded setters
- [ ] Unit test use cases implemented and passed
