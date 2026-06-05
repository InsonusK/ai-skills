---
uid: 89c9a8ed-0695-48ff-a925-0896324278e8
status: draft
name: domain-service
description: rules for implementing domain services that encapsulate complex domain logic outside entities
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - domain-service
triggers:
  - domain service design
  - entity behavior extraction
  - complex domain logic
  - multi-entity coordination
aliases:
  - DomainService
  - Domain Service
  - Services
---
# Goal
Define where and how to move domain logic out of entities when that logic grows too large, spans multiple entities, or requires coordination between aggregates. 
A Domain Service is pure — it operates only on objects passed to it, never fetches data itself, and never depends on infrastructure. Without this pattern, entities accumulate unrelated behavior and become mega-classes that are hard to test and reason about.

# Core Principles
- Domain Service encapsulates logic that does not naturally belong to a single entity
- Domain Service is pure — it receives all data it needs as parameters, never fetches
- Domain Service has no infrastructure dependencies — no repositories, no DbContext, no HTTP
- Application layer is responsible for loading data; domain service is responsible for deciding
- Prefer extension method form when logic is scoped to one primary entity
- Prefer static class form when logic coordinates multiple entities or has no clear owner

# Structure / Contracts

## File location
```
/Domain
  /Services
    OrderDomainService.cs
    DriverEligibilityService.cs
```

## Extension method form
Use when logic operates primarily on one entity but is too large for the entity itself.
```CSharp
public static class OrderDomainService
{
    public static void ApplyDiscount(this Order order, Discount discount)
    {
        if (!(order, discount).CanApply())
            throw new DomainException("Discount cannot be applied to this order");

        order.SetDiscount(discount);
    }
}
```

## Static class form
Use when logic coordinates multiple entities or has no single clear owner.
```CSharp
public static class TransferDomainService
{
    public static void Transfer(Account source, Account target, Money amount)
    {
        if (!source.CanWithdraw(amount))
            throw new DomainException("Insufficient funds");

        source.Withdraw(amount);
        target.Deposit(amount);
    }
}
```

## Application layer loads, domain service decides
Domain service never fetches. Application layer resolves all required data first.
```CSharp
// Application layer (command handler)
var order = await _orderRepository.GetAsync(command.OrderId);
var discount = await _discountRepository.GetAsync(command.DiscountId);

// Domain service receives data — never fetches it
order.ApplyDiscount(discount);
```

# Rules
MUST:
- be stateless
- be pure — all inputs passed as parameters
- operate only on domain objects (entities, value objects)
- throw `DomainException` when invariant is violated
- use [[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill]] for reusable predicate logic 
SHOULD:
- be extension method when scoped to one primary entity
- be static class when coordinating multiple entities 
MUST NOT:
- depend on repositories, DbContext, or any infrastructure interface
- make async calls or perform IO
- access HttpContext, IOptions, or any application-layer service
- duplicate logic already enforced inside entity behavior

# Anti-patterns
- Domain service takes `IRepository` parameter — application layer loads, domain decides
- Domain service duplicates invariant already enforced in entity setter or method
- Large static class mixing unrelated entity behaviors — split into focused services
- Instance-based domain service with injected dependencies — use static or extension method

# Checklist
- [ ] Service is stateless (no instance fields)
- [ ] All required data received as parameters — no fetching
- [ ] No infrastructure dependencies
- [ ] Logic does not duplicate entity invariants
- [ ] Extension method used when primary entity is clear
- [ ] Static class used when coordinating multiple entities
- [ ] Unit tests pass with in-memory objects only (no mocks needed)

# Unittest TestCases
- [ ] When valid inputs provided Then domain state changes as expected
- [ ] When invariant violated Then throws DomainException
- [ ] When coordinating multiple entities Then all entities reach correct state
- [ ] Test requires no mocks — only in-memory domain objects

# Relations
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|entity-pattern.skill]] — domain service extracts behavior that would otherwise bloat the entity
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill|entity-behavior.skill]] — defines the boundary between entity behavior and domain service
- [[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill|domain-rule-pattern.skill]] — domain services compose rules for multi-value business decisions