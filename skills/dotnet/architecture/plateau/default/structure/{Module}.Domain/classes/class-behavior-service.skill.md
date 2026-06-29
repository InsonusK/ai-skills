---
name: class-behavior-service
description: Create static domain service extension methods for bulky or multi-step entity behavior
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour.skill]]"
---

# Goal
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement
- Provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Delegates all validation to existing domain rules from `{Module}.Domain/Rules`
- Does not own property mutation — delegates back to the entity's guarded methods or setters
- A single entity property must not have multiple uncoordinated public mutation points

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Behavior}Service.cs | OrderPricingService.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-behavior-service
//Plateau: default
//Version: 20260628
```

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

The entity must provide a guarded mutation method for the service to call:

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- [ ] WHEN applied THEN Keep entity classes small and readable without losing invariant enforcement
- [ ] WHEN component is requested THEN it provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters
- [ ] WHEN applied THEN Implemented as a static class with extension methods on the entity type
- [ ] WHEN applied THEN Stateless and side-effect free aside from invoking entity mutation methods
- [ ] WHEN applied THEN Delegates all validation to existing domain rules from {Module}.Domain/Rules
- [ ] WHEN applied THEN Does not own property mutation — delegates back to the entity's guarded methods or setters
- [ ] WHEN applied THEN A single entity property must not have multiple uncoordinated public mutation points
- [ ] WHEN naming 'Domain service extension' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]
