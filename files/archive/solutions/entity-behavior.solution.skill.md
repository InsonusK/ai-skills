---
uid:
name: entity-behavior
description: defines how domain behavior is implemented on entities and when logic is extracted to domain services
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - entity
  - behavior
triggers:
  - implement entity behavior
  - extract domain logic
  - entity method validation
  - domain service extraction
---
# Goal
Define how domain behavior is implemented on entities and when it should be extracted to a Domain Service. Every state-changing method must validate before changing. When behavior spans multiple entities or grows too large for one class, it moves to a Domain Service. Without this solution, invariant enforcement scatters and entities become invalid from any caller.

# Core Principles
- Entity owns its own invariants — no external code can put it in invalid state
- Every mutation validates via domain rules before changing state
- Behavior method is the single point of change for a property — never multiple paths
- Domain Service extracts behavior when it spans multiple entities or becomes too large
- Application layer loads data — domain layer decides what to do with it

# Depend on
- rule-usage.solution.skill.md — entity methods use rules for validation

# Flow
```
Application handler loads entity via repository
    ↓
Handler calls entity behavior method
    ↓
Method calls domain rule → throws DomainException if violated
    ↓
Method changes state
    ↓
Method adds domain event to _domainEvents if state change is significant
    ↓
Handler returns — UnitOfWorkBehavior commits
```

# Implementation

## {Entity}.cs — `{Module}.Domain/Entities`

### Simple behavior method
```csharp
public void Assign(int assigneeId)
{
    if (!assigneeId.IsValidId())
        throw new DomainException("Invalid assignee");

    AssigneeId = assigneeId;
    _domainEvents.Add(new TaskAssignedEvent(Id, assigneeId));
}
```

### When to extract to Domain Service
Extract when:
- behavior involves more than one entity
- behavior logic is too large to read comfortably in the entity class
- behavior has no single clear entity owner

## {Domain}Service.cs — `{Module}.Domain/Services`

### Extension method form — one primary entity
```csharp
public static class TaskDomainService
{
    public static void Complete(this Task task, DateTimeOffset completedAt)
    {
        if (!task.CanBeCompleted())
            throw new DomainException("Task cannot be completed");

        task.Status = TaskStatus.Completed;
        task.CompletedAt = completedAt;
    }
}
```

### Static class form — multiple entities
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

### Application handler loads, domain service decides
```csharp
// Handler loads all required data
var task = await _repository.FirstOrDefaultAsync(new TaskByIdSpec(id), ct);
var subtasks = await _repository.ListAsync(new SubtasksByTaskIdSpec(id), ct);

// Domain service receives data — never fetches
task.Complete(subtasks, command.CompletedAt);
```

# Example
```csharp
// Entity — single entity behavior
public class Task
{
    public void Assign(int assigneeId)
    {
        if (!assigneeId.IsValidId())
            throw new DomainException("Invalid assignee");
        AssigneeId = assigneeId;
    }
}

// Domain Service — multi-entity behavior
public static class TaskCompletionService
{
    public static void Complete(Task task, IReadOnlyList<Task> subtasks)
    {
        if (subtasks.Any(s => s.Status != TaskStatus.Completed))
            throw new DomainException("All subtasks must be completed first");
        task.Status = TaskStatus.Completed;
    }
}

// Handler — loads, delegates, never decides
var task = await _repository.FirstOrDefaultAsync(new TaskByIdSpec(id), ct);
var subtasks = await _repository.ListAsync(new SubtasksByTaskIdSpec(id), ct);
TaskCompletionService.Complete(task, subtasks);
```

# Rules
MUST:
- Every state-changing method validates via domain rules before mutating
- Throw `DomainException` when rule violated — never silently ignore
- One single method per state change — no multiple mutation paths for same property
- Domain Service is pure — all data passed as parameters, no repository calls
MUST NOT:
- Domain Service inject or call repositories, DbContext, or any infrastructure
- Application handler contain domain decisions — delegate to entity or service
- Same property be mutatable from multiple uncoordinated methods

# Anti-patterns
- Handler decides: `if (task.Status == TaskStatus.Active) task.Status = TaskStatus.Completed` — belongs in entity method
- Domain Service takes `IRepository` as parameter — application loads, domain decides
- Two entity methods both setting the same property with different validation — consolidate

# Checklist
- [ ] Every mutation method validates before changing state
- [ ] `DomainException` thrown on rule violation
- [ ] No multiple uncoordinated mutation paths for same property
- [ ] Complex multi-entity behavior extracted to Domain Service
- [ ] Domain Service has no infrastructure dependencies
- [ ] Application handler loads data, domain method/service decides

# Unittest TestCases
- [ ] When valid input Then state changes correctly
- [ ] When invalid input Then DomainException thrown
- [ ] When multi-entity condition not met Then DomainException thrown
- [ ] Domain Service tests require no mocks — only in-memory domain objects

# Relations
- rule-usage.solution.skill.md — rules used inside behavior methods
- domain-events.solution.skill.md — behavior methods raise domain events
- entity.class.skill.md — entity structure
- domain-service.class.skill.md — domain service structure
