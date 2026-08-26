---
name: plateau-v1--class-timestamp-contracts
description: Classes ICreationInfoModel/IUpdateInfoModel/ICommandWithTimestamp in the v1 plateau
whenToUse: when a user-initiated entity needs creation/update timestamps, or a command needs to carry the client-supplied action timestamp
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Distinguish the user-supplied action time from the server-authoritative commit time, and give commands a single `ActionTimeStamp` to carry it

# Core Principles
- The user may only supply `ActionTimeStamp`; the server alone decides `ServerCreatedDateTime`/`ServerUpdatedDateTime`
- Which interfaces an entity implements is decided by `solution-entity-classification`'s Timestamp Matrix — `Internal Immutable` implements neither

# Implementation
```csharp
//Skill: class-timestamp-contracts
//Plateau: v1
//Version: 20260825140000

public interface ICreationInfoModelReadOnly
{
    DateTimeOffset UserCreatedDateTime { get; }
    DateTimeOffset ServerCreatedDateTime { get; }
}

public interface ICreationInfoModel : ICreationInfoModelReadOnly
{
    void SetCreationInfo(DateTimeOffset userCreatedDateTime);
}

public interface IUpdateInfoModelReadOnly
{
    DateTimeOffset UserUpdatedDateTime { get; }
    DateTimeOffset ServerUpdatedDateTime { get; }
}

public interface IUpdateInfoModel : IUpdateInfoModelReadOnly
{
    void SetUpdateInfo(DateTimeOffset userUpdatedDateTime);
}

public interface ICommandWithTimestamp
{
    DateTimeOffset ActionTimeStamp { get; }
}
```

# Rules
MUST:
- All five live in `Shared/Timestamps`
- Server timestamp setters are never exposed publicly — only `AppDbContext.OnBeforeSaving` assigns them
MUST NOT:
- Mix a user timestamp setter with the server timestamp field on the same public method

# Check list
- [ ] All five interfaces defined in `Shared/Timestamps`
- [ ] No public setter exposes a server-assigned timestamp field directly

__Applied solutions:__
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
