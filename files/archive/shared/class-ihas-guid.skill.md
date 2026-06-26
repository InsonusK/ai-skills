---
name: class-ihas-guid
description: defines the IHasGuid marker interface that opts a command into GuidResolvingBehavior
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - guid
  - idempotency
triggers:
  - IHasGuid interface
  - guid marker
  - opt into guid resolving
---
# Goal
Define the `IHasGuid` marker interface. Commands implement this to signal they carry a client-generated Guid and should be intercepted by `GuidResolvingBehavior` for idempotency checking.

# Governed by
- solution-guid-resolving.skill.md — full pipeline this marker activates

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Mediatr
    IHasGuid.cs
```

## Naming convention
```
interface name: IHasGuid
file name: IHasGuid.cs
```

# Contracts
```csharp
public interface IHasGuid
{
    Guid Guid { get; }
}
```

# Rules
MUST:
- All creation commands for externally created entities implement `IHasGuid`
MUST NOT:
- Update or delete commands implement `IHasGuid` — Guid is set only on creation

# Relations
- csproj-shared.skill.md — lives here
- class-command.skill.md — creation commands implement this
- solution-guid-resolving.skill.md — GuidResolvingBehavior activates on this marker
- solution-external-created-entity.skill.md — entity must have Guid field and unique index
