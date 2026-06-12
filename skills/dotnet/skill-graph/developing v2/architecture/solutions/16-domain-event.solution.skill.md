---
uid: 3e7b1f9a-5c2d-4a8e-b6f3-d1c9e4f7a2b5
order: 16
name: domain-event
description: Defines the full domain event stack — IDomainEvent and IHasDomainEvents in Shared/BuildingBlocks, event record and entity collection in Domain, OutboxMessage and DomainEventInterceptor in BuildingBlocks/App.Infrastructure, OutboxDispatcher background service, integration event contract in Interfaces, and idempotent INotificationHandler in Application
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - ddd
  - events
  - outbox
  - mediatr
  - idempotency
triggers:
  - domain event design
  - raise domain event
  - entity state change notification
  - outbox pattern
  - cross-module event communication
  - integration event
  - event handler
  - idempotent handler
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/classes/IDomainEvent.class.skill|IDomainEvent.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IHasDomainEvents.class.skill|IHasDomainEvents.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/OutboxMessage.class.skill|OutboxMessage.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/DomainEventInterceptor.class.skill|DomainEventInterceptor.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/OutboxDispatcher.class.skill|OutboxDispatcher.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/OutboxMessageConfig.class.skill|OutboxMessageConfig.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/DomainEvent.class.skill|DomainEvent.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/IntegrationEvent.class.skill|IntegrationEvent.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/EventHandler.class.skill|EventHandler.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/IdempotencySpec.class.skill|IdempotencySpec.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/Shared.csproj.skill|Shared.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/{Module}.Interfaces.csproj.skill|{Module}.Interfaces.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/ModuleApplicationRegistration.class.skill|ModuleApplicationRegistration.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill|03-domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.solution.skill|06-domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/07-ardalis-specification.solution.skill|07-ardalis-specification.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/11-unit-of-work.solution.skill|11-unit-of-work.solution.skill]]"
---

# Goal
- Define `IDomainEvent` in Shared as the base contract for all domain and integration events
- Define `IHasDomainEvents` in BuildingBlocks so the outbox interceptor can work across all entity types without listing them
- Define the event `record` shape in `{Module}.Domain/Events` and the integration event contract in `{Module}.Interfaces/Events`
- Define the entity event collection pattern — `_domainEvents`, `DomainEvents`, `ClearDomainEvents()`
- Define `OutboxMessage` in BuildingBlocks and `DomainEventInterceptor` in App.Infrastructure — writing events to the outbox in the same EF transaction as the state change
- Define `OutboxDispatcher` as the background service that polls the outbox and publishes to MediatR
- Define `INotificationHandler<TEvent>` event handlers in `{Module}.Application/EventHandlers` with mandatory idempotency

# Architecture Decision Summary

## Why not dispatch events directly in the entity
Entity calling MediatR introduces an infrastructure dependency into Domain — violating the rule that Domain has no infrastructure dependencies. It also fires events even if the transaction rolls back, creating phantom notifications.

## Why not dispatch after SaveChanges without outbox
```
await _unitOfWork.SaveChangesAsync(); // transaction committed
await _mediator.Publish(event);       // crash here → event lost forever
```
Any failure between save and publish loses the event permanently with no retry possible.

## Why outbox
Events written to `OutboxMessage` table in the **same EF transaction** as the state change. If the app crashes after commit, outbox rows survive. `OutboxDispatcher` retries on restart. Delivery is guaranteed — the only question is when, not whether.

## Why idempotent handlers
The outbox guarantees at-least-once delivery — not exactly-once. A handler may receive the same event more than once (app crash after publish, before `ProcessedAt` is set). Every handler must produce the same result whether it processes the event once or ten times.

# Full event flow
```
Entity behavior method called (solution 06)
    ↓
Event added to entity._domainEvents list
    ↓
UnitOfWorkBehavior calls SaveChangesAsync (solution 11)
    ↓
DomainEventInterceptor fires (EF SavingChangesAsync interceptor)
    ↓
Reads _domainEvents from all tracked IHasDomainEvents entities
    ↓
Serializes events → writes OutboxMessage rows in SAME TRANSACTION
    ↓
Calls ClearDomainEvents() on all entities
    ↓
EF completes SaveChangesAsync — state change + outbox rows committed atomically
    ↓
OutboxDispatcher (BackgroundService) polls unprocessed OutboxMessage rows
    ↓
Deserializes event type and payload
    ↓
_mediator.Publish(domainEvent) — dispatches to all INotificationHandler<T> subscribers
    ↓
Handler executes idempotency guard — returns early if already processed
    ↓
Handler applies side effect — repositories stage changes
    ↓
Handler calls _unitOfWork.SaveChangesAsync() directly — event handlers own their own commit
    ↓
OutboxMessage.ProcessedAt = DateTime.UtcNow — marks as delivered
    ↓
OutboxDispatcher saves ProcessedAt
```

