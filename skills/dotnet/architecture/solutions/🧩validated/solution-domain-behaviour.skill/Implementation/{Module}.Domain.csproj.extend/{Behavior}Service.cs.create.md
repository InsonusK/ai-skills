---
description: Create static domain service extension methods for bulky or multi-step entity behavior
project_name: "{Module}.Domain"
name: "{Behavior}Service"
element_kind: class
change_kind: create
tags:
  - solution/domain-behaviour
  - element/behavior-service-cs
---

# Goals
- Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- Keep entity classes small and readable without losing invariant enforcement
- Provide a stateless, side-effect free place for complex calculations that still mutate entity state only through guarded entity methods or setters

# Core Principles
- Implemented as a `static class` with extension methods on the entity type
- Stateless and side-effect free aside from invoking entity mutation methods
- Delegates all validation to existing domain rules from `{Module}.Domain/Rules`
- Does not own property mutation — delegates back to the entity's guarded methods or setters

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Behavior}Service.cs | OrderPricingService.cs |

# Implementation changes

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

# Rule changes

## MUST
- Be a `static class`
- Live in `{Module}.Domain/Services`
- Use domain rules from `{Module}.Domain/Rules` for every validation
- Mutate entity state only through entity methods or guarded setters

## MUST NOT
- Reimplement rule logic inline
- Introduce a new uncoordinated public mutation point for an entity property
- Depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Hold instance state
- Duplicate invariant logic across setters, methods, or service extensions

## SHOULD
- Name service files after the behavior they encapsulate

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
