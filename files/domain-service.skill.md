---
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
  - extract entity behavior to service
  - multi-entity domain logic
  - domain service design
---
# Goal
Define where and how to move domain logic out of an entity when that logic spans multiple entities or grows too large for the entity class. A Domain Service is pure — it receives all data as parameters, never fetches, and has no infrastructure dependencies.

# Core Principles
- Domain Service is pure — all inputs passed as parameters, no fetching
- Application layer loads data; domain service decides what to do with it
- No infrastructure dependencies — no repositories, no DbContext, no HTTP
- Prefer extension method when logic is primarily scoped to one entity
- Prefer static class when coordinating multiple entities

# File Location
```
/{ModuleName}.Domain
  /Services
    TaskDomainService.cs
    TransferDomainService.cs
```

## Extension method form
Use when logic is too large for the entity but still primarily about one entity.
```csharp
public static class TaskDomainService
{
    public static void Complete(this Task task, DateTimeOffset completedAt)
    {
        if (task.Status == TaskStatus.Completed)
            throw new DomainException("Already completed");
        task.Status = TaskStatus.Completed;
        task.CompletedAt = completedAt;
    }
}
```

## Static class form — multiple entities
```csharp
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
```csharp
// Application handler — loads data
var task = await _repository.FirstOrDefaultAsync(new TaskByIdSpec(command.TaskId), ct);

// Domain service receives data — never fetches
task.Complete(command.CompletedAt);
```

# Rules
MUST:
- Be stateless — no instance fields
- Receive all data as parameters
- Throw `DomainException` when invariant is violated
MUST NOT:
- Take repository, DbContext, or any infrastructure interface as parameter
- Make async calls or perform IO
- Duplicate logic already enforced inside entity behavior

# Checklist
- [ ] Service is stateless
- [ ] All required data received as parameters
- [ ] No infrastructure dependencies
- [ ] Extension method used when primary entity is clear
- [ ] Static class used when coordinating multiple entities

# Unittest TestCases
- [ ] When valid inputs provided Then domain state changes as expected
- [ ] When invariant violated Then DomainException thrown
- [ ] Tests require no mocks — only in-memory domain objects

# Relations
- entity-behavior.skill — boundary between entity behavior and domain service
- domain-rule.skill — domain services compose rules for multi-value decisions
- entity.skill — entities passed as parameters to domain services
