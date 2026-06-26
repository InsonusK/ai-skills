---
name: class-ihas-domain-events
description: defines the IHasDomainEvents marker interface for entities that collect domain events
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - domain
  - events
triggers:
  - IHasDomainEvents interface
  - entity event collection marker
---
# Goal
Define the `IHasDomainEvents` marker interface. Entities implement this so `DomainEventInterceptor` can discover all entities with pending events during SaveChanges without knowing concrete entity types.

# Governed by
- solution-domain-events.skill.md — interceptor reads this interface during SaveChanges

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Mediatr
    IHasDomainEvents.cs
```

## Naming convention
```
interface name: IHasDomainEvents
file name: IHasDomainEvents.cs
```

# Contracts
```csharp
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

# Rules
MUST:
- All entities that raise domain events implement `IHasDomainEvents`
- `ClearDomainEvents()` called by interceptor after writing outbox rows

# Relations
- csproj-shared.skill.md — lives here
- class-entity.skill.md — entities implement this
- solution-domain-events.skill.md — interceptor uses this to discover events
