---
name: solution-command-handler
description: Defines ChangeCustomerEmailCommand and its handler — the handler loads the entity, calls exactly one guarded domain method, and never validates business rules itself
whenToUse: when adding a new write operation for this module
domain: skill
type: architecture
kind: mechanism
group: "[[../../groups/group-request-handling.skill/group-request-handling.skill.md|group-request-handling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/command-handler
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Application.Commands.ChangeCustomerEmailCommand.cs"
  - "{Module}.Application.Commands.ChangeCustomerEmailHandler.cs"
extends:
  - "{Module}.Application.csproj"
depends_on:
  - "[[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
adr:
---

# Goal
- Give every write operation the same shape: a Command record and a Handler that loads state, calls one guarded domain method, and returns.
- Keep business logic entirely out of the handler — it orchestrates, it never decides.

# Core Principle
- `ChangeCustomerEmailHandler` loads `Customer`, constructs `Email`, and calls `customer.ChangeEmail(email)` — it does not re-check the invariant `ChangeEmail` already enforces.
- The handler never contains an `if` that decides whether the request is valid; that decision belongs to the domain method it calls.

# Boundaries
- This solution's group, [[../../groups/group-request-handling.skill/group-request-handling.skill.md|group-request-handling]], `depends_on` [[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]] as a whole (see that group's own ADR) — this solution therefore assumes `Customer`/`Email` already exist, without repeating that dependency here itself.
- Persisting the change (calling `SaveChanges` or equivalent) is out of scope for this example — a real module needs a commit mechanism such as the real catalog's `solution-unit-of-work`.

# Requirements
SOLUTION:
- [[../solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]
  - [[../solution-module-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - provides the `/Commands` folder this solution's files live in

# Template Skill Mutations
PROJECT:
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - add the command and its handler
  - [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs]] - create - immutable write-intent record
  - [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs]] - create - loads `Customer`, calls its guarded method

# Workflow

## Change a customer's email (happy path)
1. Caller sends `ChangeCustomerEmailCommand`.
2. `ChangeCustomerEmailHandler` loads the `Customer` by `CustomerId`.
3. The handler constructs `Email` from `NewEmail` — an invalid format throws here, inside `Email`'s own constructor.
4. The handler calls `customer.ChangeEmail(email)` — an unchanged email throws here, inside `Customer`'s own invariant.
5. The handler returns; nothing in the handler itself decided validity.

# Rule

## MUST
- [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md#MUST|ChangeCustomerEmailHandler.cs.create]]
- Never re-implement a check inside the handler that a domain method it calls already enforces.
  - Risk: two copies of the same condition (one in the handler, one in `Customer`) drift apart the first time either one is edited.
  - Fix: let `Email`'s constructor and `Customer.ChangeEmail` be the only places that reject invalid data; the handler only orchestrates.

# Check list
- [ ] `ChangeCustomerEmailHandler` contains no `if` that duplicates a check already made by `Email` or `Customer`.
- [ ] `ChangeCustomerEmailCommand` is an immutable record.
