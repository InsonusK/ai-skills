---
description: Notification (domain event) handler
project_name: "{Module}.Application"
name: "{EventName}.EventHandler.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/event-handler-cs
---

# Goals
- React to a published `INotificationEvent` — one handler per (event, reacting concern), assembly-scanned like request handlers.

# Structure

## Project Structure
```
/{Module}.Application
  /Events
    /{EventName}
      {EventName}.EventHandler.cs
```

A module subscribing to another module's event references only that module's `{Module}.Interfaces`.

# Implementation changes

```csharp
// {Module}.Application/Events/TaskClosed/TaskClosed.EventHandler.cs
using MediatR;
using {OtherModule}.Interfaces.Events;

namespace {Module}.Application.Events.TaskClosed;

public class TaskClosedEventHandler(ISender sender) : INotificationHandler<TaskClosed>
{
    public async Task Handle(TaskClosed e, CancellationToken ct)
        => await sender.Send(new ArchiveTaskArtifacts(e.TaskId), ct);
}
```

# Rule changes

## MUST
- Name the file `{EventName}.EventHandler.cs` / class `{EventName}EventHandler`, one per reacting concern.
  - Risk: several unrelated reactions in one handler make the failure of one silently affect the others.
  - Fix: one handler per (event, concern); MediatR runs them all.
- Keep an event handler idempotent — publishing may be retried.
  - Risk: a non-idempotent reaction double-applies on a retry.
  - Fix: guard on already-applied state, or make the effect naturally idempotent.
- Never throw to "reject" an event.
  - Risk: a throw from one handler aborts `Publish` and the remaining handlers, and there is nothing to reject — the fact already happened.
  - Fix: log and, if needed, raise a compensating Command.

# Check list
- [ ] Handler is `INotificationHandler<{Event}>` in `/Events/{EventName}`.
- [ ] Idempotent.
- [ ] Does not throw as control flow.
- [ ] Subscribes across modules only via `{Module}.Interfaces.Events`.