> **Note on handler SaveChangesAsync:** Event handlers call `_unitOfWork.SaveChangesAsync()` directly — they are NOT inside the MediatR pipeline. They are called by `OutboxDispatcher` via `_mediator.Publish()`, which bypasses the `UnitOfWorkBehavior` pipeline. Handlers must commit their own changes explicitly.

# Core Principles
- Domain layer never references MediatR or any dispatcher — entity only collects, never dispatches
- Events are immutable facts — past tense names, no expectations of response
- Outbox guarantees delivery — handlers must guarantee idempotency
- `IHasDomainEvents` in BuildingBlocks — interceptor works across all entity types generically
- Domain event defined in `{Module}.Domain/Events` — internal to module
- Integration event contract defined in `{Module}.Interfaces/Events` — shared across modules
- Both event types travel the same pipeline — scope and placement differ, transport does not
- Event handlers call `_unitOfWork.SaveChangesAsync()` directly — they are outside the command pipeline
- `ProcessedAt` set only after successful `_mediator.Publish` — enables safe retry on failure

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines Shared, BuildingBlocks, App.Infrastructure, module project boundaries
- [[02-solution-layer-structure.solution.skill]] — Shared has no deps; BuildingBlocks references Shared; App.Infrastructure references BuildingBlocks
- [[03-domain-configuration.solution.skill]] — `OutboxMessageConfig` follows the EF configuration pattern
- [[06-domain-behaviour.solution.skill]] — entity behavior methods are where events are raised
- [[07-ardalis-specification.solution.skill]] — idempotency specs in Application follow spec placement rules
- [[08-repository.solution.skill]] — handlers inject `IRepository<T>` for staging and `IReadRepository<T>` for idempotency checks
- [[09-command-handler.solution.skill]] — module registration pattern extended with event handler auto-scan
- [[11-unit-of-work.solution.skill]] — `IUnitOfWork` injected directly by event handlers — outside pipeline

# Requirements
- `MediatR` NuGet package — provides `INotification`, `INotificationHandler<T>`, `IPublisher`
- `System.Text.Json` NuGet package — provides `JsonSerializer` used in interceptor and dispatcher
- `Microsoft.EntityFrameworkCore` NuGet package — provides `SaveChangesInterceptor`, `DbContext`

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own `IDomainEvent` — the base interface all events implement, accessible by every layer

#### Core Principals
- Extends MediatR `INotification` — enables `INotificationHandler<T>` subscription
- `EventId` is the idempotency key — unique per event instance, never reused
- `OccurredAt` is set at creation time — immutable fact of when the event happened
- Lives in Shared — Domain, Application, BuildingBlocks, and Infrastructure all reference it

#### Structure

##### Project Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Exceptions
    ConflictException.cs    ← solution 15
  /MediatR
    ICommand.cs             ← solution 09
    IQuery.cs               ← solution 12
  /UnitOfWork
    IUnitOfWork.cs          ← solution 11
  /Repositories
    IReadRepository.cs      ← solution 08
    IRepository.cs          ← solution 08
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events/IDomainEvent.cs | Base interface for all domain and integration events | IDomainEvent.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Provides `INotification` that `IDomainEvent` extends |

#### Rules
MUST:
- `IDomainEvent` defined in Shared — accessible by Domain (raises events) and Application (handles events) without coupling to infrastructure

MUST NOT:
- Domain layer reference MediatR directly — `IDomainEvent` extending `INotification` is the only MediatR surface in Domain

---

### Class extension

#### IDomainEvent (created)

##### Goal
- Define the base contract for all domain and integration events
- Carry `EventId` as the idempotency key and `OccurredAt` as the immutable timestamp

##### Core Principals
- `EventId` is `Guid.NewGuid()` assigned at event creation — unique per instance
- `OccurredAt` is `DateTime.UtcNow` assigned at event creation — immutable
- Both properties are init-only in the implementing `record` — never changed after creation

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain event base interface | `IDomainEvent` | `IDomainEvent` | `IDomainEvent.cs` | `IDomainEvent.cs` |

##### Implementation changes

```csharp
// Shared/Events/IDomainEvent.cs
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

##### Rule changes
MUST:
- All domain and integration events implement `IDomainEvent`
- `EventId` unique per event instance — used as idempotency key in handlers

---

## BuildingBlocks (.csproj) (extended)

### Project extension

#### Goal
- Own `IHasDomainEvents` so the interceptor can discover event-carrying entities generically
- Own `OutboxMessage` as the persistence shape for domain events

#### Structure

##### Project Structure
```
/BuildingBlocks
  /Outbox
    IHasDomainEvents.cs
    OutboxMessage.cs
  /MediatR
    ValidationBehavior.cs      ← solution 10
    GuidResolvingBehavior.cs   ← solution 15
    ConcurrencyBehavior.cs     ← solution 14
    UnitOfWorkContext.cs       ← solution 11
    UnitOfWorkBehavior.cs      ← solution 11
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Outbox/IHasDomainEvents.cs | Interface allowing interceptor to discover event-carrying entities | IHasDomainEvents.class.skill |
| /Outbox/OutboxMessage.cs | Persistence shape for outbox rows | OutboxMessage.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | `INotification` referenced via `IDomainEvent` from Shared |

