---
name: solution-transport-validation
description: Defines ChangeCustomerEmailCommandValidator — a FluentValidation validator that rejects a malformed command before the handler runs, checking transport shape only
whenToUse: when a command needs input-shape checks (missing/malformed fields) before its handler runs
domain: skill
type: architecture
kind: mechanism
group: "[[../../groups/group-request-handling.skill/group-request-handling.skill.md|group-request-handling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/transport-validation
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Application.Commands.ChangeCustomerEmailCommandValidator.cs"
extends:
  - "{Module}.Application.csproj"
depends_on:
  - "[[../solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]"
adr:
---

# Goal
- Reject a malformed `ChangeCustomerEmailCommand` before `ChangeCustomerEmailHandler` ever runs.
- Keep this check about input shape only — not a business decision, which stays inside `Email`/`Customer`.

# Core Principle
- `ChangeCustomerEmailCommandValidator` owns its own condition, written locally in this file, exactly like [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] does for `Email` — the two are written independently today, and this solution does not guarantee they agree.
- This is deliberate: the same "is this a valid email" condition now exists in two places (`Email`'s constructor and this validator). That duplication is exactly the trigger [[../solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] watches for.

# Requirements
SOLUTION:
- [[../solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]
  - [[../solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs]] - the type this validator validates
NUGET:
- FluentValidation
  - `AbstractValidator<ChangeCustomerEmailCommand>` - base class for the validator

# Template Skill Mutations
PROJECT:
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - add the command validator
  - [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs]] - create - transport-shape check on `NewEmail`

# Rule

## MUST
- [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md#MUST|ChangeCustomerEmailCommandValidator.cs.create]]
- Check input shape only (empty, malformed) — never a business decision that belongs to `Customer`.
  - Risk: a business rule inside the validator (e.g. "customer must be active") duplicates a decision the domain layer should own, and the two can silently disagree.
  - Fix: keep this validator to transport-shape checks; business invariants stay in `Customer`/`Email`.

# Check list
- [ ] `ChangeCustomerEmailCommandValidator` rejects an empty `NewEmail`.
- [ ] `ChangeCustomerEmailCommandValidator` contains no check that reads or depends on `Customer`'s current state.
