---
name: solution-entity-invariant
description: Defines the Customer entity and its guarded ChangeEmail method — every state change validates before assignment, using the state's own value object
whenToUse: when adding a new state-mutating method to an entity, or creating a new entity that needs at least one guarded mutation
domain: skill
type: architecture
kind: mechanism
group: "[[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/entity-invariant
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Domain.Entities.Customer.cs"
extends:
  - "{Module}.Domain.csproj"
depends_on:
  - "[[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
  - "[[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
adr:
---

# Goal
- Make an invalid `Customer` state unreachable: every method that changes state validates first and throws instead of assigning a bad value.
- Keep `Customer` the single source of truth for its own validity.

# Core Principle
- `Customer.ChangeEmail` takes an already-valid `Email` (see [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]) — it does not re-validate the email's format, only its own invariant (here: the new email must differ from the current one).
- A property has exactly one mutation point; no other method on `Customer` assigns `Email`.

# Boundaries
- The exception thrown by a failed invariant is not caught here — the same global exception-handling gap noted in [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]'s Boundaries applies to this solution too.

# Requirements
SOLUTION:
- [[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]
  - [[../solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - provides the `/Entities` folder this solution's file lives in
- [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]
  - [[../solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs]] - the type `ChangeEmail` accepts as its parameter

# Template Skill Mutations
PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - add the `Customer` entity
  - [[./Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs]] - create - entity with a guarded `ChangeEmail` method

# Rule

## MUST
- [[./Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md#MUST|Customer.cs.create]]
- Validate before assigning, in every method that changes `Customer` state.
  - Risk: assigning first and validating after leaves invalid state reachable, even if only briefly.
  - Fix: check the invariant, throw `DomainException` on failure, assign only afterward.

# Check list
- [ ] `Customer.Email` has exactly one method that assigns it (`ChangeEmail`).
- [ ] Calling `ChangeEmail` with the customer's current email throws instead of silently succeeding.
