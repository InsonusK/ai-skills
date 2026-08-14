---
description: Add cross-module reusable Value Objects and rules to Shared project
name: Shared.csproj
element_kind: project
change_kind: extend
---

# Goals
- Host Value Objects and rules that are reused by two or more module Domain projects
- Prevent duplication of identical domain logic across module boundaries
- Provide a single source of truth for cross-cutting domain primitives

# Core Principles
- Shared contains only cross-cutting primitives — no business logic specific to a single module
- Any project at any layer may depend on Shared
- Shared VOs and rules follow the same patterns as module-local ones

# Structure

## Project Structure
```
/Shared
  /ValueObjects
    Email.cs
    Money.cs
  /Rules
    StringRules.cs
    IntRules.cs
  /Exceptions
    DomainException.cs
  Shared.csproj
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /ValueObjects | Cross-module reusable Value Object types |
| /Rules | Cross-module reusable domain rule static classes |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Module-specific Value Objects — belong in respective `{Module}.Domain/ValueObjects`
- Module-specific rules — belong in respective `{Module}.Domain/Rules`
- Business logic — belongs to Domain
- Infrastructure implementations — belong to BuildingBlocks or App.Infrastructure

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

## MUST
- A VO or rule lives in Shared only when used by two or more modules
- Shared VOs follow the same `sealed record`, immutable, self-validating rules as module VOs
- Shared rules follow the same static extension method, bool-return, stateless rules as module rules
- Value Objects live in `/{Module}.Domain/ValueObjects` or `/Shared/ValueObjects` when cross-module
- All rules live in `/{Module}.Domain/Rules` or `/Shared/Rules` when cross-module

## MUST NOT
- Place module-specific VO or rule in Shared
- Add project references to Shared.csproj
- Put business logic in Shared VOs or rules

# Anti-patterns
- Putting every VO/rule in Shared "just in case" — Shared should stay minimal
- Duplicating a Shared VO/rule back into a module Domain project
- Adding module-specific behavior to a Shared VO or rule

# Check list
- [ ] /ValueObjects folder exists in Shared
- [ ] /Rules folder exists in Shared
- [ ] Every VO in /Shared/ValueObjects is referenced by at least two modules
- [ ] Every rule in /Shared/Rules is referenced by at least two modules
- [ ] Shared.csproj has no project references