---

### Class extension

#### IHasDomainEvents (created)

##### Goal
- Allow `DomainEventInterceptor` to discover all entities with pending events via EF `ChangeTracker` — without knowing entity types at compile time
- Allow `ClearDomainEvents()` to be called on all entities after events are serialized

##### Core Principals
- Two members: read-only events collection and a clear method
- Implemented by every entity that raises domain events
- Used exclusively by `DomainEventInterceptor` — not by application code

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain events carrier | `IHasDomainEvents` | `IHasDomainEvents` | `IHasDomainEvents.cs` | `IHasDomainEvents.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Outbox/IHasDomainEvents.cs
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

##### Rule changes
MUST:
- Every entity that raises domain events implements `IHasDomainEvents`
- `ClearDomainEvents()` called by interceptor after serializing — never by application code

---

#### OutboxMessage (created)

##### Goal
- Represent one domain event row in the outbox table — persisted atomically with the state change
- Carry enough information to deserialize and republish the event on retry

##### Core Principals
- `Id` matches the event's `EventId` — enables idempotent outbox processing
- `Type` stores `AssemblyQualifiedName` — required for safe deserialization across app restarts and namespace refactors
- `Payload` stores JSON-serialized event — deserialized back to the exact event type by `OutboxDispatcher`
- `ProcessedAt` is null until successfully published — null rows are retried by dispatcher

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Outbox persistence row | `OutboxMessage` | `OutboxMessage` | `OutboxMessage.cs` | `OutboxMessage.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Outbox/OutboxMessage.cs
public class OutboxMessage
{
    public Guid Id { get; init; }           // = event.EventId
    public string Type { get; init; }       // AssemblyQualifiedName
    public string Payload { get; init; }    // JSON-serialized event
    public DateTime OccurredAt { get; init; }
    public DateTime? ProcessedAt { get; set; }  // null = not yet processed
}
```

##### Rule changes
MUST:
- `Id` set to `event.EventId` — links outbox row to the domain event
- `Type` uses `AssemblyQualifiedName` — never `FullName` or short name
- `ProcessedAt` set to `DateTime.UtcNow` only after successful `_mediator.Publish` — not before

MUST NOT:
- `ProcessedAt` set before `_mediator.Publish` — would silently skip failed handlers
- `Type.FullName` used — breaks deserialization after namespace changes

---

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goal
- Own the event `record` definitions for this module — internal domain events
- Extend entity classes to implement `IHasDomainEvents` and raise events in behavior methods

#### Structure

##### Project Structure
```
/{Module}.Domain
  /Events
    {Entity}{Verb}Event.cs
  /Entities
    {Entity}.cs    ← extended with _domainEvents and IHasDomainEvents
  /Specifications  ← solution 07
  /Configurations  ← solution 03
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events/{Entity}{Verb}Event.cs | Immutable domain event record | DomainEvent.class.skill |

#### Rules
MUST:
- All domain events for this module live in `/{Module}.Domain/Events`
- Domain project does NOT reference MediatR directly — `IDomainEvent` extends `INotification` via Shared

MUST NOT:
- Domain event class shared across module boundaries — use integration event contract in Interfaces instead

---

### Class extension

#### DomainEvent (created)

##### Goal
- Define an immutable fact that something significant happened in the domain
- Carry all data the handler needs — handlers must not query back into the publishing module

##### Core Principals
- Declared as `record` — structural equality, immutable by default
- Implements `IDomainEvent` with `EventId = Guid.NewGuid()` and `OccurredAt = DateTime.UtcNow`
- Name is past tense — `TaskAssigned`, `OrderCompleted`, `PaymentProcessed`
- Properties carry all data handlers need — never just an Id that handlers must resolve separately
- Internal to the module — cross-module communication uses integration events in Interfaces

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain event | `{Entity}{Verb}Event` | `TaskAssignedEvent` | `{Entity}{Verb}Event.cs` | `TaskAssignedEvent.cs` |

##### Implementation changes

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

