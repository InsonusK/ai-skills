---
description: Add Services folder and domain behavior patterns to module Domain project
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Own all entity behavior and invariant enforcement for the bounded context
- Provide a place to extract bulky entity logic without scattering mutation points
- Keep entities small and focused on single-responsibility state transitions

# Core Principals
- Entity methods are the primary gatekeepers of state change
- Domain rules encode reusable predicates; entities decide when and how to enforce them
- Static domain service extension methods hold complex or multi-step logic that does not fit naturally inside the entity
- A single property must not be mutated from many independent public entry points

# Structure

## Project Structure
```
/{ModuleName}.Domain
  /Entities
  /ValueObjects
  /Rules
  /Services
  /Events
  /Configurations
  {ModuleName}.Domain.csproj
```

## Directory and class skills
| Directory \| file | Description | Pattern skill |
| --- | --- | --- |
| /Entities | All entity types for this module | [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md\|solution-structure.solution.skill]] |
| /ValueObjects | All Value Object types for this module | [[skills/dotnet/skill-graph/developing v3/architecture/solutions/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md\|value-objects-and-rules.solution.skill]] |
| /Rules | All domain rule static classes for this module | [[skills/dotnet/skill-graph/developing v3/architecture/solutions/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md\|value-objects-and-rules.solution.skill]] |
| /Services | Static domain service extension methods for bulky entity behavior | Domain service pattern (this solution) |
| /Events | Domain events raised by this module | |
| /Configurations | One EF config class per entity | [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md\|domain-configuration.solution.skill]] |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Transport validation — belongs to module Application validators
- Infrastructure implementations — belongs to App.Infrastructure
- Pipeline behaviors — belongs to BuildingBlocks
- Command/Query handlers — belong to module Application
- Cross-module workflow orchestration — belongs in Application

# Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

# Rules

MUST:
- Every property mutation validates state through domain rules before assigning
- Entity methods throw `DomainException` when a rule returns `false`
- Static service extension methods live in `{Module}.Domain/Services`
- Static service extension methods use existing domain rules from `{Module}.Domain/Rules`
- A single entity property must not have multiple uncoordinated public mutation points

SHOULD:
- Prefer thin entity methods that delegate rule checks and then call a single setter
- Name service files after the behavior they encapsulate, e.g. `OrderPricingService.cs`

MUST NOT:
- Duplicate invariant logic across setters, entity methods, or service extensions
- Mutate entity state in a service extension without going through the entity's own guarded method or setter
- Allow public setters that bypass rule validation
- Let a service extension introduce a second independent mutation point for the same property

# Anti-patterns
- Entity has several points changing the same property with separate validation
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside entity methods instead of calling rules from `{Module}.Domain/Rules`

# Check list
- [ ] Every entity state change is validated by domain rules
- [ ] `DomainException` thrown when a rule returns `false`
- [ ] Bulky logic extracted to static extension methods in `{Module}.Domain/Services`
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions do not duplicate rule logic
- [ ] Service extensions mutate state only through entity methods or guarded setters
