---
description: Add ValueObjects and Rules folders to module Domain project
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Store all Value Object types for this bounded context
- Store all domain rule types for this bounded context
- Keep domain logic organized and discoverable

# Core Principals
- Value Objects define correctness — they encode domain semantics and enforce invariants at construction time
- Rules define predicates — they encode reusable business conditions without deciding enforcement
- Entities define consistency — they decide when and how to enforce invariants using VOs and rules

# Structure

## Project Structure
```
/{Module}.Domain
  /ValueObjects
    Age.cs
    Money.cs
    Email.cs
  /Rules
    IntRules.cs
    StringRules.cs
    AgeRules.cs
    CanDriveCarRule.cs
  /Entities
    Order.cs
    Driver.cs
```

## Directory and class skills
| Directory \| file | Description                                      | Pattern skill                                                                                                                                   |                                |
| ----------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ |
| /ValueObjects     | All Value Object types for this module           | Value Object pattern (this solution)                                                                                                            |                                |
| /Rules            | All domain rule static classes for this module   | Domain Rule pattern (this solution)                                                                                                             |                                |
| /Entities         | Domain entities that use Value Objects and rules | [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill.md\|solution-sln-structure.skill]] | solution-sln-structure.skill]] |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Infrastructure implementations — belong to App.Infrastructure or BuildingBlocks
- Application orchestration — belong to {Module}.Application
- Cross-module reusable VOs/rules — belong in Shared when used by 2+ modules
- EF Core configuration classes — belong in {Module}.Domain/Configurations per [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]

# Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (for multi-property VO `OwnsOne` configuration only)

# Rules

MUST:
- All Value Objects live in `/{Module}.Domain/ValueObjects`
- All domain rules live in `/{Module}.Domain/Rules`
- Only module-specific VOs and rules live here — cross-cutting ones belong in Shared

MUST NOT:
- Place Value Object definitions outside /ValueObjects folder
- Place rule definitions outside /Rules folder
- Put module-specific VO or rule in Shared

# Anti-patterns
- Scattering VO or rule classes across arbitrary folders in Domain
- Putting cross-module VO in {Module}.Domain instead of Shared
- Keeping duplicated copies of the same VO/rule in multiple module Domain projects

# Check list
- [ ] /ValueObjects folder exists in {Module}.Domain
- [ ] /Rules folder exists in {Module}.Domain
- [ ] All VOs are in /ValueObjects
- [ ] All rules are in /Rules
- [ ] No cross-module VO/rule duplicated here when it already exists in Shared
