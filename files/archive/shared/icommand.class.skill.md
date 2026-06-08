---
name: icommand
description: defines the ICommand marker interface that activates UnitOfWorkBehavior for write operations
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - cqrs
  - mediatr
triggers:
  - ICommand interface
  - command marker
  - write intent marker
---
# Goal
Define the `ICommand` marker interface. Commands implement this to signal write intent and activate `UnitOfWorkBehavior` in the MediatR pipeline. Queries must never implement this.

# Governed by
- command-handling.solution.skill.md — UnitOfWorkBehavior activates on ICommand only

# Structure
## Place in csproj
Defined in `shared.csproj.skill.md`
```
/Shared
  /Mediatr
    ICommand.cs
```

## Naming convention
```
interface name: ICommand / ICommand<TResponse>
file name: ICommand.cs
```

# Contracts
```csharp
public interface ICommand : IRequest { }
public interface ICommand<TResponse> : IRequest<TResponse> { }
```

# Rules
MUST:
- All write operation commands implement `ICommand<Result<T>>`
- Commands that return no value implement `ICommand<Result>`
MUST NOT:
- Query implement `ICommand` — breaks read-only contract and triggers SaveChanges

# Relations
- shared.csproj.skill.md — lives here
- command.class.skill.md — commands in Interfaces implement this
- command-handling.solution.skill.md — UnitOfWorkBehavior constraint on ICommand