```csharp
// Task.Domain/Events/TaskCompletedEvent.cs
public record TaskCompletedEvent(
    int TaskId,
    int CompletedByUserId,
    DateTime CompletedAt
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

##### Rule changes
MUST:
- Declared as `record`
- Name is past tense
- Implements `IDomainEvent` with `EventId = Guid.NewGuid()` and `OccurredAt = DateTime.UtcNow`
- Carries all data handlers need — not just entity Id

MUST NOT:
- Declared as `class` with mutable properties
- Name use imperative tense (`AssignTask`, `CompleteTask`)
- Be shared across module boundaries — declare integration event in Interfaces instead

---

#### Entity.class.skill (extended)

##### Goal
- Add `_domainEvents` collection, `DomainEvents` property, and `ClearDomainEvents()` to every entity that raises events
- Implement `IHasDomainEvents` so the outbox interceptor can discover the entity

##### Core Principals
- `_domainEvents` is private — only the entity's own behavior methods add to it
- `DomainEvents` is a public read-only property — interceptor reads it, never writes to it
- `ClearDomainEvents()` is public — called by interceptor after serializing, never by application code
- Events are raised inside behavior methods alongside the state change — never raised separately

##### Implementation changes
Entity extended with event collection and `IHasDomainEvents`:

```csharp
// Task.Domain/Entities/TodoTask.cs
public class TodoTask : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();

    public int Id { get; internal set; }
    public string Title { get; internal set; }
    public TaskStatus Status { get; internal set; }
    public int AssigneeId { get; internal set; }
    public uint Version { get; internal set; }  // solution 14

    internal void Assign(int assigneeId)
    {
        if (assigneeId <= 0)
            throw new DomainException("Invalid assignee");

        AssigneeId = assigneeId;

        // event raised alongside state change — same transaction
        _domainEvents.Add(new TaskAssignedEvent(Id, assigneeId));
    }

    internal void Complete(int completedByUserId)
    {
        if (Status == TaskStatus.Completed)
            throw new DomainException("Task is already completed");

        Status = TaskStatus.Completed;
        _domainEvents.Add(new TaskCompletedEvent(Id, completedByUserId, DateTime.UtcNow));
    }
}
```

##### Rule changes
MUST:
- Entity implements `IHasDomainEvents`
- `_domainEvents` is `private readonly List<IDomainEvent>`
- `DomainEvents` is `public IReadOnlyList<IDomainEvent>`
- `ClearDomainEvents()` is `public void` — called only by interceptor
- Events added inside behavior methods alongside the state change

MUST NOT:
- Application code call `_domainEvents.Add(...)` directly — only entity behavior methods add events
- Application code call `ClearDomainEvents()` — only interceptor calls it
- Entity reference MediatR or any dispatcher

---

## {Module}.Interfaces (.csproj) (extended)

### Project extension

#### Goal
- Own integration event contracts — the cross-module event surface of this module
- Separate from domain events — integration events are what other modules subscribe to

#### Structure

##### Project Structure
```
/{Module}.Interfaces
  /Commands    ← solution 09
  /Queries     ← solution 12
  /DTOs        ← solution 12
  /Events
    {Entity}{Verb}Event.cs    ← integration event contract
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events/{Entity}{Verb}Event.cs | Integration event contract for cross-module subscribers | IntegrationEvent.class.skill |

#### Rules
MUST:
- Integration events that cross module boundaries declared in `/{Module}.Interfaces/Events`
- Integration event class is a separate declaration from the domain event — same structure, different placement

MUST NOT:
- Domain event class from `/{Module}.Domain/Events` referenced directly by another module
- Integration event carry domain entity types as properties — primitives only

---

### Class extension

#### IntegrationEvent (created)

##### Goal
- Declare the cross-module event contract that other modules subscribe to
- Be structurally identical to the domain event but placed in Interfaces — the stable public surface

##### Core Principals
- Same structure as the domain event — but declared in Interfaces, not Domain
- The domain event and integration event may share the same class if the event only needs one contract, or be separate if the public contract differs from the internal one
- When domain event and integration event are the same class, place it in Interfaces and reference from Domain via `{Module}.Interfaces` dependency

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Integration event | `{Entity}{Verb}Event` | `TaskAssignedEvent` | `{Entity}{Verb}Event.cs` | `TaskAssignedEvent.cs` |

##### Implementation changes

