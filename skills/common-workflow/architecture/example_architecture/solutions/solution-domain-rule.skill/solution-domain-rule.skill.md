---
name: solution-domain-rule
description: Centralizes the "valid email format" condition, once duplicated between Email's constructor and ChangeCustomerEmailCommandValidator, into one EmailRule static class, then redirects both to call it
whenToUse: after solution-condition-ownership has found the same condition duplicated across two or more owners and selected the "centralize" branch
domain: skill
type: architecture
kind: mechanism
group: "[[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/domain-rule
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Domain.Rules.EmailRule.cs"
extends:
  - "{Module}.Domain.ValueObjects.Email.cs"
  - "{Module}.Application.Commands.ChangeCustomerEmailCommandValidator.cs"
depends_on:
  - "[[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]"
adr:
---

# Goal
- Give the "valid email format" condition exactly one place it is declared, once it is needed by both `Email`'s constructor and `ChangeCustomerEmailCommandValidator`.
- Redirect both existing owners to call the shared `EmailRule` instead of keeping their own copies.

# Core Principle
- `EmailRule.IsValid(string)` is the one place the condition is written; `EmailRule.Check(string)` throws `DomainException` for the `Email` side, `EmailRule.IsValid` is also exposed for the FluentValidation `.Must(...)` side.
- Redirecting an existing owner means deleting its local copy of the condition, not adding a second check next to it.

# Requirements
SOLUTION:
- [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]
  - [[../solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs]] - redirected to call `EmailRule.Check`
- [[../solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]
  - [[../solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs]] - redirected to call `EmailRule.IsValid`

This solution's own group is [[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]], but one of its two extend-targets (`ChangeCustomerEmailCommandValidator.cs`) belongs to `solution-transport-validation` in [[../../groups/group-request-handling.skill/group-request-handling.skill.md|group-request-handling]]. That is why this solution — unlike every other solution in this example — declares a narrow `depends_on` on one specific solution in the other group instead of relying on the group-level edge: it has a concrete `Implementation/*.extend.md` target inside `solution-transport-validation`, which is exactly the exception [[../../example-architecture.skill.md|example-architecture]]'s rule allows.

# Template Skill Mutations
PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - add `EmailRule` and redirect `Email`
  - [[./Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs]] - create - the centralized condition
  - [[./Implementation/{Module}.Domain.csproj.extend/Email.cs.extend.md|Email.cs]] - extend - call `EmailRule.Check` instead of the local predicate
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - redirect the transport validator
  - [[./Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.extend.md|ChangeCustomerEmailCommandValidator.cs]] - extend - call `EmailRule.IsValid` instead of the local `.Must(...)`

# Rule

## MUST
- [[./Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md#MUST|EmailRule.cs.create]]
- Remove the local condition from every owner this solution redirects — never leave the old copy next to the new call.
  - Risk: a leftover local copy can drift from `EmailRule` the next time either one is edited, silently reintroducing the duplication this solution exists to remove.
  - Fix: delete `Email`'s private predicate and the validator's inline `.Must(...)` body when redirecting them.

# Check list
- [ ] `Email`'s constructor no longer contains its own format predicate — it calls `EmailRule.Check`.
- [ ] `ChangeCustomerEmailCommandValidator` no longer contains its own `.Must(...)` body — it calls `EmailRule.IsValid`.
- [ ] `EmailRule` has no reference to `{Module}.Application` or `{Module}.Domain`'s entities — only the primitive value.
