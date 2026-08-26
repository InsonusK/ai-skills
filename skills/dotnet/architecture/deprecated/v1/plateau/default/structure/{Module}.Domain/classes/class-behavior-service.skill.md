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
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]"
---

# Goal
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement
- Provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]

# Core Principles
- Apply ONE plateau template per class
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Validates via a condition it owns locally — the service does not delegate to any shared rules abstraction
- Does not own property mutation — delegates back to the entity's guarded methods or setters
- A single entity property must not have multiple uncoordinated public mutation points

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Behavior}Service.cs | OrderPricingService.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-behavior-service
//Plateau: default
//Version: 20260628
```

Static service extension validates locally and delegates mutation to the entity:

```csharp
// {Module}.Domain/Services/OrderPricingService.cs
public static class OrderPricingService
{
    public static void RecalculateTotal(this Order order, IEnumerable<LineItem> items)
    {
        if (!items.Any())
            throw new DomainException("{ModuleName}.Order.MustHaveLineItem", "Order must contain at least one line item.");

        var total = items.Sum(i => i.Quantity * i.UnitPrice);

        order.SetTotal(total); // Order.SetTotal validates total >= 0 itself
    }
}
```

The entity provides a guarded mutation method for the service to call — see `class-entity.skill.md`:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (total < 0)
            throw new DomainException("{ModuleName}.Order.TotalMustBePositive", "Total must be positive.");

        Total = total;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]

# Rules
MUST:
	- Be a `static class`
	- Live in `{Module}.Domain/Services`
	- Validate via a condition it owns locally, before calling into the entity's guarded mutation method
	- Mutate entity state only through entity methods or guarded setters
SHOULD:
	- Name service files after the behavior they encapsulate, e.g. `OrderPricingService.cs`
MUST NOT:
	- Introduce a new uncoordinated public mutation point for an entity property
	- Depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
	- Hold instance state

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- [ ] WHEN applied THEN Keep entity classes small and readable without losing invariant enforcement
- [ ] WHEN component is requested THEN it provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters
- [ ] WHEN applied THEN Implemented as a static class with extension methods on the entity type
- [ ] WHEN applied THEN Stateless and side-effect free aside from invoking entity mutation methods
- [ ] WHEN applied THEN Does not own property mutation — delegates back to the entity's guarded methods or setters
- [ ] WHEN applied THEN A single entity property must not have multiple uncoordinated public mutation points
- [ ] WHEN naming 'Domain service extension' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{Behavior}Service.cs.create|{Behavior}Service.cs]]
