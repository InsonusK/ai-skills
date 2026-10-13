---
description: Notification (domain event) record declaration
project_name: "{Module}.Interfaces"
name: "{Event}.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/event-cs
---

# Goals
- Declare a domain fact as an immutable past-tense record implementing `INotificationEvent`, published for zero or more in-process subscribers.

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Events
    {Thing}{PastTenseVerb}.cs
```

# Implementation changes

```csharp
// {Module}.Interfaces/Events/TaskClosed.cs
using Shared;

namespace {Module}.Interfaces.Events;

public record TaskClosed(Guid TaskId, DateTimeOffset ClosedAt) : INotificationEvent;
```

Published from a handler after the fact is decided:
```csharp
await _publisher.Publish(new TaskClosed(task.Id, DateTimeOffset.UtcNow), ct);
```

# Rule changes

## MUST
- Name the record `{Thing}{PastTenseVerb}` and place it in `/{Module}.Interfaces/Events`.
  - Risk: an imperative name invites a subscriber to treat the event as a command it must fulfil.
  - Fix: past tense — the publisher has already acted.
- Carry only value data (primitives, `Soft{ValueObject}`), never an entity.
  - Risk: a cross-module subscriber touching a passed entity breaks the boundary and may see stale tracked state.
  - Fix: copy the values needed onto the record.

# Check list
- [ ] Record is past-tense, in `/Events`, implementing `INotificationEvent`.
- [ ] Carries only value data.
- [ ] Published via `IPublisher.Publish`, never `ISender.Send`.
