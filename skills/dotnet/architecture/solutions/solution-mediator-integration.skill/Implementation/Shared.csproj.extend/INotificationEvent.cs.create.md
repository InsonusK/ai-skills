---
description: Publish/subscribe marker interface for a domain fact
project_name: Shared
name: INotificationEvent.cs
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/inotificationevent-cs
---

# Goals
- Mark a MediatR `INotification` as a domain event — a fact that has already happened — published for zero or more handlers, in-process, with no response.

# Core Principles
- Interface only, extends MediatR's `INotification`; no members.
- A notification is named in the past tense (`TaskClosed`, `AssigneeChanged`) and carries only the data a subscriber needs — never an entity reference.
- Publishing is fire-and-observe within the same process and (by default) the same transaction — a handler that must not run in that transaction, or must survive a crash, needs the outbox (VP14), not a plain notification.
- A notification handler may dispatch Commands, but never returns a value and never blocks the publisher on its result.

# Structure

## Project Structure
```
/Shared
  /MediatR
    INotificationEvent.cs
```

# Implementation changes

```csharp
// Shared/MediatR/INotificationEvent.cs
using MediatR;

namespace Shared;

public interface INotificationEvent : INotification { }
```

Module event record:
```csharp
// {Module}.Interfaces/Events/TaskClosed.cs
namespace {Module}.Interfaces.Events;

public record TaskClosed(Guid TaskId, DateTimeOffset ClosedAt) : INotificationEvent;
```

# Rule changes

## MUST
- Name a notification record in the past tense and put it in `{Module}.Interfaces/Events`.
  - Risk: a present/imperative name (`CloseTask`) reads as a Command and invites a subscriber to treat it as one.
  - Fix: past tense; the publisher has already committed the fact.
- Carry only primitive/`Soft{ValueObject}` data on the record — never an entity or a `DbContext`-tracked object.
  - Risk: a subscriber in another module touching a passed entity crosses the bounded-context boundary and may act on stale tracked state.
  - Fix: copy the needed values onto the record.
- Never make the publisher depend on a subscriber's outcome.
  - Risk: turning `Publish` into a de-facto request couples the two and reintroduces the direct call the mechanism removes.
  - Fix: if the publisher needs a result, it is a Command, not a notification.

# Check list
- [ ] `INotificationEvent : INotification` in `Shared/MediatR`, no members.
- [ ] Event records are past-tense, in `{Module}.Interfaces/Events`, carrying only value data.
- [ ] No publisher branches on a handler's result.
