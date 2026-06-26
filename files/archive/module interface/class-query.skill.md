---
name: class-query
description: defines how to declare a query contract in the Interfaces project
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - cqrs
  - query
  - mediatr
triggers:
  - declare query
  - create query contract
  - read intent contract
---
# Goal
Define how to declare a MediatR query in `{Module}.Interfaces`. A query expresses read intent — it carries the input needed to fetch and return data without modifying state. The declaration lives in Interfaces so any module can dispatch it.

# Core Principles
- Query is a declaration only — no logic, no validation
- Implements `IQuery<Result<T>>` — excludes query from `UnitOfWorkBehavior`
- `record` type — immutable input contract
- Returns `Result<T>` where T is a DTO — never a domain entity
- Query never modifies state — handler must enforce this

# Governed by
- solution-command-handling.skill.md — IQuery marker excludes query from UnitOfWork pipeline

# Structure
## Place in csproj
Defined in `csproj-module-interfaces.skill.md`
```
/{ModuleName}.Interfaces
  /Queries
    GetTaskQuery.cs
    GetTasksQuery.cs
```

## Naming convention
```
class name:
  rule: Get + entity name + Query suffix (singular for one, plural for collection)
  pattern: Get{Entity}Query / Get{Entities}Query
  example: GetTaskQuery, GetTasksQuery

file name:
  rule: matches class name exactly
  pattern: Get{Entity}Query.cs
  example: GetTaskQuery.cs
```

# Contracts

## Single entity query
```csharp
public record GetTaskQuery(int Id) : IQuery<Result<TaskDto>>;
```

## Collection query with filters
```csharp
public record GetTasksQuery(
    int AssigneeId,
    int Page = 1,
    int RowCount = 20
) : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>;
```

## Cross-module query — declared here, implemented in App.Queries
```csharp
// Task.Interfaces/Queries/GetTaskWithUserDetailsQuery.cs
public record GetTaskWithUserDetailsQuery(int TaskId)
    : IQuery<Result<TaskWithUserDetailsDto>>;
```

# Rules
MUST:
- Implement `IQuery<Result<T>>`
- Be a `record` type
- Return DTO type — never domain entity
- Result DTO declared in `/DTOs` folder
MUST NOT:
- Implement `ICommand` — query must never activate UnitOfWorkBehavior
- Contain logic
- Reference Domain entities

# Anti-patterns
- Query implements `ICommand` — triggers SaveChanges pipeline, breaks read-only contract
- Query returns domain entity directly — exposes internals, creates coupling
- Pagination parameters missing — unbounded queries hit DB with no limit

# Checklist
- [ ] `record` type
- [ ] Implements `IQuery<Result<T>>`
- [ ] Returns DTO — not domain entity
- [ ] Collection queries have pagination parameters
- [ ] No logic

# Relations
- csproj-module-interfaces.skill.md — project this query lives in
- class-dto.skill.md — DTO returned by this query
- class-feature-query-handler.skill.md — single-module handler
- csproj-app-queries.skill.md — cross-module handler location
