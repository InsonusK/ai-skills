---
description: Query record declaration
project_name: "{Module}.Interfaces"
name: "{Query}.cs"
element_kind: class
change_kind: create
---

# Goals
- Express a named read intent as an immutable record that carries all filter/selection input needed for the operation
- Implement `IQuery<Result<T>>` so MediatR routes it to the correct handler

# Core Principles
- Declared as `record` — immutable, structural equality
- Implements `IQuery<Result<T>>` — return type is always `Result<T>` or `Result<IReadOnlyList<T>>`
- Input properties are primitives — no domain entity references
- One query per read intent — `GetTaskQuery`, `GetTasksQuery`, `GetTaskWithUserDetailsQuery`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Get single entity | `Get{Entity}Query` | `GetTaskQuery` | `Get{Entity}Query.cs` | `GetTaskQuery.cs` |
| Get collection | `Get{Entities}Query` | `GetTasksQuery` | `Get{Entities}Query.cs` | `GetTasksQuery.cs` |
| Cross-module JOIN | `Get{Entity}With{Related}Query` | `GetTaskWithUserDetailsQuery` | `Get{Entity}With{Related}Query.cs` | `GetTaskWithUserDetailsQuery.cs` |

# Implementation changes

Single entity query:

```csharp
// {Module}.Interfaces/Queries/GetTaskQuery.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Queries;

public record GetTaskQuery(int Id) : IQuery<Result<TaskDto>>;
```

Collection query:

```csharp
// {Module}.Interfaces/Queries/GetTasksQuery.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Queries;

public record GetTasksQuery(int AssigneeId) : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>;
```

Cross-module query:

```csharp
// {Module}.Interfaces/Queries/GetTaskWithUserDetailsQuery.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Queries;

public record GetTaskWithUserDetailsQuery(int TaskId)
    : IQuery<Result<TaskWithUserDetailsDto>>;
```

# Rules

MUST:
- Declared as `record`
- Implement `IQuery<Result<T>>` — never `IRequest<T>` directly
- Properties are primitives or simple types — no domain entity references

MUST NOT:
- Contain methods or logic
- Reference domain entity types as properties
- Extend `ICommand` or `ICommand<TResponse>` — queries must remain distinct from write-side markers

# Unittest TestCases
- [ ] WHEN applied THEN Express a named read intent as an immutable record that carries all filter/selection input needed for the operation
- [ ] WHEN inspected THEN it implement IQuery<Result<T>> so MediatR routes it to the correct handler
- [ ] WHEN applied THEN Declared as record — immutable, structural equality
- [ ] WHEN applied THEN Implements IQuery<Result<T>> — return type is always Result<T> or Result<IReadOnlyList<T>>
- [ ] WHEN applied THEN Input properties are primitives — no domain entity references
- [ ] WHEN applied THEN One query per read intent — GetTaskQuery, GetTasksQuery, GetTaskWithUserDetailsQuery
- [ ] WHEN naming 'Get single entity' THEN pattern matches convention
- [ ] WHEN naming 'Get collection' THEN pattern matches convention
- [ ] WHEN naming 'Cross-module JOIN' THEN pattern matches convention
