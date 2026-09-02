---
name: plateau-core--class-i-notification-event
description: Class INotificationEvent in the plateau-core plateau — the domain-fact notification marker in Shared/MediatR
whenToUse: when creating or editing the INotificationEvent marker, or deciding whether an interaction is a notification (fire-and-forget fact) or a command (request to act)
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
- Give a module one marker for a domain fact that has already happened, published to zero or more in-process subscribers.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/INotificationEvent.cs.create.md|INotificationEvent.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Pure marker — no members. `INotificationEvent : INotification`.
- An event is a past-tense fact — the publisher has already acted; subscribers react, they cannot reject.
- Published via `IPublisher.Publish`, never `ISender.Send`.
- Lives in `Shared/MediatR`, `namespace Shared.MediatR`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Notification marker | `INotificationEvent` | `INotificationEvent` | `INotificationEvent.cs` | `INotificationEvent.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-i-notification-event
// Plateau: core
// Version: 20260902000000
using MediatR;

namespace Shared.MediatR;

public interface INotificationEvent : INotification { }
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/INotificationEvent.cs.create.md|INotificationEvent.cs.create]]

# Rules
MUST:
- Keep it member-free; extend `INotification`.
- Place it in `Shared/MediatR/INotificationEvent.cs`, `namespace Shared.MediatR`.
- Never apply several plateau templates per class.
- Never add a member or a base other than `INotification`.

# Check list
- [ ] `INotificationEvent : INotification`, empty, in `Shared/MediatR/INotificationEvent.cs`.

# Unittest TestCases
- [ ] WHEN the marker is inspected THEN it is in `Shared.MediatR` and declares no instance members.
