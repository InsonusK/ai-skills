---
name: plateau-offline-sync-service--class-timestamp-contracts
description: Classes ICreationInfoModel(ReadOnly) / IUpdateInfoModel(ReadOnly) / ICommandWithTimestamp in the plateau-offline-sync-service plateau — the Shared/Timestamps contracts for user vs server creation/update times
whenToUse: when creating or editing any Shared/Timestamps interface, or deciding which timestamp interfaces an entity or command should implement
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Declare the timestamp contracts in `Shared/Timestamps` so Domain, Application, Interfaces, and Infrastructure reference them without coupling to BuildingBlocks — separating the user-supplied action time from the server-authoritative commit time.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create.md|ICreationInfoModel.cs.create]]

# Core Principles
- Apply ONE plateau template per class (this covers a contract family: one file per interface in `Shared/Timestamps`).
- `ICreationInfoModelReadOnly` / `IUpdateInfoModelReadOnly` — read-only `ServerCreatedDateTime` / `UserCreatedDateTime` (and update pair).
- `ICreationInfoModel : ICreationInfoModelReadOnly` / `IUpdateInfoModel : IUpdateInfoModelReadOnly` — `new` the properties with setters; an entity implements the setter **explicitly** so its class-level setter can stay `internal`.
- `ICommandWithTimestamp` — `DateTimeOffset ActionTimeStamp { get; }`, on create/update commands for a timestamped entity; `ActionTimeStamp` at its slot in the fixed command property order.
- Entity classification: not user-initiated → none; user creates only → `ICreationInfoModel`; user creates + edits → both.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-timestamp-contracts
// Plateau: domain-service
// Version: 20260902000000
namespace Shared.Timestamps;

public interface ICreationInfoModelReadOnly { DateTimeOffset ServerCreatedDateTime { get; } DateTimeOffset UserCreatedDateTime { get; } }
public interface ICreationInfoModel : ICreationInfoModelReadOnly
{
    new DateTimeOffset ServerCreatedDateTime { get; set; }
    new DateTimeOffset UserCreatedDateTime { get; set; }
}
public interface IUpdateInfoModelReadOnly { DateTimeOffset ServerUpdatedDateTime { get; } DateTimeOffset UserUpdatedDateTime { get; } }
public interface IUpdateInfoModel : IUpdateInfoModelReadOnly
{
    new DateTimeOffset ServerUpdatedDateTime { get; set; }
    new DateTimeOffset UserUpdatedDateTime { get; set; }
}
public interface ICommandWithTimestamp { DateTimeOffset ActionTimeStamp { get; } }
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create.md|ICommandWithTimestamp.cs.create]]

# Rules
MUST:
- All in `Shared/Timestamps`, one file per interface; mutable interfaces `new` the read-only properties with setters.
- Type every timestamp `DateTimeOffset`; `ICommandWithTimestamp` has only `ActionTimeStamp`.
- Never implement a mutable timestamp interface with public entity setters; never add a behaviour method.
- Never put these in BuildingBlocks; never apply several plateau templates per class.

# Check list
- [ ] Five timestamp interfaces + `ICommandWithTimestamp` in `Shared/Timestamps`, one file each.
- [ ] Mutable interfaces `new` the read-only members with `set`.
- [ ] `ICommandWithTimestamp` has exactly `ActionTimeStamp`.

# Unittest TestCases
- [ ] WHEN an entity implements `ICreationInfoModel` explicitly THEN assignment through the interface works and the class setter stays `internal`.
