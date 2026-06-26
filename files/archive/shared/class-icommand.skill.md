---
name: class-icommand
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
- solution-command-handling.skill.md — UnitOfWorkBehavior activates on ICommand only

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
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
- csproj-shared.skill.md — lives here
- class-command.skill.md — commands in Interfaces implement this
- solution-command-handling.skill.md — UnitOfWorkBehavior constraint on ICommand
