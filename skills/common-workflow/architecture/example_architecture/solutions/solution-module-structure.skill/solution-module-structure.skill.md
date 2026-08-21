---
name: solution-module-structure
description: Creates the two projects — {Module}.Domain and {Module}.Application — that every other solution in this example builds on
whenToUse: when starting a new module for this example architecture, before any value object, entity, command, or validator is written
domain: skill
type: architecture
kind: mechanism
version: 20260821
tags:
  - skill/architecture/solution
  - solution/module-structure
  - stack/dotnet
  - concern/architecture
creates:
  - "{Module}.Domain.csproj"
  - "{Module}.Application.csproj"
extends:
depends_on:
adr:
---

# Goal
- Give the module a fixed two-project skeleton — `{Module}.Domain` for invariants, `{Module}.Application` for request handling — before any group-level solution needs a place to put its files.

# Core Principle
- `{Module}.Domain` has no dependency on `{Module}.Application`; `{Module}.Application` references `{Module}.Domain`.
- This is the one solution every group in this example implicitly assumes exists — it belongs to no group, the same way `solution-sln-structure` sits outside every group in the real .NET catalog.

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - lay out the module's two projects
PROJECT:
- [[./Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - create - holds value objects and entities
- [[./Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - create - holds commands, handlers, and validators; references `{Module}.Domain`

# Rule

## MUST
- Keep `{Module}.Domain` free of any reference to `{Module}.Application`.
  - Risk: a domain type that references the application layer can no longer be reused or tested without pulling in request-handling concerns.
  - Fix: reference direction is always `{Module}.Application` → `{Module}.Domain`, never the reverse.

# Check list
- [ ] `{Module}.Domain.csproj` exists and has zero project references.
- [ ] `{Module}.Application.csproj` references `{Module}.Domain.csproj`.
