---
name: plateau-offline-sync-service--class-event
description: Class {Event} in the plateau-offline-sync-service plateau — a module's past-tense domain-fact record in {Module}.Interfaces/Events
whenToUse: when creating or editing a notification record in {Module}.Interfaces/Events, or deciding what data a published fact should carry
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
- Declare a domain fact as an immutable past-tense `record` implementing `INotificationEvent`, published for zero or more in-process subscribers.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Event}.cs.create.md|{Event}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Past tense — the publisher has already acted; subscribers react and cannot reject.
- Carries only value data (primitives, `Soft{ValueObject}`) — never an entity.
- Published via `IPublisher.Publish`, never `ISender.Send`.
- Cross-module subscribers reference only this module's `{Module}.Interfaces`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain fact | `{Thing}{PastTenseVerb}` | `TaskClosed` | `{Event}.cs` | `TaskClosed.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-event
// Plateau: core
// Version: 20260902000000
using Shared.MediatR;

namespace {Module}.Interfaces.Events;

public record TaskClosed(Guid TaskId, DateTimeOffset ClosedAt) : INotificationEvent;
```
Published from a handler after the fact is decided:
```csharp
await publisher.Publish(new TaskClosed(task.Id, DateTimeOffset.UtcNow), ct);
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Event}.cs.create.md|{Event}.cs.create]]

# Rules
MUST:
- Name it `{Thing}{PastTenseVerb}`, place it in `/{Module}.Interfaces/Events`, implement `INotificationEvent`.
- Carry only value data — copy the values needed onto the record, never pass an entity.
- Never apply several plateau templates per class.
- Never publish via `ISender.Send`; never use an imperative name.

# Check list
- [ ] Past-tense `record` in `/Events` implementing `INotificationEvent`.
- [ ] Carries only primitives / `Soft{ValueObject}`.

# Unittest TestCases
- [ ] WHEN the record is reflected THEN it is assignable to `INotificationEvent` and carries no entity-typed property.
