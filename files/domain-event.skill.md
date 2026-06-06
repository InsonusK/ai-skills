---
name: domain-event
description: rules for defining domain events and how entities raise and collect them
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - events
triggers:
  - define domain event
  - entity raises event
  - domain event collection
---
# Goal
Define how to declare a domain event and how an entity raises and collects it. An event is an immutable fact describing something that happened. The entity collects events — it never dispatches. The infrastructure pipeline handles delivery. See domain-events.solution.skill for the full system flow.

# Core Principles
- Events are immutable facts — past tense, no expectations of response
- Entity collects events in a private list — never dispatches directly
- Domain layer has no reference to MediatR or any dispatcher
- Integration event contracts (cross-module) are declared in `{Module}.Interfaces/Events`

# File Location
```
/{ModuleName}.Domain
  /Events
    TaskAssignedEvent.cs
```

## Base interface — Shared project
```csharp
// Shared/Events/IDomainEvent.cs
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

## Event definition
```csharp
// Task.Domain/Events/TaskAssignedEvent.cs
public record TaskAssignedEvent(
    int TaskId,
    int AssigneeId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

## Entity raises and collects
```csharp
public class Task
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    internal void Assign(int assigneeId)
    {
        if (assigneeId <= 0)
            throw new DomainException("Invalid assignee");
        AssigneeId = assigneeId;
        _domainEvents.Add(new TaskAssignedEvent(Id, assigneeId));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

# Rules
MUST:
- Event is `record` — immutable
- Event name is past tense
- Event implements `IDomainEvent` with `EventId` and `OccurredAt`
- Entity has private `_domainEvents` list, read-only public accessor, and `ClearDomainEvents()`
- Domain events live in `{Module}.Domain/Events`
- Integration event contracts live in `{Module}.Interfaces/Events`
MUST NOT:
- Entity reference MediatR or any dispatcher
- Event have mutable properties
- Share domain event class across module boundaries

# Checklist
- [ ] Event is `record` implementing `IDomainEvent`
- [ ] Event name is past tense
- [ ] Entity has `_domainEvents`, `DomainEvents`, and `ClearDomainEvents()`
- [ ] No MediatR reference in Domain project
- [ ] Cross-module event contract in `{Module}.Interfaces/Events`

# Unittest TestCases
- [ ] When behavior method called Then DomainEvents contains expected event
- [ ] When ClearDomainEvents called Then DomainEvents is empty
- [ ] When event created Then EventId is unique and OccurredAt is set

# Relations
- entity-behavior.skill — behavior methods are where events are raised
- entity.skill — entity structure that collects events
- domain-events.solution.skill — full system flow from collection to dispatch
- outbox.skill — how collected events are persisted and dispatched
