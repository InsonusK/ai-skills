---
description: Read operation marker interface
project_name: Shared
name: IQuery.cs
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/iquery-cs
---

# Goals
- Mark a MediatR request as a read operation — request/response, no mutation, no side effect — so a reader (and a pipeline behavior) can tell it apart from a Command.

# Core Principles
- Interface only, no members.
- `IQuery<TResponse>` maps straight through to `IRequest<TResponse>`; the response is `Result<T>` for a query that can fail to find its target, or a bare DTO for one that always succeeds.
- A query handler never mutates state and never dispatches a Command.

# Structure

## Project Structure
```
/Shared
  /MediatR
    IQuery.cs
```

# Implementation changes

```csharp
// Shared/MediatR/IQuery.cs
using MediatR;

namespace Shared;

public interface IQuery<TResponse> : IRequest<TResponse> { }
```

# Rule changes

## MUST
- Declare every read operation as `IQuery<Result<T>>` (or `IQuery<T>` when it cannot fail), never `IRequest<T>` directly.
  - Risk: a raw `IRequest<T>` read is indistinguishable from a write, so behaviors that should skip reads (or apply to them) cannot select correctly.
  - Fix: implement `IQuery<TResponse>`.
- Define `IQuery<TResponse>` in `Shared/MediatR`, never in `BuildingBlocks` or a module.
  - Risk: a module referencing `BuildingBlocks` for a marker inverts the layer rule.
  - Fix: it lives beside `ICommand` in `Shared`.
- Never mutate state or dispatch a Command from a query handler.
  - Risk: a "read" with a hidden write is the hardest kind of bug to find.
  - Fix: a query handler reads and projects only.

# Check list
- [ ] `IQuery<TResponse> : IRequest<TResponse>` in `Shared/MediatR/IQuery.cs`, no members.
- [ ] Every read operation implements it.
- [ ] No query handler writes or sends a Command.
