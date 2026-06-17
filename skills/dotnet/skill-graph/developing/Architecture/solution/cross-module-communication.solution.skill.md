---
uid:
name: cross-module-communication
description: defines how modules communicate — commands, queries, and events across module boundaries without direct coupling
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - architecture
  - modules
  - bounded-context
triggers:
  - cross module communication
  - call another module
  - integration event
  - cross module query
---
# Goal
Define the three allowed ways modules communicate without coupling to each other's implementation. Modules interact only through contracts declared in `{Module}.Interfaces`. Without this solution, modules reference each other's Domain or Application directly, boundaries blur, and the codebase becomes impossible to split into separate services.

# Core Principles
- Modules never depend on each other's Domain or Application — only on Interfaces
- Write intent crosses boundaries via MediatR command dispatch
- Read intent crosses boundaries via MediatR query dispatch or App.Queries for JOINs
- Event-based communication is fire-and-forget — no response expected
- All interaction contracts declared in `{Module}.Interfaces` — stable, breaking changes are versioned

# Depend on
- command-handling.solution.skill.md — cross-module commands flow through same pipeline
- domain-events.solution.skill.md — integration events travel via outbox

# Flow

## Cross-module write
```
Handler in Module A
    ↓
_mediator.Send(new CommandFromModuleB.Interfaces)
    ↓
UnitOfWorkContext depth++ — sub-command defers commit to root
    ↓
Handler in Module B executes
    ↓
UnitOfWorkContext depth-- — root commits both atomically
```

## Cross-module read
```
Handler in Module A
    ↓
_mediator.Send(new QueryFromModuleB.Interfaces)
    ↓
Handler in Module B returns DTO
    ↓
Module A uses DTO — never touches Module B domain objects
```

## Cross-module event
```
Module A entity behavior method
    ↓
IntegrationEvent added to _domainEvents
    ↓
Outbox writes event → dispatcher publishes to MediatR
    ↓
INotificationHandler<TEvent> in Module B executes
    ↓
Module B applies side effect — idempotency guard required
```

## Cross-module JOIN read (App.Queries)
```
Query declared in {Module}.Interfaces
    ↓
Handler implemented in App.Queries — has direct DbContext access
    ↓
JOIN across module entity tables
    ↓
Returns flat DTO — never exposes domain objects
```

# Implementation

## {Module}.Interfaces
## Interfaces project — `{Module}.Interfaces`
All cross-module contracts declared in [[skills/dotnet/skill-graph/developing/Module/Interfaces csproj/module-interface.csproj.skill|{ModuleName}.Interfaces]]. Other modules reference only this project.

```
/{Module}.Interfaces
  /Commands    ← write intent
  /Queries     ← read intent
  /DTOs        ← response shapes
  /Events      ← integration event contracts
```

## Cross-module write — command dispatch
```csharp
// Order.Application — dispatches to User module
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private readonly IMediator _mediator;

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand command, CancellationToken ct)
    {
        // cross-module write — via MediatR, not direct call
        var result = await _mediator.Send(
            new ReserveStockCommand(command.ProductId, command.Quantity), ct);

        if (!result.IsSuccess) return Result.Error("Stock reservation failed");

        var order = Order.Create(command.Guid, command.ProductId);
        await _repository.AddAsync(order, ct);
        return Result.Created(new CreateOrderResult(order.Id));
    }
}
```

## Cross-module read — query dispatch
```csharp
// TimeLog.Application — reads from Task module
var taskResult = await _mediator.Send(new GetTaskQuery(command.TaskId), ct);
if (!taskResult.IsSuccess) return Result.NotFound();
// use taskResult.Value (DTO) — never Task domain entity
```

## Integration event — `{Module}.Interfaces/Events`
Contract declared in Interfaces so consuming modules can reference it.
```csharp
// Task.Interfaces/Events/TaskAssignedIntegrationEvent.cs
public record TaskAssignedIntegrationEvent(
    int TaskId,
    int AssigneeId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

## Event handler in consuming module
```csharp
// TimeLog.Application/EventHandlers/TaskAssignedEventHandler.cs
public class TaskAssignedEventHandler
    : INotificationHandler<TaskAssignedIntegrationEvent>
{
    public async Task Handle(
        TaskAssignedIntegrationEvent notification, CancellationToken ct)
    {
        var exists = await _repository.AnyAsync(
            new TimeEntryByEventIdSpec(notification.EventId), ct);
        if (exists) return;

        await _repository.AddAsync(
            new TimeEntry(notification.TaskId, notification.AssigneeId), ct);
    }
}
```

# Example
```
Scenario: Creating an order reserves stock and notifies shipping module

1. POST /order → CreateOrderCommand (Order.Interfaces)
2. CreateOrderHandler dispatches ReserveStockCommand (Stock.Interfaces)
   → Stock handler reserves stock atomically in same UnitOfWork
3. Order entity raises OrderCreatedIntegrationEvent
4. Outbox writes event in same transaction
5. OutboxDispatcher publishes OrderCreatedIntegrationEvent
6. ShippingModule.OrderCreatedHandler creates shipment record
```

# Rules
MUST:
- Cross-module writes via `_mediator.Send(command)` — never direct Application method call
- Cross-module reads via `_mediator.Send(query)` or App.Queries for JOINs
- Integration event contracts declared in `{SourceModule}.Interfaces/Events`
- Event handlers implement idempotency guard
- All cross-module contracts stable — breaking changes are versioned
MUST NOT:
- Module reference another module's Domain directly
- Module reference another module's Application directly
- Cross-module write via direct method call — breaks atomicity and boundary
- DTO expose domain entity internals — projection only
- Cross-module JOIN in Application — belongs in App.Queries

# Anti-patterns
- Direct call: `_taskApplicationService.CreateTask(...)` — use `_mediator.Send(new CreateTaskCommand(...))`
- Consuming module references `Task.Domain.Entities.Task` — use DTO from `Task.Interfaces`
- Cross-module JOIN in `{Module}.Application` — Application has no access to other module's DB tables
- Event handler without idempotency — duplicate side effects on retry

# Checklist
- [ ] No direct Application-to-Application calls
- [ ] No cross-module Domain references
- [ ] All cross-module writes via `_mediator.Send()`
- [ ] All cross-module reads via `_mediator.Send()` or App.Queries
- [ ] Integration events declared in `{Module}.Interfaces/Events`
- [ ] All event handlers have idempotency guard
- [ ] DTOs never expose domain entity internals

# Unittest TestCases
- [ ] When cross-module command dispatched Then both module changes committed atomically
- [ ] When cross-module command fails Then neither module's changes committed
- [ ] When integration event handled first time Then side effect applied
- [ ] When integration event handled twice Then side effect applied only once

# Relations
- command-handling.solution.skill.md — sub-command dispatch and UnitOfWorkContext depth
- domain-events.solution.skill.md — integration events travel via outbox
- module-layer.csproj.skill.md — Interfaces project is the only cross-module reference point
- event-handler.class.skill.md — idempotent handler in consuming module
