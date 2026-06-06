---
uid: a1b2c3d4-e5f6-4a7b-8c9d-e0f1a2b3c4d5 
status: draft 
name: domain-event-architecture 
description: architecture decision record for domain event flow — collect, persist, dispatch 
domain: skill 
type: architecture 
tags:
- dotnet
- architecture
- ddd
- events
- outbox
- mediatR
triggers:
- domain event architecture
- event system design
- outbox architecture decision
- cross-module event flow
---
# Goal
Define the system-level architecture for domain events — how events are raised, persisted, and dispatched across the application. This skill is the entry point for understanding the event system. It records the decisions made, explains why, and maps to the implementation skills for each component.

# Architecture Decision
## Decision: collect-on-entity, persist-via-outbox, dispatch-via-MediatR

### Why not dispatch immediately inside the entity
- Entity calling MediatR directly introduces an infrastructure dependency into the domain layer — violating the core rule that Domain has no infrastructure dependencies. 
- It also fires events even if the transaction rolls back, creating phantom notifications.

### Why not dispatch after SaveChanges without outbox
```
await _unitOfWork.SaveChangesAsync(); // transaction committed
await _mediator.Publish(event);       // crash here → event lost forever
```
Any failure between save and dispatch loses the event permanently. No retry is possible.

### Why outbox
Events written to `OutboxMessage` table in the **same transaction** as the state change. If the app crashes after commit, outbox rows survive. Background dispatcher retries. Delivery is guaranteed — the only question is when, not whether.

### Why MediatR as internal bus
All handlers — same module or cross-module — subscribe via `INotificationHandler<T>`. No broker dependency in the monolith. When splitting to separate repos, a broker publisher subscribes to MediatR and writes to the message broker. The domain and handlers change nothing.

# Full Event Flow
```
Entity behavior method called
    ↓
Event added to entity._domainEvents list
    ↓
Application layer calls SaveChanges
    ↓
DomainEventInterceptor (EF SaveChanges interceptor) fires
    ↓
Reads _domainEvents from all tracked entities
    ↓
Writes OutboxMessage rows — SAME TRANSACTION as state change
    ↓
Clears _domainEvents on all entities
    ↓
Transaction commits (state + outbox messages atomic)
    ↓
OutboxDispatcher (BackgroundService) polls OutboxMessage table
    ↓
Deserializes event, publishes to MediatR
    ↓
INotificationHandler<TEvent> executes in subscribing module
    ↓
OutboxMessage.ProcessedAt set — marks as delivered
```

# Event Types

|Type|Scope|Defined in|Dispatched via|
|---|---|---|---|
|Domain Event|Internal to module|`{Module}.Domain/Events`|MediatR via outbox|
|Integration Event|Cross-module|`{Module}.Interfaces/Events`|MediatR via outbox|

Both types travel the same pipeline. Scope and placement differ — transport does not.

# Component Map

| Component                                | Lives in                   | Skill                                  |
| ---------------------------------------- | -------------------------- | -------------------------------------- |
| `IDomainEvent` base interface            | `Shared`                   | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]]         |
| Event definition (`record`)              | `{Module}.Domain/Events`   | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]]         |
| Entity `_domainEvents` collection        | `{Module}.Domain/Entities` | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]]         |
| `OutboxMessage` class                    | `BuildingBlocks`           | [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/outbox-pattern.skill]]               |
| `DomainEventInterceptor`                 | `App.Infrastructure`       | [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/outbox-pattern.skill]]               |
| `OutboxDispatcher` background service    | `App.Infrastructure`       | [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/outbox-pattern.skill]]               |
| `INotificationHandler<T>` implementation | `{Module}.Application`     | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-handler-pattern.skill]] |

# Future: splitting to separate repos
When a module moves to its own repo, no domain or handler code changes. Add one component in the sending repo:
```
MediatR handler (App.Infrastructure)
    → subscribes to integration event
    → writes to message broker (same outbox, different publisher)
```

Add one component in the receiving repo:
```
Message broker consumer (App.Infrastructure)
    → receives message
    → publishes to local MediatR
    → existing INotificationHandler<T> executes unchanged
```

# Core Principles
- Domain layer never references MediatR or any dispatcher
- Events are facts — immutable, past tense, no expectations of response
- Outbox guarantees delivery — handlers must guarantee idempotency
- MediatR is the internal bus — broker is an optional future transport layer
- Same event class used for both domain and integration scope — placement determines scope

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]] — how to define events and raise them on entities
- [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/outbox-pattern.skill]] — OutboxMessage, interceptor, background dispatcher
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-handler-pattern.skill]] — how to write handlers, idempotency rules
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — where each component lives in the solution
- [[skills/dotnet/skill-graph/developing/Architecture/cross-module-interaction.skill]] — integration events as cross-module communication