```csharp
// Task.Interfaces/Events/TaskAssignedEvent.cs
public record TaskAssignedEvent(
    int TaskId,
    int AssigneeId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

##### Rule changes
MUST:
- Properties are primitives — no domain entity types
- Name is past tense
- Implements `IDomainEvent`

---

## App.Infrastructure (.csproj) (extended)

### Project extension

#### Goal
- Own `DomainEventInterceptor` — writes outbox rows in the same transaction as the state change
- Own `OutboxDispatcher` — polls outbox and publishes events to MediatR
- Own `OutboxMessageConfig` — EF configuration for the outbox table

#### Structure

##### Project Structure
```
/App.Infrastructure
  /Outbox
    DomainEventInterceptor.cs
    OutboxDispatcher.cs
  /Persistence
    /Configurations
      OutboxMessageConfig.cs
  /Repositories
    Repository.cs          ← solution 08
  /UnitOfWork
    UnitOfWork.cs          ← solution 11
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Outbox/DomainEventInterceptor.cs | EF SaveChanges interceptor — writes outbox rows atomically | DomainEventInterceptor.class.skill |
| /Outbox/OutboxDispatcher.cs | Background service — polls outbox and publishes to MediatR | OutboxDispatcher.class.skill |
| /Persistence/Configurations/OutboxMessageConfig.cs | EF table configuration for OutboxMessage | OutboxMessageConfig.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.EntityFrameworkCore` | `SaveChangesInterceptor`, `DbContext`, `ChangeTracker` |
| `System.Text.Json` | `JsonSerializer` for event serialization/deserialization |
| `MediatR` | `IPublisher` used by `OutboxDispatcher` to publish events |

#### Rules
MUST:
- `DomainEventInterceptor` registered on `AppDbContext` as an interceptor
- `OutboxDispatcher` registered as `IHostedService` in App.Host
- `OutboxMessage` EF configuration applied via `ApplyConfigurationsFromAssembly`

---

### Class extension

#### DomainEventInterceptor (created)

##### Goal
- Intercept EF `SavingChangesAsync` to collect pending domain events from all tracked entities and write them as `OutboxMessage` rows — in the same transaction as the state change

##### Core Principals
- Inherits `SaveChangesInterceptor` — fires before EF writes to DB
- Reads `IHasDomainEvents` from all tracked `ChangeTracker` entries — entity type agnostic
- Serializes each event to JSON with `AssemblyQualifiedName` as the type identifier
- Writes all `OutboxMessage` rows in the same `SaveChangesAsync` call — atomic with state change
- Calls `ClearDomainEvents()` on all entities after serializing — prevents double processing
- If no events exist, passes through without side effects

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| EF event interceptor | `DomainEventInterceptor` | `DomainEventInterceptor` | `DomainEventInterceptor.cs` | `DomainEventInterceptor.cs` |

##### Implementation changes

```csharp
// App.Infrastructure/Outbox/DomainEventInterceptor.cs
public class DomainEventInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
            return await base.SavingChangesAsync(eventData, result, ct);

        // collect all entities with pending events
        var entities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        if (entities.Count == 0)
            return await base.SavingChangesAsync(eventData, result, ct);

        // serialize all events to outbox rows
        var outboxMessages = entities
            .SelectMany(e => e.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.EventId,
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredAt = domainEvent.OccurredAt
            })
            .ToList();

        // write outbox rows — same transaction as state change
        await dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, ct);

        // clear events — prevents double processing on retry
        foreach (var entity in entities)
            entity.ClearDomainEvents();

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
```

##### Rule changes
MUST:
- Call `base.SavingChangesAsync` after writing outbox rows — never short-circuit
- Use `AssemblyQualifiedName` for `Type` — never `FullName` or `GetType().Name`
- Call `ClearDomainEvents()` after serializing — prevents double serialization on second SaveChanges
- Early return (pass through) when no entities have events

MUST NOT:
- Call `SaveChangesAsync` inside the interceptor — it fires during SaveChanges, calling it again causes infinite recursion
- Dispatch events directly — only write to outbox

---

#### OutboxDispatcher (created)

##### Goal
- Poll the outbox table on a fixed interval for unprocessed messages
- Deserialize and publish each event to MediatR
- Mark `ProcessedAt` only after successful publish — enables safe retry on failure

##### Core Principals
- `BackgroundService` — runs as a hosted service in the background
- Processes events in `OccurredAt` order — maintains causality within a batch
- Batches with `Take(20)` — never unbounded query
- `ProcessedAt` set after `_mediator.Publish` succeeds — not before
- If `_mediator.Publish` throws, `ProcessedAt` remains null — message retried on next poll
- Saves `ProcessedAt` updates after processing the full batch — one `SaveChangesAsync` per batch
- Polls every 5 seconds — configurable

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Outbox background dispatcher | `OutboxDispatcher` | `OutboxDispatcher` | `OutboxDispatcher.cs` | `OutboxDispatcher.cs` |

##### Implementation changes

```csharp
// App.Infrastructure/Outbox/OutboxDispatcher.cs
public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await ProcessBatchAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        // create a scope — DbContext is Scoped, BackgroundService is Singleton
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            var eventType = Type.GetType(message.Type);
            if (eventType is null)
            {
                // unknown type — mark as processed to avoid blocking the queue
                message.ProcessedAt = DateTime.UtcNow;
                continue;
            }

            var domainEvent = (IDomainEvent)JsonSerializer
                .Deserialize(message.Payload, eventType)!;

            // publish — if this throws, ProcessedAt is NOT set → message retried next poll
            await mediator.Publish(domainEvent, ct);

            // set only after successful publish
            message.ProcessedAt = DateTime.UtcNow;
        }

        // save all ProcessedAt updates in one batch
        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(ct);
    }
}
```

> **Note on scope creation:** `OutboxDispatcher` is registered as `Singleton` but `DbContext` is `Scoped`. A new DI scope is created per batch to resolve Scoped services correctly.

##### Rule changes
MUST:
- Create a new DI scope per batch — `DbContext` is Scoped, dispatcher is Singleton
- Set `ProcessedAt` after successful `_mediator.Publish` — not before
- Use `Take(N)` — never unbounded query
- Process in `OccurredAt` order — `OrderBy(m => m.OccurredAt)`
- Use `IPublisher` — not `ISender` — events are notifications, not requests

MUST NOT:
- Set `ProcessedAt` before `_mediator.Publish` — would silently lose failed deliveries
- Use `ISender` for event dispatch — use `IPublisher`

---

#### OutboxMessageConfig (created)

##### Goal
- Configure the `OutboxMessage` EF table — primary key, required columns, index on unprocessed messages

##### Implementation changes

```csharp
// App.Infrastructure/Persistence/Configurations/OutboxMessageConfig.cs
public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Payload)
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        // index for efficient unprocessed message polling
        builder.HasIndex(m => m.ProcessedAt)
            .HasFilter("\"ProcessedAt\" IS NULL")
            .HasDatabaseName("IX_OutboxMessage_Unprocessed");
    }
}
```

##### Rule changes
MUST:
- Partial index on `ProcessedAt IS NULL` for efficient dispatcher polling
- `Type` max length set — `AssemblyQualifiedName` can be long

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Own `INotificationHandler<TEvent>` event handler implementations in `/EventHandlers`
- Own idempotency specs in `/Specifications` — used by event handlers to check prior processing
- Event handlers auto-discovered by `AddMediatR` assembly scan — no additional registration needed

#### Structure

##### Project Structure
```
/{Module}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs
      CreateTask.Validator.cs
  /EventHandlers
    {Entity}{Verb}EventHandler.cs
  /Resolvers             ← solution 15
    Create{Entity}GuidResolver.cs
  /Specifications
    {Entity}ByEventIdSpec.cs    ← idempotency spec
  {Module}ApplicationRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /EventHandlers/{Entity}{Verb}EventHandler.cs | Idempotent INotificationHandler implementation | EventHandler.class.skill |
