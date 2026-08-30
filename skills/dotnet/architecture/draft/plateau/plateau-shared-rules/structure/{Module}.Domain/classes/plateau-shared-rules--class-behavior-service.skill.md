---
name: plateau-shared-rules--class-behavior-service
description: Class {Behavior}Service in the shared-rules plateau
whenToUse: when entity behavior logic grows too large or multi-step to stay readable inside the Entity itself
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
---

# Goal
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Core Principles
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Validates via a condition it owns locally — does not delegate to any shared rules abstraction
- Does not own property mutation — delegates back to the entity's guarded methods or setters

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Behavior}Service.cs | OrderPricingService.cs |

# Implementation
```csharp
//Skill: class-behavior-service
//Plateau: shared-rules
//Version: 20260824163000

public static class OrderPricingService
{
    public static void RecalculateTotal(this Order order, IEnumerable<LineItem> items)
    {
        if (!items.Any())
            throw new DomainException("{ModuleName}.Order.MustHaveLineItem", "Order must contain at least one line item.");

        var total = new Money(items.Sum(i => i.Quantity * i.UnitPrice), "USD");

        order.SetTotal(total); // Order.SetTotal is the entity's own guarded internal setter
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Rules
MUST:
- Be a `static class`, live in `{Module}.Domain/Services`
- Validate via a condition it owns locally, before calling into the entity's guarded mutation method
- Mutate entity state only through entity methods or guarded setters
MUST NOT:
- Introduce a new uncoordinated public mutation point for an entity property
- Depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Hold instance state

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Check list
- [ ] Is a `static class` living in `{Module}.Domain/Services`
- [ ] Stateless, validates locally before calling an entity's guarded method
- [ ] Never writes to an entity property directly

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]
