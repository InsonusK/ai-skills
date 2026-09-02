---
name: plateau-core--class-event-handler
description: Class {EventName}EventHandler in the plateau-core plateau — a notification handler in {Module}.Application/Events, one per reacting concern
whenToUse: when creating or editing a notification handler in {Module}.Application/Events, or deciding how a module should react to another module's published fact
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

# Goal
- React to a published `INotificationEvent` — one handler per (event, reacting concern), assembly-scanned like request handlers.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{EventName}.EventHandler.cs.create.md|{EventName}.EventHandler.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Implements `INotificationHandler<{Event}>`; lives in `/Events/{EventName}/`.
- Idempotent — publishing may be retried.
- Never throws to "reject" an event — the fact already happened; log, or raise a compensating command.
- A module subscribing to another module's event references only that module's `{Module}.Interfaces`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Notification handler | `{EventName}EventHandler` | `TaskClosedEventHandler` | `{EventName}.EventHandler.cs` | `TaskClosed.EventHandler.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-event-handler
// Plateau: core
// Version: 20260902000000
using MediatR;
using {OtherModule}.Interfaces.Events;

namespace {Module}.Application.Events.TaskClosed;

public sealed class TaskClosedEventHandler(ISender sender) : INotificationHandler<TaskClosed>
{
    public Task Handle(TaskClosed e, CancellationToken ct)
        => sender.Send(new ArchiveTaskArtifactsCommand(e.TaskId), ct);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{EventName}.EventHandler.cs.create.md|{EventName}.EventHandler.cs.create]]

# Rules
MUST:
- Name it `{EventName}EventHandler`, place it in `/Events/{EventName}`, implement `INotificationHandler<{Event}>` — one per reacting concern.
- Keep the reaction idempotent.
- Never throw as control flow; never apply several plateau templates per class.
- Subscribe across modules only via `{Module}.Interfaces.Events`.

# Check list
- [ ] Handler is `INotificationHandler<{Event}>` in `/Events/{EventName}`.
- [ ] Idempotent; does not throw as control flow.
- [ ] Cross-module subscription references only `{Module}.Interfaces`.

# Unittest TestCases
- [ ] WHEN the event is handled twice with the same payload THEN the effect is applied once.
- [ ] WHEN the reaction cannot complete THEN the handler logs / compensates, it does not throw.