| /Specifications/{Entity}ByEventIdSpec.cs | Idempotency check spec — has this event been processed? | IdempotencySpec.class.skill |

#### Rules
MUST:
- Event handlers live in `/EventHandlers` — not inside `/Features`
- Idempotency specs live in `/Specifications` — not inside `/EventHandlers`
- Event handlers auto-discovered by `AddMediatR` scan in module registration — no extra registration

MUST NOT:
- Event handler reference the publishing module's Domain or Application directly
- Event handler placed inside `/Features`

---

### Class extension

#### IdempotencySpec (created)

##### Goal
- Check whether a specific event has already been processed by this handler — the idempotency query

##### Core Principals
- Filters by `CreatedByEventId` (or equivalent field on the side-effect entity)
- Placed in `/{Module}.Application/Specifications` — feature-specific, not reusable across modules
- Named `{Entity}ByEventIdSpec` — reflects intent: find evidence of prior processing

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Idempotency check spec | `{Entity}ByEventIdSpec` | `TimeEntryByEventIdSpec` | `{Entity}ByEventIdSpec.cs` | `TimeEntryByEventIdSpec.cs` |

##### Implementation changes

```csharp
// TimeLog.Application/Specifications/TimeEntryByEventIdSpec.cs
public class TimeEntryByEventIdSpec : Specification<TimeEntry>
{
    public TimeEntryByEventIdSpec(Guid eventId)
    {
        Query.Where(e => e.CreatedByEventId == eventId);
    }
}
```

> **Note:** The side-effect entity must carry `CreatedByEventId` — the `EventId` of the event that caused its creation. This is what makes the idempotency check possible.

##### Rule changes
MUST:
- Filter by `EventId` of the event — not by entity Id or other fields
- Live in `/{Module}.Application/Specifications`

---

#### EventHandler (created)

##### Goal
- React to a domain or integration event as a side effect in the subscribing module
- Guarantee idempotency — same event delivered twice produces same result as once

##### Core Principals
- Implements `INotificationHandler<TEvent>`
- Injects `IRepository<T>` for staging side-effect entities and `IUnitOfWork` for committing
- Calls `_unitOfWork.SaveChangesAsync()` directly — event handlers are outside the command pipeline
- Idempotency guard is mandatory — `AnyAsync` with idempotency spec before any side effect
- Returns early (no throw) when duplicate detected
- Cross-module handler depends only on `{SourceModule}.Interfaces` — never on Domain or Application of another module

##### Three idempotency strategies

