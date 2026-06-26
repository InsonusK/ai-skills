---
uid:
name: solution-command-handling
description: defines the full command pipeline — validation, guid resolving, concurrency check, unit of work, handler execution, and result mapping
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - cqrs
  - mediatr
  - pipeline
triggers:
  - command pipeline
  - mediatr pipeline
  - command handling flow
  - pipeline behavior order
---
# Goal
Define the full flow of a write operation from HTTP request through the MediatR pipeline to the handler and back to HTTP response. Establishes the correct pipeline behavior order and the responsibilities of each layer. Without this solution, behaviors are registered in the wrong order, handlers contain logic that belongs in the pipeline, and the unit of work commits at wrong times.

# Core Principles
- Pipeline behaviors execute in registration order — order is not arbitrary
- Handler orchestrates — it never contains business rules or validation
- Only the top-level command commits — sub-commands dispatched from handlers defer to root
- Handler never calls `SaveChangesAsync` — `UnitOfWorkBehavior` owns this
- Every `ResultStatus` from handler has explicit HTTP mapping in controller
- Unexpected `ResultStatus` throws — never returns undocumented response

# Depend on
_none — this is a root pipeline solution_

# Flow
```
HTTP POST/PUT/PATCH/DELETE
    ↓
Controller — extracts If-Match, maps body to Command, calls _sender.Send()
    ↓
ValidationBehavior — runs FluentValidation, returns Result.Invalid if invalid
    ↓
GuidResolvingBehavior — (IHasGuid commands only) checks Guid, returns Result.Conflict if duplicate
    ↓
ConcurrencyBehavior — (IHasVersions commands only) validates versions, returns Result.Conflict if mismatch
    ↓
UnitOfWorkBehavior — increments depth counter
    ↓
Handler — loads via IRepository + Spec → guard → domain call → stage
    ↓
UnitOfWorkBehavior — SaveChanges if depth == 1 (top-level command only)
    ↓
Controller — maps Result<T> status to HTTP response
```

# Implementation

## Pipeline registration order — `App.Host`
Order is critical — wrong order breaks validation, idempotency, and atomicity.
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
```

## ICommand.cs — `BuildingBlocks`
Marker interface — activates `UnitOfWorkBehavior` for commands only.
```csharp
public interface ICommand : IRequest { }
public interface ICommand<TResponse> : IRequest<TResponse> { }
```

## UnitOfWorkBehavior.cs — `BuildingBlocks`
Only depth == 1 commits — sub-commands dispatched from handlers defer to root.
```csharp
public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _context.Depth++;
        try
        {
            var response = await next();
            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);
            return response;
        }
        finally
        {
            _context.Depth--;
        }
    }
}
```

## {CommandName}.Handler.cs — `{Module}.Application/Commands/{CommandName}`
Standard handler structure: load → guard → domain → stage → return.
```csharp
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IRepository<Task> _repository;

    public async Task<Result<CreateTaskResult>> Handle(
        CreateTaskCommand command, CancellationToken ct)
    {
        // load
        var assignee = await _repository.FirstOrDefaultAsync(
            new UserByIdSpec(command.AssigneeId), ct);

        // guard
        if (assignee is null) return Result.NotFound();

        // domain call
        var task = Task.Create(command.Guid, command.Title, command.AssigneeId);

        // stage
        await _repository.AddAsync(task, ct);

        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

## {Entity}Controller.cs — `{Module}.Api/Controllers`
Every ResultStatus explicitly handled — unexpected throws.
```csharp
var result = await _sender.Send(command, ct);

return result.Status switch
{
    ResultStatus.Created => CreatedAtAction(..., result.Value),
    ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
    ResultStatus.NotFound => NotFound(),
    ResultStatus.Conflict => Conflict(ToProblemDetails(result.Errors)),
    _ => throw new InvalidOperationException(
        $"Unexpected result status '{result.Status}'.")
};
```

# Example
```
POST /task { guid: "abc", title: "My Task", assigneeId: 5 }
    ↓
ValidationBehavior: title not empty ✓, assigneeId > 0 ✓
GuidResolvingBehavior: guid "abc" not found ✓
UnitOfWorkBehavior: depth = 1
Handler: loads user#5 → exists ✓ → Task.Create() → AddAsync()
UnitOfWorkBehavior: depth == 1 → SaveChanges → depth = 0
Controller: ResultStatus.Created → 201 { id: 42 }
```

# Rules
MUST:
- Pipeline behaviors registered in order: Validation → GuidResolving → Concurrency → UnitOfWork
- `UnitOfWorkBehavior` activates only for `ICommand` — never for queries
- `UnitOfWorkContext` registered as `Scoped` — one per HTTP request
- Handler follows load → guard → domain → stage → return
- Handler never calls `SaveChangesAsync`
- Controller maps every `ResultStatus` explicitly
- Unexpected `ResultStatus` throws `InvalidOperationException`
MUST NOT:
- Handler contain business rules — delegate to domain
- Handler dispatch multiple top-level commands sequentially — design as orchestrating command
- Pipeline behaviors registered inside module registration — register once in App.Host

# Anti-patterns
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — unit of work opens before validation fails
- Handler calls `_unitOfWork.SaveChangesAsync()` — behavior owns this
- Controller returns undocumented status — missing switch arm
- Sub-command calls `SaveChangesAsync` — breaks atomicity with root command

# Checklist
- [ ] Pipeline behaviors registered in correct order in App.Host
- [ ] `UnitOfWorkContext` registered as Scoped
- [ ] All commands implement `ICommand<Result<T>>`
- [ ] Handler follows load → guard → domain → stage → return
- [ ] No `SaveChangesAsync` in any handler
- [ ] Controller has explicit arm for every expected ResultStatus
- [ ] Controller has `_ => throw` arm for unexpected statuses

# Unittest TestCases
- [ ] When invalid command Then ValidationBehavior returns Result.Invalid, handler never runs
- [ ] When valid command Then handler runs and SaveChanges called exactly once
- [ ] When handler dispatches sub-command Then SaveChanges called once after root completes
- [ ] When handler throws Then SaveChanges not called
- [ ] When unexpected ResultStatus Then controller throws InvalidOperationException

# Relations
- solution-guid-resolving.skill.md — GuidResolvingBehavior in pipeline
- solution-concurrency-control.skill.md — ConcurrencyBehavior in pipeline
- solution-domain-events.skill.md — SaveChanges triggers DomainEventInterceptor
- class-feature-command-handler.skill.md — handler implementation
- class-api-controller.skill.md — controller result mapping
