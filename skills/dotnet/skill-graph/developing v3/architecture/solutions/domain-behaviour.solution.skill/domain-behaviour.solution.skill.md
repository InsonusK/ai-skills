---
uid: 95ddb3de-2d54-4890-b075-e86bab2e38c0
name: domain-behaviour
description: Defines how domain entities change state, enforce invariants through domain rules, and extract bulky logic into static domain service extension methods
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - entity
  - behavior
  - invariants
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
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules.solution.skill]]"
---

# Goal
- Define how entities mutate state while keeping invalid states unreachable
- Ensure every state change is validated through domain rules before assignment
- Allow bulky or multi-step domain logic to be extracted from entities without scattering property mutation points
- Keep entity behavior as the single source of truth for invariant enforcement

# Core Principals
- Entity is the single point of truth for its own state validity
- Every method or setter that changes state must validate before changing
- Invalid state must never be reachable — throw `DomainException` if attempted
- Domain rules are the only source of business predicates used during validation
- Bulky behavior is extracted to static domain service extension methods in `{Module}.Domain/Services`
- A property must not have multiple uncoordinated mutation points

# Requirements
- definition of `module project structure` — [[solution-structure.solution.skill]] defines the module projects that this solution extends
- definition of `Entity` — [[solution-structure.solution.skill]] defines the entity pattern this solution extends
- definition of `domain rules` — [[value-objects-and-rules.solution.skill]] defines the domain rule pattern used for validation

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Services folder for static domain service extensions
  - [[./Implementation/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add behavior methods that enforce invariants through domain rules
  - [[./Implementation/{Behavior}Service.cs.create.md|{Behavior}Service.cs]] - create - Static extension methods for bulky entity behavior

# Rules

MUST:
- Every entity property mutation validates state through domain rules before assigning
- Every entity method that changes state validates through domain rules before mutating
- `DomainException` thrown when a rule returns `false`
- Bulky logic extracted to static extension methods in `{Module}.Domain/Services`
- Service extensions delegate all validation to domain rules
- A single entity property must not have multiple uncoordinated public mutation points

SHOULD:
- Keep entity methods small and delegate complex calculations to service extensions
- Name service files after the behavior they encapsulate

MUST NOT:
- Reimplement rule logic inline inside entity methods or service extensions
- Mutate state before validating with rules
- Allow invalid state to persist silently
- Let a service extension bypass entity methods and write directly to properties
- Duplicate invariant logic across setters, methods, or service extensions

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

# Unittest TestCases
- [ ] When valid value set Then state changes correctly
- [ ] When invalid value set Then `DomainException` thrown
- [ ] When behavior method called with invalid args Then `DomainException` thrown
- [ ] When service extension called with invalid args Then `DomainException` thrown
- [ ] When service extension called Then state changes only through entity guarded method
- [ ] When rule returns false Then entity does not mutate state
