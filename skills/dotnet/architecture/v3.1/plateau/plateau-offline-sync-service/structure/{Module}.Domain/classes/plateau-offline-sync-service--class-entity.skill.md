---
name: plateau-offline-sync-service--class-entity
description: Class {Entity} in the plateau-offline-sync-service plateau — a domain entity with guarded state transitions, a concurrency version, and creation/update timestamps
whenToUse: when creating or editing an entity in {Module}.Domain/Entities, adding a guarded state-change method, or adding the version / timestamp contributions
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../../solutions/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]"
---

# Goal
- Model one domain concept as the single point of truth for its own state validity: an `Id`, guarded state-change methods, and (per the entity's classification) a concurrency version and creation/update timestamps.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- No public setter for guarded state — mutation only through named methods (or `internal set` written only by those methods). A `private` parameterless constructor exists for EF materialization.
- Every mutating method validates first, via a locally-owned condition (inline or a `private static` helper), and throws `Shared.Exceptions.DomainException` on failure — invalid state is unreachable.
- Property types are strict `{ValueObject}` where the value carries a domain invariant (VP3), otherwise `Soft{ValueObject}` or primitives.
- **Classification (documented next to the entity):** Internal/External × Immutable/Mutable decides which of VP5/VP6 apply — no partial application, no concurrency on an immutable entity, no `Guid` on an internal one.
- **VP5 contribution (bounded):** implement `Shared.Concurrency.IVersioned` + `public uint Version { get; internal set; }`. Application code never assigns `Version` — the database owns it.
- **VP6 contribution (bounded):** implement `Shared.Guid.IHasGuid` + `public Guid Guid { get; internal set; }`, set once via the `Create(guid, ...)` factory parameter, never reassigned. A correlation handle only — never a foreign key, never a route parameter after creation.
- **VP7 contribution (bounded):** implement `ICreationInfoModel` (and `IUpdateInfoModel` if the user edits the entity); add the timestamp properties with `internal set` and explicit interface setters; add a `SetTimestamps`-style method this solution owns. Server times are set by `AppDbContext`; the handler copies `ActionTimeStamp` to the user times.
- Bulky behavior moves to a `static` domain service in `/Services` that calls back into the entity's guarded methods.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-entity
// Plateau: domain-service
// Version: 20260902000000
using Shared.Concurrency;
using Shared.Exceptions;
using Shared.Timestamps;
using {Module}.Domain.ValueObjects;

namespace {Module}.Domain.Entities;

public class {Entity} : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public {ValueObject} Title { get; internal set; } = null!;
    public uint Version { get; internal set; }                       // VP5 — database-owned

    public DateTimeOffset ServerCreatedDateTime { get; internal set; } // VP7
    public DateTimeOffset UserCreatedDateTime { get; internal set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }
    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime { get => ServerCreatedDateTime; set => ServerCreatedDateTime = value; }
    DateTimeOffset ICreationInfoModel.UserCreatedDateTime { get => UserCreatedDateTime; set => UserCreatedDateTime = value; }
    DateTimeOffset IUpdateInfoModel.ServerUpdatedDateTime { get => ServerUpdatedDateTime; set => ServerUpdatedDateTime = value; }
    DateTimeOffset IUpdateInfoModel.UserUpdatedDateTime { get => UserUpdatedDateTime; set => UserUpdatedDateTime = value; }

    private {Entity}() { }                                            // EF

    public static {Entity} Create({ValueObject} title) => new() { Title = title };

    public void Rename({ValueObject} newTitle)
    {
        if (/* invariant violated */ false)
            throw new DomainException("{Module}.{Entity}.CannotRename", "…");
        Title = newTitle;
    }

    public void RecordCreatedByUser(DateTimeOffset at) { UserCreatedDateTime = at; UserUpdatedDateTime = at; }
    public void RecordUpdatedByUser(DateTimeOffset at) => UserUpdatedDateTime = at;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]

# Rules
MUST:
- Validate before mutating in every state-change method; throw `DomainException` (with a `{Module}.{Entity}.{Reason}` code) on an invariant violation.
- Keep guarded state free of public setters; one property has one coordinated mutation path.
- Carry zero EF attributes — all persistence mapping lives in `{Entity}Config`.
- Add `IVersioned` + `uint Version` only if the entity is mutable; never assign `Version` from application code.
- Add `ICreationInfoModel` / `IUpdateInfoModel` and the timestamp members only if the entity is user-initiated; never set server timestamps here.
- Never apply several plateau templates per class; never let a domain service add a second mutation path for a property.

# Check list
- [ ] `Id` + guarded methods; no public setter for guarded state; `private` parameterless ctor.
- [ ] Every mutation validates first and throws `DomainException` on violation.
- [ ] Strict `{ValueObject}` on invariant-bearing properties; no EF attributes.
- [ ] `IVersioned` + `uint Version` iff mutable; `ICreationInfoModel`/`IUpdateInfoModel` iff user-initiated.

# Unittest TestCases
- [ ] WHEN a state-change method's condition fails THEN `DomainException` with the expected code is thrown and state is unchanged.
- [ ] WHEN the entity is inspected THEN `Version` and timestamp members match its classification.
