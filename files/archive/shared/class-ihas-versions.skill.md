---
name: class-ihas-versions
description: defines the IHasVersions marker interface that opts an update command into ConcurrencyBehavior
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - concurrency
  - etag
triggers:
  - IHasVersions interface
  - concurrency marker
  - opt into concurrency check
---
# Goal
Define the `IHasVersions` marker interface. Update commands implement this to carry decoded ETag versions and activate `ConcurrencyBehavior` in the pipeline.

# Governed by
- solution-concurrency-control.skill.md — full pipeline this marker activates

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Mediatr
    IHasVersions.cs
```

## Naming convention
```
interface name: IHasVersions
file name: IHasVersions.cs
```

# Contracts
```csharp
public interface IHasVersions
{
    // entity name → (entity id → row version)
    // example: {"Task": {"2": 3}, "TimeLog": {"1": 19}}
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

# Rules
MUST:
- All update commands for mutable entities implement `IHasVersions`
- Entity name keys are stable strings — never C# type names

# Relations
- csproj-shared.skill.md — lives here
- class-command.skill.md — update commands implement this
- solution-concurrency-control.skill.md — ConcurrencyBehavior activates on this marker
