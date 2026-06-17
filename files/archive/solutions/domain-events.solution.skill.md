---
uid:
name: domain-events
description: defines the full domain event flow — entity collection, outbox persistence, background dispatch, and idempotent handler execution
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - events
  - outbox
triggers:
  - domain event flow
  - entity raises event
  - outbox pattern
  - event handler idempotency
---
# Goal
Define how domain events flow from entity behavior methods through the outbox to MediatR handlers. Guarantees that no event is lost even if the application crashes after a transaction commits — the outbox row survives and the background dispatcher retries. Without this solution, events are either lost on crash or fire before the transaction commits.

# Core Principles
- Entity collects events — never dispatches directly
- Outbox row written in the same transaction as the state change — atomicity guaranteed
- Background dispatcher is the only component that publishes to MediatR
- Handlers must be idempotent — outbox guarantees delivery, not exactly-once
- Domain layer has zero MediatR or infrastructure dependencies

# Depend on
- entity-behavior.solution.skill.md — behavior methods are where events are raised

# Flow
```
Entity behavior method called
    ↓
Event added to entity._domainEvents list
    ↓
UnitOfWorkBehavior calls SaveChanges
    ↓
DomainEventInterceptor fires inside SaveChanges
    ↓
Reads _domainEvents from all EF tracked entities
Writes OutboxMessage rows — SAME transaction as state change
Clears _domainEvents on all entities
    ↓
Transaction commits — state + outbox messages atomic
    ↓
OutboxDispatcher (BackgroundService) polls OutboxMessage table
    ↓
Deserializes event → publishes to MediatR
    ↓
INotificationHandler<TEvent> executes in subscribing module
    ↓
OutboxMessage.ProcessedAt set — marks as delivered
```

# Implementation

## IDomainEvent.cs — `Shared`
```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

## {EventName}.cs — `{Module}.Domain/Events`
```csharp
public record TaskAssignedEvent(int TaskId, int AssigneeId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

## {Entity}.cs — `{Module}.Domain/Entities`
Entity collects events — never dispatches.
```csharp
public class Task
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();

    public void Assign(int assigneeId)
    {
        AssigneeId = assigneeId;
        _domainEvents.Add(new TaskAssignedEvent(Id, assigneeId));
    }
}
```

## OutboxMessage.cs — `BuildingBlocks`
```csharp
public class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; }      // AssemblyQualifiedName
    public string Payload { get; init; }   // JSON
    public DateTime OccurredAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}
```

## DomainEventInterceptor.cs — `App.Infrastructure`
Writes outbox rows inside the same SaveChanges transaction.
```csharp
public class DomainEventInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct)
    {
        var entities = eventData.Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity).ToList();

        var messages = entities
            .SelectMany(e => e.DomainEvents)
            .Select(e => new OutboxMessage
            {
                Id = e.EventId,
                Type = e.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(e, e.GetType()),
                OccurredAt = e.OccurredAt
            }).ToList();

        await eventData.Context.Set<OutboxMessage>().AddRangeAsync(messages, ct);
        entities.ForEach(e => e.ClearDomainEvents());

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
```

## {EventName}Handler.cs — `{Module}.Application/EventHandlers`
Handler must be idempotent — check before act using EventId.
```csharp
public class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly IRepository<TimeEntry> _repository;

    public async Task Handle(TaskAssignedEvent notification, CancellationToken ct)
    {
        var exists = await _repository.AnyAsync(
            new TimeEntryByEventIdSpec(notification.EventId), ct);

        if (exists) return;  // already processed — return early, never throw

        var entry = new TimeEntry(notification.TaskId, notification.AssigneeId);
        await _repository.AddAsync(entry, ct);
    }
}
```

# Example
```
task.Assign(assigneeId)               → TaskAssignedEvent in _domainEvents
SaveChanges()                         → OutboxMessage written, _domainEvents cleared
Transaction commits                   → state + outbox atomic

OutboxDispatcher polls                → finds unprocessed OutboxMessage
Publishes TaskAssignedEvent           → TaskAssignedEventHandler.Handle()
Handler idempotency check             → not yet processed, continue
TimeEntry created                     → side effect applied
OutboxMessage.ProcessedAt set         → marked delivered

On retry (crash before ProcessedAt):
OutboxDispatcher polls again          → finds same OutboxMessage (ProcessedAt null)
Publishes again                       → TaskAssignedEventHandler.Handle()
Handler idempotency check             → already processed, return early
No duplicate TimeEntry created
```

# Rules
MUST:
- Entity has `_domainEvents` list, `DomainEvents` read-only accessor, `ClearDomainEvents()`
- Entity implements `IHasDomainEvents`
- Event is `record` implementing `IDomainEvent` — immutable, past tense name
- `DomainEventInterceptor` writes outbox rows in same transaction as state change
- `ProcessedAt` set only after successful MediatR publish
- Every event handler has explicit idempotency guard using `EventId`
- Handler returns early on duplicate — never throws
MUST NOT:
- Entity reference MediatR or call `_mediator.Publish()` directly
- Dispatch events after SaveChanges without outbox — crash loses events
- Set `ProcessedAt` before publish — failed handlers silently skipped
- Handler throw on duplicate event — blocks outbox retry

# Anti-patterns
- Entity calls `_mediator.Publish()` — infrastructure dependency in domain, event fires before transaction commits
- Dispatching events in handler after SaveChanges without outbox — crash between save and dispatch loses event permanently
- Handler without idempotency guard — duplicate TimeEntries on retry
- `Type` stored as `FullName` instead of `AssemblyQualifiedName` — breaks deserialization after namespace change

# Checklist
- [ ] Entity has `_domainEvents`, `DomainEvents`, `ClearDomainEvents()`
- [ ] Entity implements `IHasDomainEvents`
- [ ] Event is `record`, past tense name, implements `IDomainEvent`
- [ ] `DomainEventInterceptor` registered on DbContext
- [ ] `OutboxMessage.Type` uses `AssemblyQualifiedName`
- [ ] `ProcessedAt` set after successful publish only
- [ ] Every event handler has idempotency guard
- [ ] Duplicate event returns early — does not throw

# Unittest TestCases
- [ ] When behavior method called Then DomainEvents contains expected event
- [ ] When SaveChanges called Then OutboxMessage row created in same transaction
- [ ] When SaveChanges called Then _domainEvents cleared
- [ ] When OutboxDispatcher runs Then MediatR Publish called for unprocessed messages
- [ ] When handler processes event first time Then side effect applied
- [ ] When handler processes same event twice Then side effect applied only once
- [ ] When crash before ProcessedAt set Then same event redelivered and handled idempotently

# Relations
- entity-behavior.solution.skill.md — behavior methods raise events
- command-handling.solution.skill.md — SaveChanges triggered by UnitOfWorkBehavior
- entity.class.skill.md — _domainEvents collection on entity
- event-handler.class.skill.md — idempotent handler implementation
