---
name: class-iquery
description: defines the IQuery marker interface that excludes read operations from UnitOfWorkBehavior
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - cqrs
  - mediatr
triggers:
  - IQuery interface
  - query marker
  - read intent marker
---
# Goal
Define the `IQuery` marker interface. Queries implement this to signal read intent and be excluded from `UnitOfWorkBehavior`. A query that accidentally implements `ICommand` would trigger SaveChanges — this marker prevents that.

# Governed by
- solution-command-handling.skill.md — UnitOfWorkBehavior skips IQuery

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Mediatr
    IQuery.cs
```

## Naming convention
```
interface name: IQuery<TResponse>
file name: IQuery.cs
```

# Contracts
```csharp
public interface IQuery<TResponse> : IRequest<TResponse> { }
```

# Rules
MUST:
- All read operations implement `IQuery<Result<T>>`
MUST NOT:
- Query implement `ICommand` — triggers UnitOfWorkBehavior and SaveChanges

# Relations
- csproj-shared.skill.md — lives here
- class-query.skill.md — queries in Interfaces implement this
- solution-command-handling.skill.md — UnitOfWorkBehavior skips IQuery
