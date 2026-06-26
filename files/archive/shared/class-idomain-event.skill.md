---
name: class-idomain-event
description: defines the IDomainEvent base interface for all domain and integration events
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - domain
  - events
triggers:
  - IDomainEvent interface
  - domain event base
---
# Goal
Define `IDomainEvent`. All domain events and integration events implement this interface. Provides `EventId` for idempotency and `OccurredAt` for ordering. Extends MediatR `INotification` so events can be published via MediatR.

# Governed by
- solution-domain-events.skill.md — full event flow

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Events
    IDomainEvent.cs
```

## Naming convention
```
interface name: IDomainEvent
file name: IDomainEvent.cs
```

# Contracts
```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

# Rules
MUST:
- All domain events implement `IDomainEvent`
- All integration events implement `IDomainEvent`
- `EventId` initialized to `Guid.NewGuid()` — unique per event instance
- `OccurredAt` initialized to `DateTime.UtcNow`

# Relations
- csproj-shared.skill.md — lives here
- class-domain-event.skill.md — domain events implement this
- class-integration-event.skill.md — integration events implement this
- solution-domain-events.skill.md — EventId used for handler idempotency
