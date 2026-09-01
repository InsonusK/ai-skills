---
description: Query and response record declaration
project_name: "{Module}.Interfaces"
name: "{Query}.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/query-cs
---

# Goals
- Declare a read operation as an immutable record implementing `IQuery<TResponse>`, with its response DTO co-located.

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Queries
    Get{Thing}.cs        — query + its response DTO
  /DTOs
    {Thing}Dto.cs        — shared response shape when reused by several queries
```

# Implementation changes

```csharp
// {Module}.Interfaces/Queries/GetTaskById.cs
using Ardalis.Result;
using Shared;

namespace {Module}.Interfaces.Queries;

public record GetTaskById(Guid Id) : IQuery<Result<TaskDto>>;

public record TaskDto(Guid Id, string Title, SoftComplexity Complexity);
```

A query that always returns data (a list that can be empty, a count) uses `IQuery<IReadOnlyList<TaskDto>>` — no `Result<>` wrapper.

# Rule changes

## MUST
- Declare the query as a `record` in `/{Module}.Interfaces/Queries`, implementing `IQuery<...>`.
  - Risk: a class or a raw `IRequest<T>` loses immutability and the read/write distinction.
  - Fix: `public record GetX(...) : IQuery<Result<XDto>>;`.
- Type response DTO properties with `Soft{ValueObject}` or primitives, never the domain `{ValueObject}`.
  - Risk: `{Module}.Interfaces` would depend on `{Module}.Domain`.
  - Fix: the handler maps `{ValueObject}` → `Soft{ValueObject}` when projecting.

## SHOULD
- Co-locate a single-use response DTO in the query file; move a DTO shared by several queries to `/DTOs`.

# Check list
- [ ] Query is a `record` in `/Queries` implementing `IQuery<...>`.
- [ ] Response DTO uses `Soft{ValueObject}`/primitives only.
- [ ] `Result<T>` wrapper present iff the query can fail to find its target.