| Strategy | When to use | Example |
| --- | --- | --- |
| Check-before-act | Side effect creates a new entity with `CreatedByEventId` | `AnyAsync(new TimeEntryByEventIdSpec(eventId))` |
| Natural idempotency | Operation is inherently safe to repeat | `entity.SetStatus(status)` — same status twice = no-op |
| Upsert | DB-level insert-or-ignore | `INSERT ... ON CONFLICT DO NOTHING` |

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain event handler | `{Entity}{Verb}EventHandler` | `TaskAssignedEventHandler` | `{Entity}{Verb}EventHandler.cs` | `TaskAssignedEventHandler.cs` |

##### Implementation changes

Check-before-act (most common — creates a new entity as the side effect):

```csharp
// TimeLog.Application/EventHandlers/TaskAssignedEventHandler.cs
public class TaskAssignedEventHandler
    : INotificationHandler<TaskAssignedEvent>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskAssignedEventHandler(
        IRepository<TimeEntry> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken ct)
    {
        // idempotency guard — check if this event was already processed
        var alreadyProcessed = await _repository.AnyAsync(
            new TimeEntryByEventIdSpec(notification.EventId), ct);

        if (alreadyProcessed)
            return; // duplicate delivery — return without throwing

        // side effect — create time entry as reaction to task assignment
        var entry = TimeEntry.Create(
            notification.TaskId,
            notification.AssigneeId,
            createdByEventId: notification.EventId);  // stores EventId for future idempotency checks

        await _repository.AddAsync(entry, ct);

        // handlers commit directly — they are outside the UnitOfWorkBehavior pipeline
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

Natural idempotency (state update safe to repeat):

```csharp
// Task.Application/EventHandlers/TaskStatusSyncEventHandler.cs
public class TaskStatusSyncEventHandler
    : INotificationHandler<ExternalStatusChangedEvent>
{
    private readonly IRepository<TodoTask> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(
        ExternalStatusChangedEvent notification, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(notification.TaskId), ct);

        if (task is null)
            return; // entity no longer exists — skip silently

        // naturally idempotent — setting the same status twice is a no-op
        task.SetStatus(notification.NewStatus);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

##### Rule changes
MUST:
- Implement `INotificationHandler<TEvent>`
- Have explicit idempotency guard before any side effect
- Call `_unitOfWork.SaveChangesAsync()` directly — outside pipeline
- Return early on duplicate — never throw
- Live in `/{Module}.Application/EventHandlers`
- For cross-module events, depend only on `{SourceModule}.Interfaces` — never Domain or Application

MUST NOT:
- Assume exactly-once delivery — at-least-once is guaranteed
- Throw on duplicate event detection — return early
- Reference publishing module's Domain or Application
- Dispatch commands from event handler without careful consideration of nested UoW implications

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `DomainEventInterceptor` on `AppDbContext`
- Register `OutboxDispatcher` as a hosted service

---

### Class extension

#### InfrastructureRegistration (created)

##### Goal
- Register infrastructure concerns: DbContext with interceptor, OutboxDispatcher as hosted service

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Infrastructure DI registration | `InfrastructureRegistration` | `InfrastructureRegistration` | `InfrastructureRegistration.cs` | `InfrastructureRegistration.cs` |

##### Implementation changes

```csharp
// App.Host/DependencyInjection/InfrastructureRegistration.cs
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // register interceptor as singleton — stateless
        services.AddSingleton<DomainEventInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));

            // register interceptor on DbContext
            options.AddInterceptors(
                sp.GetRequiredService<DomainEventInterceptor>());
        });

        // outbox dispatcher as hosted background service
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
```

Program.cs extended:

```csharp
// App.Host/Program.cs
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApi()
    .AddPipeline()
    .AddRepositories()
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration)
    .RegisterAppQueries();
```

##### Rule changes
MUST:
- `DomainEventInterceptor` registered as `Singleton` — stateless, safe for singleton lifetime
- `DomainEventInterceptor` added to `AppDbContext` via `AddInterceptors`
- `OutboxDispatcher` registered via `AddHostedService<OutboxDispatcher>()`

MUST NOT:
- `DomainEventInterceptor` registered as `Scoped` — interceptors are resolved before DbContext scope; Scoped registration causes lifetime mismatch

---

# Rules

MUST:
- `IDomainEvent` defined in Shared — extends `INotification`
- `IHasDomainEvents` and `OutboxMessage` defined in BuildingBlocks
- Domain events defined as `record` in `/{Module}.Domain/Events` — past tense names
- Integration events declared in `/{Module}.Interfaces/Events`
- Every entity raising events implements `IHasDomainEvents`
- Events raised inside behavior methods alongside state changes — same transaction
- `DomainEventInterceptor` writes outbox rows in same `SaveChangesAsync` as state change
- `DomainEventInterceptor` registered as `Singleton` on `AppDbContext`
- `OutboxDispatcher` registered as hosted service
- `OutboxMessage.Type` uses `AssemblyQualifiedName`
- `ProcessedAt` set only after successful `_mediator.Publish`
- All event handlers implement `INotificationHandler<TEvent>`
- All event handlers have explicit idempotency guard
- Event handlers call `_unitOfWork.SaveChangesAsync()` directly — outside command pipeline
- Event handlers live in `/{Module}.Application/EventHandlers`
- Event handlers return early on duplicate — never throw

MUST NOT:
- Entity reference MediatR or any dispatcher — only collects events
- Application code call `ClearDomainEvents()` — only interceptor calls it
- `ProcessedAt` set before `_mediator.Publish` — silently skips failures
- Domain event class shared across module boundaries — use Interfaces integration event
- Event handler reference publishing module's Domain or Application
- `OutboxDispatcher` use `ISender` — use `IPublisher` for notifications
- `DomainEventInterceptor` call `SaveChangesAsync` — causes infinite recursion

# Anti-patterns
- Entity calls `_mediator.Publish()` directly — infrastructure dependency in domain, phantom events on rollback
- Dispatching events after `SaveChangesAsync` without outbox — crash between save and dispatch loses event permanently
- `ProcessedAt` set before `_mediator.Publish` — failed handler silently skipped, never retried
- `Type.FullName` in `OutboxMessage` — breaks deserialization after namespace refactor
- Unbounded outbox query without `Take(N)` — performance degradation under load
- Event handler throws on duplicate — blocks outbox retry, message stuck in failed state
- Event handler depends on publishing module's Domain — tight coupling across module boundaries
- `DomainEventInterceptor` registered as `Scoped` — lifetime mismatch, interceptor resolved before DbContext scope

# Check list
- [ ] `IDomainEvent` defined in `Shared/Events/IDomainEvent.cs`
- [ ] `IHasDomainEvents` defined in `BuildingBlocks/Outbox/IHasDomainEvents.cs`
- [ ] `OutboxMessage` defined in `BuildingBlocks/Outbox/OutboxMessage.cs`
- [ ] `OutboxMessage.Type` uses `AssemblyQualifiedName`
- [ ] Domain events in `/{Module}.Domain/Events` — `record`, past tense, implements `IDomainEvent`
- [ ] Integration events in `/{Module}.Interfaces/Events`
- [ ] Every event-raising entity implements `IHasDomainEvents`
- [ ] `_domainEvents` is private, `DomainEvents` is public read-only, `ClearDomainEvents()` is public
- [ ] Events raised inside behavior methods alongside state change
- [ ] `DomainEventInterceptor` defined in `App.Infrastructure/Outbox`
- [ ] `DomainEventInterceptor` registered as `Singleton` on `AppDbContext` via `AddInterceptors`
- [ ] `OutboxMessageConfig` defined and applied via `ApplyConfigurationsFromAssembly`
- [ ] Partial index on `ProcessedAt IS NULL` in `OutboxMessageConfig`
- [ ] `OutboxDispatcher` defined in `App.Infrastructure/Outbox`
- [ ] `OutboxDispatcher` creates new DI scope per batch
- [ ] `OutboxDispatcher` uses `Take(20)` batching
- [ ] `OutboxDispatcher` uses `IPublisher` — not `ISender`
- [ ] `ProcessedAt` set after `_mediator.Publish` — not before
- [ ] Event handlers in `/{Module}.Application/EventHandlers`
- [ ] Idempotency specs in `/{Module}.Application/Specifications`
- [ ] Every event handler has idempotency guard
- [ ] Every event handler calls `_unitOfWork.SaveChangesAsync()` directly
- [ ] Every event handler returns early on duplicate — does not throw
- [ ] `AddHostedService<OutboxDispatcher>()` registered in App.Host

# Unittest TestCases
- [ ] When entity behavior method called Then `DomainEvents` contains expected event with unique `EventId`
- [ ] When `ClearDomainEvents` called Then `DomainEvents` is empty
- [ ] When `SaveChangesAsync` called with entity having domain events Then `OutboxMessage` rows created with correct `Type` and `Payload`
- [ ] When `SaveChangesAsync` called Then entity `DomainEvents` cleared by interceptor
- [ ] When `OutboxDispatcher` runs Then `_mediator.Publish` called for each unprocessed message
- [ ] When `_mediator.Publish` succeeds Then `ProcessedAt` set to non-null
- [ ] When `_mediator.Publish` throws Then `ProcessedAt` remains null — message retried next poll
- [ ] When event handler processes event for first time Then side effect applied
- [ ] When same event delivered twice Then side effect applied exactly once — idempotency guard prevents duplicate
- [ ] When duplicate detected Then handler returns without throwing
- [ ] When event handler throws Then `ProcessedAt` not set — outbox retries delivery
