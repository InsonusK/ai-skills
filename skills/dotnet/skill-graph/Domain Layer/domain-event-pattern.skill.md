---
uid:
status: draft
name: domain-event-pattern
description: rules for defining, raising, and dispatching domain events in the domain layer
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - events
  - outbox
  - mediatR
triggers:
  - domain event design
  - entity state change notification
  - cross-module event communication
  - outbox pattern
aliases:
  - DomainEvent
  - Domain Event
  - Events
---
# Goal
Define how to declare a domain event and how an entity raises and collects it. An event is an immutable fact describing something that happened in the domain. The entity never dispatches — it collects. The infrastructure pipeline handles delivery. See [[skills/dotnet/skill-graph/architecture/solution/domain-event-architecture.skill]] for the full system flow.

# Core Principles
- Events are immutable facts — past tense, no expectations of response
- Entity collects events in a private list — never dispatches directly
- Domain layer has no reference to MediatR or any dispatcher
- Domain event defined in `{Module}.Domain/Events`
- Integration event contract declared in `{Module}.Interfaces/Events`

# Structure / Contracts

## Base interface — Shared

```csharp
// Shared/Events/IDomainEvent.cs
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

## Event definition — [[skills/dotnet/skill-graph/Domain Layer/domain-csproj|{Module}.Domain]]/Events

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

## Entity raises and collects events

```csharp
public class TodoTask
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public int AssigneeId { get; internal set; }

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
- Event declared as `record` — immutable by design
- Event implements `IDomainEvent` with `EventId` and `OccurredAt`
- Entity has private `_domainEvents` list and public `ClearDomainEvents()`
- Domain event placed in `{Module}.Domain/Events`
- Integration event contract placed in `{Module}.Interfaces/Events` 
MUST NOT:
- Entity reference MediatR, IMediator, or any dispatcher
- Entity dispatch events directly — only collect
- Domain layer depend on outbox or infrastructure

# Anti-patterns
- Entity calls `_mediator.Publish()` directly — infrastructure dependency in domain
- Event defined as class with mutable properties — events are facts, must be immutable
- Sharing domain event class across module boundaries — declare integration contract in Interfaces instead
- Event with imperative name (`AssignTask`) — events are facts, use past tense (`TaskAssigned`)

# Checklist
- [ ] `IDomainEvent` lives in `Shared` and extends `INotification`
- [ ] Event is `record` implementing `IDomainEvent`
- [ ] Event name is past tense
- [ ] Entity has `_domainEvents` list, read-only public accessor, and `ClearDomainEvents()`
- [ ] Domain event in `{Module}.Domain/Events`
- [ ] Integration event contract in `{Module}.Interfaces/Events`
- [ ] No MediatR or dispatcher reference in Domain project

# Unittest TestCases
- [ ] When entity behavior method called Then `DomainEvents` contains expected event
- [ ] When `ClearDomainEvents` called Then `DomainEvents` is empty
- [ ] When event created Then `EventId` is unique and `OccurredAt` is set

# Relations
- [[skills/dotnet/skill-graph/architecture/solution/domain-event-architecture.skill]] — system-level flow and architecture decisions
- [[skills/dotnet/skill-graph/Infrastructure Layer/outbox-pattern.skill]] — how collected events are persisted and dispatched
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill]] — behavior methods are where events are raised
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill]] — entities that raise events follow standard entity rules