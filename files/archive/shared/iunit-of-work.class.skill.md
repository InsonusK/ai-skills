---
name: iunit-of-work
description: defines IUnitOfWork — the single commit abstraction used by UnitOfWorkBehavior
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - unit-of-work
triggers:
  - IUnitOfWork interface
  - unit of work abstraction
  - SaveChanges abstraction
---
# Goal
Define `IUnitOfWork`. This is the only place `SaveChangesAsync` is declared. `UnitOfWorkBehavior` calls it after the handler completes. Handlers and repositories never call it directly.

# Governed by
- command-handling.solution.skill.md — UnitOfWorkBehavior is the only caller

# Structure
## Place in csproj
Defined in `shared.csproj.skill.md`
```
/Shared
  /UnitOfWork
    IUnitOfWork.cs
```

## Naming convention
```
interface name: IUnitOfWork
file name: IUnitOfWork.cs
```

# Contracts
```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

# Rules
MUST:
- `UnitOfWorkBehavior` is the only caller of `SaveChangesAsync`
MUST NOT:
- Handler call `SaveChangesAsync` directly
- Repository expose `SaveChangesAsync`

# Relations
- shared.csproj.skill.md — lives here
- irepository.class.skill.md — repository stages changes, IUnitOfWork commits them
- command-handling.solution.skill.md — UnitOfWorkBehavior calls this after handler
