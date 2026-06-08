---
uid: 058bf027-1369-4ff5-a8b7-ff442f260870
name: domain-service-extension
description: Static extension-method service for bulky entity behavior that does not fit naturally inside the entity itself. Keeps entities small without losing invariant enforcement.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - service
  - extension
triggers:
  - create domain service
  - extract bulky logic from entity
  - add domain service extension
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]"
aliases:
  - Domain Service Extension
---

# Goal
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement
- Provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters

# Core Principles
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Delegates all validation to existing domain rules from `{Module}.Domain/Rules`
- Does not own property mutation — delegates back to the entity's guarded methods or setters
- A single entity property must not have multiple uncoordinated public mutation points

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /Services
    {Behavior}Service.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Entity}{Behavior}Service.cs | OrderPricingService.cs |

## Implementation
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

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]

# Rules

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

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]

# Anti-patterns
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside service extension instead of calling rules from `{Module}.Domain/Rules`
- Service extension holds state or depends on infrastructure
- Same business condition checked in controller, validator, entity, and service separately

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]

# Check list
- [ ] Service is a `static class`
- [ ] Lives in `{Module}.Domain/Services`
- [ ] Uses domain rules from `{Module}.Domain/Rules` for validation
- [ ] Mutates entity state only through entity methods or guarded setters
- [ ] No duplicate rule logic inside the service
- [ ] No infrastructure dependencies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]

# Unittest TestCases
- [ ] When service extension called with invalid args Then `DomainException` thrown
- [ ] When service extension called Then state changes only through entity guarded method
- [ ] When rule returns false Then service does not mutate entity state

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.skill.md.md]]
