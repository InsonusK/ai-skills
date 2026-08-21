---
name: solution-value-object
description: Defines a self-validating Value Object type — Email — that rejects an invalid value at construction, using a condition it owns and writes locally
whenToUse: when a primitive property on an entity or command carries business meaning (e.g. an email address) and needs to reject invalid values at the point of construction
domain: skill
type: architecture
kind: mechanism
group: "[[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/value-object
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Domain.ValueObjects.Email.cs"
extends:
  - "{Module}.Domain.csproj"
depends_on:
  - "[[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
adr:
---

# Goal
- Eliminate a bare `string` email property by giving it a dedicated type that cannot hold an invalid value.
- Keep the validating condition local to `Email` — no shared rules abstraction required.

# Core Principle
- `Email` is immutable and equal by value, not by reference.
- `Email`'s constructor validates via a condition written locally in this class — a `private static` predicate on `Email` itself.
- This solution is complete and usable entirely on its own. [[../solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] may later decide this condition should move into [[../solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] once a second owner needs it — this solution does not require or anticipate that.

# Boundaries
- The exception `Email`'s constructor throws on an invalid value is not caught by this solution — some global exception-handling mechanism is expected to turn it into a client-facing error. This example does not include that mechanism; the real `solution-mediator-exception-handler` in the .NET catalog plays that role today.

# Requirements
SOLUTION:
- [[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]
  - [[../solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - provides the `/ValueObjects` folder this solution's file lives in

# Template Skill Mutations
PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - add the `Email` value object
  - [[./Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs]] - create - self-validating email value object

# Rule

## MUST
- [[./Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md#MUST|Email.cs.create]]
- Throw on an invalid value inside the constructor — never return a partially-valid `Email`.
  - Risk: an `Email` instance that can hold `"not-an-email"` defeats the entire reason to have the type.
  - Fix: validate before assigning any field, and throw if the condition fails.

# Check list
- [ ] `Email` cannot be constructed with a value that fails its local condition.
- [ ] Two `Email` instances with the same string value are equal.
