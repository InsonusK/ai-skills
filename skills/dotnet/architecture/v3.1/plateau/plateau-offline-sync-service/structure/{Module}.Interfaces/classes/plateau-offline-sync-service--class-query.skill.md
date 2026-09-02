---
name: plateau-offline-sync-service--class-query
description: Class {Query} in the plateau-offline-sync-service plateau — a module's read-intent record (with its response DTO) in {Module}.Interfaces/Queries
whenToUse: when creating or editing a query record and its response DTO in {Module}.Interfaces/Queries
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

# Goal
- Declare one read operation as an immutable `record` implementing `IQuery<TResponse>`, with its response DTO co-located (or in `/DTOs` when several queries share it).

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md|{Query}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `record`, immutable. `IQuery<Result<{Thing}Dto>>` when it can fail to find its target; `IQuery<IReadOnlyList<{Thing}Dto>>` when it always returns data.
- Response DTO properties are `Soft{ValueObject}` or primitives — never the domain `{ValueObject}` (that would couple `{Module}.Interfaces` to `{Module}.Domain`).
- At plateau-core a query handler answers from another module or an in-memory stand-in; repository-backed reads arrive with VP2.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Single-item read | `Get{Thing}By{Key}` | `GetTaskById` | `{Query}.cs` | `GetTaskById.cs` |
| List read | `List{Things}` / `Get{Things}` | `ListTasks` | `{Query}.cs` | `ListTasks.cs` |
| Response DTO | `{Thing}Dto` | `TaskDto` | same file or `/DTOs` | `GetTaskById.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-query
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Queries;

public record GetTaskById(Guid Id) : IQuery<Result<TaskDto>>;

public record TaskDto(Guid Id, string Title, SoftComplexity Complexity);
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md|{Query}.cs.create]]

# Rules
MUST:
- Declare the query a `record` in `/{Module}.Interfaces/Queries` implementing `IQuery<...>`.
- Type response DTO properties with `Soft{ValueObject}` / primitives, never the domain `{ValueObject}`.
- Use the `Result<T>` wrapper iff the query can fail to find its target.
- Never apply several plateau templates per class.
- Never put logic in the query or the DTO.

# Check list
- [ ] Query is a `record` in `/Queries` implementing `IQuery<...>`.
- [ ] Response DTO uses `Soft{ValueObject}` / primitives only.
- [ ] `Result<T>` present iff the target can be absent.

# Unittest TestCases
- [ ] WHEN a `{Query}` is constructed THEN it is assignable to `IQuery<TResponse>`.
- [ ] WHEN the response DTO is reflected THEN no property type is a domain `{ValueObject}`.
