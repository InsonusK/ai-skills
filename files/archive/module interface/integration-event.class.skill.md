---
name: integration-event
description: defines how to declare an integration event contract in the Interfaces project
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - events
  - integration-event
  - cross-module
triggers:
  - declare integration event
  - cross module event
  - integration event contract
---
# Goal
Define how to declare an integration event in `{Module}.Interfaces`. An integration event is a cross-module fact — it announces that something happened in this module so other modules can react. Declaring it in Interfaces means consuming modules reference only Interfaces, never Domain.

# Core Principles
- Integration event is a fact — past tense, immutable, no expectation of response
- Declared in Interfaces so consuming modules can subscribe without Domain dependency
- `record` implementing `IDomainEvent` — travels via outbox same as domain events
- `EventId` ensures idempotency in consuming handlers
- One event per significant state change — not one event for all changes

# Governed by
- domain-events.solution.skill.md — event travels via outbox, handler must be idempotent
- cross-module-communication.solution.skill.md — consuming module subscribes via INotificationHandler

# Structure
## Place in csproj
Defined in `module-interfaces.csproj.skill.md`
```
/{ModuleName}.Interfaces
  /Events
    TaskAssignedIntegrationEvent.cs
```

## Naming convention
```
class name:
  rule: entity name + past tense action + IntegrationEvent suffix
  pattern: {Entity}{PastTenseAction}IntegrationEvent
  example: TaskAssignedIntegrationEvent, OrderCreatedIntegrationEvent

file name:
  rule: matches class name exactly
  pattern: {Entity}{PastTenseAction}IntegrationEvent.cs
  example: TaskAssignedIntegrationEvent.cs
```

# Contracts

```csharp
// Task.Interfaces/Events/TaskAssignedIntegrationEvent.cs
public record TaskAssignedIntegrationEvent(
    int TaskId,
    int AssigneeId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

## Raised in entity behavior method
```csharp
// Task.Domain/Entities/Task.cs
public void Assign(int assigneeId)
{
    AssigneeId = assigneeId;
    _domainEvents.Add(new TaskAssignedIntegrationEvent(Id, assigneeId));
}
```

## Consumed in another module
```csharp
// TimeLog.Application/EventHandlers/TaskAssignedEventHandler.cs
public class TaskAssignedEventHandler
    : INotificationHandler<TaskAssignedIntegrationEvent>
{
    public async Task Handle(
        TaskAssignedIntegrationEvent notification, CancellationToken ct)
    {
        var exists = await _repository.AnyAsync(
            new TimeEntryByEventIdSpec(notification.EventId), ct);
        if (exists) return;

        await _repository.AddAsync(
            new TimeEntry(notification.TaskId, notification.AssigneeId), ct);
    }
}
```

# Rules
MUST:
- Be a `record` type
- Implement `IDomainEvent` with `EventId` and `OccurredAt`
- Name is past tense
- Contain only primitive types — no domain entity references
- `EventId` initialized to `Guid.NewGuid()` — unique per event instance
MUST NOT:
- Contain business logic
- Reference domain entity types
- Have mutable properties

# Anti-patterns
- Event name is imperative: `AssignTaskEvent` — must be past tense: `TaskAssignedIntegrationEvent`
- Event declared in Domain instead of Interfaces — consuming module cannot reference it without Domain dependency
- Event missing `EventId` — consuming handler cannot implement idempotency guard
- Event exposes domain entity: `public Task Task { get; }` — project to flat primitives

# Checklist
- [ ] `record` type
- [ ] Implements `IDomainEvent`
- [ ] Past tense name with `IntegrationEvent` suffix
- [ ] `EventId = Guid.NewGuid()` initialized
- [ ] Only primitive types — no domain entity references
- [ ] Declared in `/Events` folder of Interfaces project

# Relations
- module-interfaces.csproj.skill.md — project this event lives in
- domain-events.solution.skill.md — outbox delivers this event to MediatR
- cross-module-communication.solution.skill.md — consuming module subscribes to this event
- event-handler.class.skill.md — idempotent handler in consuming module
