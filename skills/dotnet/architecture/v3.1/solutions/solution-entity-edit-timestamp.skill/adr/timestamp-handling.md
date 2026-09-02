---
name: timestamp-handling
description: Decide how user and server timestamps are validated, assigned, and mapped to entity classifications.
problem: Where should ActionTimeStamp be validated, where should user timestamps be assigned, where should server timestamps be assigned, which entities receive which timestamp fields, and where should the command marker live?
decision: Validator checks ActionTimeStamp; handlers assign user timestamps; AppDbContext assigns server timestamps; mutable entities get creation+update timestamps, external immutable get creation only, internal immutable get none; command marker lives in Shared.Timestamps as ICommandWithTimestamp.
tags:
  - solution/entity-edit-timestamp
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

`solution-entity-edit-timestamp` introduces both user-controlled and server-controlled timestamps. We needed to decide:

1. Where to validate that `ActionTimeStamp` is not in the future.
2. Where to assign user timestamps (`UserCreatedDateTime`, `UserUpdatedDateTime`).
3. Where to assign server timestamps (`ServerCreatedDateTime`, `ServerUpdatedDateTime`).
4. Which entity classifications receive creation timestamps, update timestamps, or neither.
5. Whether to provide read-only timestamp interfaces in addition to mutable ones.
6. Where the command marker interface should live and what it should be named.

# Selected variant

**Selected variant:** [[#Validator + Handler + DbContext split with classification-aware interfaces]]

- `ActionTimeStamp` validation is transport correctness, so it belongs in the command validator.
- User timestamp assignment belongs in the handler because the handler knows which entity is being created or updated.
- Server timestamp assignment belongs in `AppDbContext.OnBeforeSaving` because it is the single persistence boundary.
- Mutable entities (`Internal Mutable`, `External Mutable`) implement both `ICreationInfoModel` and `IUpdateInfoModel`.
- `External Immutable` entities implement `ICreationInfoModel` only.
- `Internal Immutable` entities implement neither.
- Provide read-only interfaces (`ICreationInfoModelReadOnly`, `IUpdateInfoModelReadOnly`) for read models and projections while entities implement the mutable variants.
- Entities keep class-level timestamp setters `internal` and implement the mutable interface setters explicitly, preserving the "no public setters on entities" rule.
- The command marker is `ICommandWithTimestamp` in `Shared.Timestamps`.

# Searched variants

## Validator + Handler + DbContext split with classification-aware interfaces (selected)

### Description

- Validator rejects `default(DateTimeOffset)` and future `ActionTimeStamp`.
- Handler assigns `UserCreatedDateTime` and/or `UserUpdatedDateTime` from `ActionTimeStamp`.
- `AppDbContext.OnBeforeSaving` assigns `ServerCreatedDateTime` and/or `ServerUpdatedDateTime`.
- Timestamp interfaces are split into read-only and mutable pairs.

### Benefits

- Clear separation of concerns: transport validation, orchestration, and persistence each own one aspect.
- Handlers keep full knowledge of which entity is being created or updated.
- Server timestamps are guaranteed regardless of how the entity reaches `DbContext`.
- Classification-aware interface set avoids misleading `UpdatedDateTime` fields on immutable entities.
- Read-only interfaces allow query/read models to expose timestamps without exposing setters.

### Costs

- Every create/update handler for timestamped entities must remember to assign user timestamps.
- More interfaces than a single-interface design.

## Handler-only assignment and validation

### Description

Both validation of `ActionTimeStamp` and assignment of user timestamps happen inside each handler.

### Benefits

- Fewer moving parts; no new validator rules.

### Costs

- Duplicates validation logic across handlers.
- Mixes transport correctness (timestamp range) with orchestration, violating the handler purity principle from `solution-command-integration.skill`.
- Easy to forget or implement inconsistently.

## Pipeline behavior assignment

### Description

A `TimestampBehavior` intercepts commands implementing `ICommandWithTimestamp` and updates all staged entities in the `ChangeTracker`.

### Benefits

- Completely removes timestamp logic from handlers.
- Centralized and consistent.

### Costs

- A behavior cannot reliably know which staged entity is the target of the command and whether it is a create or an update.
- Implicit magic that is hard to debug and test in isolation.
- Conflicts with the principle that handlers orchestrate entity changes.

## All timestamped entities get both creation and update fields

### Description

Every entity that receives timestamps gets all four fields regardless of mutability.

### Benefits

- Simpler rule: "has Guid or Version -> add all timestamp fields".

### Costs

- `External Immutable` entities would carry `UserUpdatedDateTime` and `ServerUpdatedDateTime` that are never meaningfully updated.
- Misleading data model and wasted columns.

## Single mutable interface without read-only variant

### Description

Only `ICreationInfoModel` and `IUpdateInfoModel` with getters and setters; no read-only interfaces.

### Benefits

- Fewer files and interfaces.

### Costs

- Read models and DTOs would expose setters they do not need.
- Less precise contracts for consumers that should only read timestamps.

## Public setters on entity timestamp properties

### Description

Expose `public set` on entity timestamp properties so the interfaces can be implemented implicitly.

### Benefits

- Simpler implementation — no explicit interface implementation.
- Handlers can assign timestamps without a cast.

### Costs

- Violates the existing rule that entity properties must not have public setters.
- Allows any code with an entity reference to mutate timestamp metadata.

## Command marker in Shared.MediatR

### Description

Place `ICommandWithTimestamp` next to `ICommand` in `Shared/MediatR`.

### Benefits

- Visually grouped with other command markers.

### Costs

- Ties the timestamp contract to MediatR infrastructure; the marker is really about time, not about MediatR routing.
- `Shared.Timestamps` keeps the contract independent and reusable.
