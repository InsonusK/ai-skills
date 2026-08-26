---
description: Create static domain service extension methods for bulky or multi-step entity behavior, validating via locally-owned conditions
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
- Validates via a condition it owns locally — the service does not delegate to any shared rules abstraction
- Does not own property mutation — delegates back to the entity's guarded methods or setters

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Domain service extension | {Entity}{Behavior}Service or {Behavior}Service | OrderPricingService | {Behavior}Service.cs | OrderPricingService.cs |

# Implementation changes

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

The entity provides a guarded mutation method for the service to call — see `{EntityName}.cs.extend.md` in this solution.

# Rule changes

## MUST
- Be a `static class`
- Live in `{Module}.Domain/Services`
- Validate via a condition it owns locally, before calling into the entity's guarded mutation method
- Mutate entity state only through entity methods or guarded setters

## MUST NOT
- Introduce a new uncoordinated public mutation point for an entity property
- Depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Hold instance state

## SHOULD
- Name service files after the behavior they encapsulate

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate bulky or multi-step domain logic that does not fit naturally inside an entity method
- [ ] WHEN applied THEN Implemented as a static class with extension methods on the entity type
- [ ] WHEN applied THEN Stateless and side-effect free aside from invoking entity mutation methods
- [ ] WHEN applied THEN Does not own property mutation — delegates back to the entity's guarded methods or setters
