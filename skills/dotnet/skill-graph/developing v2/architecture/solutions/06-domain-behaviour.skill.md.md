---
uid: 77ba543e-2cde-42b1-a8e5-ff91f32e5bb1
order: 6
name: domain-behaviour
description: Defines how domain entities change state, enforce invariants through domain rules, and extract bulky logic into static domain service extension methods
domain: skill
type: architecture
version: 20260610
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
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/DomainService.class.skill|DomainService.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill|04-value-object.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill|05-domain-rule.solution.skill]]"
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
MUST:
- Every entity state change is guarded by domain rules
- All invariant enforcement logic uses existing domain rules from `{Module}.Domain/Rules`
- Bulky or multi-step logic can be moved to static extension methods in `{Module}.Domain/Services`
- State mutation still happens through the entity; services only delegate back to entity methods or controlled setters
- `DomainException` is thrown when a rule returns `false`

# Implementation

## App Repository (.sln)

### Structure

#### Project Structure
No solution-level structure changes.

#### Directory and class skills
No new solution-level directories or files.

### Rules
MUST:
- Every entity property mutation validates state before assigning
- Public or `internal` mutation points delegate to domain rules

MUST NOT:
- Allow entities to be put into an invalid state from application or infrastructure layers

---

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goals
- Own all entity behavior and invariant enforcement for the bounded context
- Provide a place to extract bulky entity logic without scattering mutation points
- Keep entities small and focused on single-responsibility state transitions

#### Core Principals
- Entity methods are the primary gatekeepers of state change
- Domain rules encode reusable predicates; entities decide when and how to enforce them
- Static domain service extension methods hold complex or multi-step logic that does not fit naturally inside the entity
- A single property must not be mutated from many independent public entry points

#### Structure

##### Project Structure
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

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Entities | All entity types for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill\|Entity.class.skill]] |
| /ValueObjects | All Value Object types for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/ValueObject.class.skill\|ValueObject.class.skill]] |
| /Rules | All domain rule static classes for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/PrimitiveRule.class.skill\|PrimitiveRule.class.skill]], [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ValueObjectRule.class.skill\|ValueObjectRule.class.skill]], [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ContextualRule.class.skill\|ContextualRule.class.skill]] |
| /Services | Static domain service extension methods for bulky entity behavior | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/DomainService.class.skill\|DomainService.class.skill]] |
| /Events | Domain events raised by this module | |
| /Configurations | One EF config class per entity | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/EntityConfiguration.class.skill\|EntityConfiguration.class.skill]] |

#### What Does NOT Belong Here
- Transport validation — belongs to module Application validators
- Infrastructure implementations — belongs to App.Infrastructure
- Pipeline behaviors — belongs to BuildingBlocks
- Command/Query handlers — belong to module Application
- Cross-module workflow orchestration — belongs in Application

#### Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

#### Rules
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

#### Anti-patterns
- Entity has several points changing the same property with separate validation
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside entity methods instead of calling rules from `{Module}.Domain/Rules`

#### Check list
- [ ] Every entity state change is validated by domain rules
- [ ] `DomainException` thrown when a rule returns `false`
- [ ] Bulky logic extracted to static extension methods in `{Module}.Domain/Services`
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions do not duplicate rule logic
- [ ] Service extensions mutate state only through entity methods or guarded setters

### Class extension

#### Entity (extended)

##### Goals
- Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- Keep entity validation logic DRY by delegating to reusable domain rules
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity as the gatekeeper of state

##### Core Principals
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods call domain rules to validate state transitions before applying changes
- Rule returns `bool` — entity decides whether to throw `DomainException`
- Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation

##### Naming convention
| use case                 | class name pattern                             | class name          | file name pattern            | file name              |
| ------------------------ | ---------------------------------------------- | ------------------- | ---------------------------- | ---------------------- |
| Entity behavior method   | {Verb}{Noun} or {Verb}                         | UpdateComment       | {EntityName}.cs              | Order.cs               |

##### Implementation changes
Entity behavior methods must validate state through domain rules before mutating:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (!comment.IsNotEmpty())
            throw new DomainException("Comment must not be empty.");

        if (!comment.IsMaxLength(500))
            throw new DomainException("Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity can compose contextual rules for complex invariants:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!(Age, Country).IsSatisfied())
            throw new DomainException("Driver does not meet licensing requirements for this country.");

        // ... assign license
    }
}
```

When behavior becomes too large for the entity, delegate to a Domain Service Extension defined in `DomainService (created)` below.

##### Rule changes
MUST:
- Call domain rules inside entity methods before mutating state
- Throw `DomainException` when a rule returns `false`
- Use the most specific rule available (primitive, VO, or contextual)
- Keep the entity as the single gatekeeper for each property mutation
MUST NOT:
- Reimplement rule logic inline inside entity methods
- Mutate state before validating with rules
- Allow invalid state to persist silently
- Let a service extension expose a second public way to change a property that is already changed by an entity method

#### DomainService (created)

##### Goals
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement
- Provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters

##### Core Principals
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Delegates all validation to existing domain rules from `{Module}.Domain/Rules`
- Does not own property mutation — delegates back to the entity's guarded methods or setters
- A single entity property must not have multiple uncoordinated public mutation points

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Entity}{Behavior}Service.cs | OrderPricingService.cs |

##### Implementation changes
Static service extension must use domain rules and delegate mutation to the entity:

```csharp
// {Module}.Domain/Services/OrderPricingService.cs
public static class OrderPricingService
{
    public static void RecalculateTotal(this Order order, IEnumerable<LineItem> items)
    {
        if (!items.Any())
            throw new DomainException("Order must contain at least one line item.");

        var total = items.Sum(i => i.Quantity * i.UnitPrice);

        if (!total.IsPositive())
            throw new DomainException("Order total must be positive.");

        order.SetTotal(total);
    }
}
```

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (!total.IsPositive())
            throw new DomainException("Total must be positive.");

        Total = total;
    }
}
```

##### Rule changes
MUST:
- Be a `static class`
- Live in `{Module}.Domain/Services`
- Use domain rules from `{Module}.Domain/Rules` for every validation
- Mutate entity state only through entity methods or guarded setters
MUST NOT:
- Reimplement rule logic inline
- Introduce a new uncoordinated public mutation point for an entity property
- Depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Hold instance state

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
