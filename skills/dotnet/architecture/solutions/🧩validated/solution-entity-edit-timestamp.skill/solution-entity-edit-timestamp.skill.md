---
name: solution-entity-edit-timestamp
description: Adds server/user creation and update timestamp fields to user-initiated entities. Commands carry ActionTimeStamp, validators reject timestamps in the future, handlers assign user timestamps, and AppDbContext assigns server timestamps before SaveChanges.
domain: skill
type: architecture
version: 20260629
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - timestamp
  - audit
  - mediatr
triggers:
  - add creation timestamp
  - add update timestamp
  - entity audit fields
  - ActionTimeStamp
  - ICommandWithTimestamp
  - server created time
  - user created time
creates:
  - Shared.Timestamps.ICreationInfoModelReadOnly.cs
  - Shared.Timestamps.ICreationInfoModel.cs
  - Shared.Timestamps.IUpdateInfoModelReadOnly.cs
  - Shared.Timestamps.IUpdateInfoModel.cs
  - Shared.Timestamps.ICommandWithTimestamp.cs
extends:
  - Shared.csproj
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Configurations.{EntityName}Config.cs"
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - App.Infrastructure.csproj
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
adr:
  - "[[./adr/timestamp-handling.md|Timestamp handling ADR]]"
---

# Goal

- Add `DateTimeOffset` timestamp fields to every entity that is created and/or updated by a user.
- Distinguish between the **user timestamp** (the moment the user invoked the command) and the **server timestamp** (the moment the persistence layer commits the change).
- Keep timestamp contracts in `Shared` so Domain, Application, Interfaces, and Infrastructure can reference them without coupling to BuildingBlocks.
- Extend the entity classification matrix with a third axis: **timestamp applicability**.
- Ensure that user timestamps are validated early, assigned in handlers, and never mixed with server-assigned timestamps.

# Capabilities

- Every user-initiated entity exposes when it was created and, when mutable, when it was last updated.
- Consumers can see both the user-supplied action time and the authoritative server commit time.
- Commands that affect timestamped entities carry a single `ActionTimeStamp`.
- Invalid future timestamps are rejected before the handler runs.
- Server timestamps are assigned automatically by `AppDbContext`, requiring no handler code.

# Core Principles

- User-initiated entities are those that have a `Guid` and/or `Version` according to `solution-entity-classification.skill`.
- `Internal Immutable` entities are never user-initiated in this sense and do not receive timestamp fields.
- `External Immutable` entities receive creation timestamps only — they are never updated.
- `Internal Mutable` and `External Mutable` entities receive both creation and update timestamps.
- The user may only supply `ActionTimeStamp`; the server alone decides `ServerCreatedDateTime` and `ServerUpdatedDateTime`.
- Timestamp contracts live in `Shared.Timestamps`.
- `ActionTimeStamp` validation is transport correctness and belongs in the command validator.
- User timestamp assignment belongs in the handler because the handler knows which entity is being created or updated.
- Server timestamp assignment belongs in `AppDbContext.OnBeforeSaving` because it is the single persistence boundary.
- All timestamp values use `DateTimeOffset` and server-side calculations use `DateTimeOffset.UtcNow`.

# Timestamp Matrix

| Entity Type | Ownership | Mutability | Timestamp Interfaces |
| --- | --- | --- | --- |
| **Internal Immutable** | Internal | Immutable | None |
| **External Immutable** | External | Immutable | `ICreationInfoModel` |
| **Internal Mutable** | Internal | Mutable | `ICreationInfoModel`, `IUpdateInfoModel` |
| **External Mutable** | External | Mutable | `ICreationInfoModel`, `IUpdateInfoModel` |

# Adr

- [[./adr/timestamp-handling.md|Timestamp handling ADR]]
  - Validator checks `ActionTimeStamp`; handler assigns user timestamps; `AppDbContext` assigns server timestamps.
  - Mutable entities implement both creation and update interfaces; external immutable entities implement creation only; internal immutable entities implement neither.
  - Timestamp command marker lives in `Shared.Timestamps` as `ICommandWithTimestamp`.

# Requirements

SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts timestamp marker interfaces.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts timestamped entities and their EF configuration.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]] - hosts commands that implement `ICommandWithTimestamp`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - hosts handlers that assign user timestamps and validators that check `ActionTimeStamp`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - hosts `AppDbContext` that assigns server timestamps before saving.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides the EF Core configuration pattern for timestamp columns.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IRepository<T>` and `IReadRepository<T>` used by handlers.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - commands implement `ICommand<T>` in addition to `ICommandWithTimestamp`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - handler structure is reused to assign user timestamps.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs]] - validators are reused to check `ActionTimeStamp`.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - determines whether an entity receives creation-only or creation+update timestamps.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs]] - delegates to `AppDbContext.SaveChangesAsync`, which triggers `OnBeforeSaving`.

NUGET:
- No new packages are required for this solution. Existing packages from dependency solutions are sufficient:
  - `Microsoft.EntityFrameworkCore` - used in `AppDbContext.OnBeforeSaving`.
  - `FluentValidation` - used in command validators.
  - `Ardalis.Result` and `MediatR` - used in commands and handlers.

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add timestamp contracts in `/Timestamps`
  - [[./Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs]] - create - Read-only creation timestamp contract.
  - [[./Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create.md|ICreationInfoModel.cs]] - create - Mutable creation timestamp contract implemented by entities.
  - [[./Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create.md|IUpdateInfoModelReadOnly.cs]] - create - Read-only update timestamp contract.
  - [[./Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create.md|IUpdateInfoModel.cs]] - create - Mutable update timestamp contract implemented by entities.
  - [[./Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create.md|ICommandWithTimestamp.cs]] - create - Command marker that carries `ActionTimeStamp`.
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add timestamp properties to classified entities and map them in EF configuration
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Implement `ICreationInfoModel` (and `IUpdateInfoModel` for mutable entities).
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Configure `DateTimeOffset` timestamp columns as required.
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Commands for timestamped entities implement `ICommandWithTimestamp`
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Add `ActionTimeStamp` property and `ICommandWithTimestamp` implementation.
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Handlers assign user timestamps; validators reject invalid `ActionTimeStamp`
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend.md|{FeatureName}.Handler.cs]] - extend - Assign `UserCreatedDateTime`/`UserUpdatedDateTime` from `ActionTimeStamp`.
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend.md|{FeatureName}.Validator.cs]] - extend - Ensure `ActionTimeStamp` is not default and not in the future.
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Override `SaveChanges`/`SaveChangesAsync` to set server timestamps
  - [[./Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md|AppDbContext.cs]] - extend - Add `OnBeforeSaving` and set `ServerCreatedDateTime`/`ServerUpdatedDateTime`.

# Workflow

```mermaid
sequenceDiagram
    participant Client
    participant Api as {Module}.Api
    participant Validator as {FeatureName}Validator
    participant Handler as {FeatureName}Handler
    participant Repository as IRepository<{Entity}>
    participant UoW as UnitOfWork
    participant DbContext as AppDbContext
    participant DB

    Client->>Api: POST /{entity} (ActionTimeStamp)
    Api->>Validator: Validate command
    alt ActionTimeStamp invalid
        Validator-->>Api: Validation failure (400)
        Api-->>Client: 400 Bad Request
    else ActionTimeStamp valid
        Api->>Handler: Send(command)
        Handler->>Handler: Set UserCreatedDateTime / UserUpdatedDateTime from ActionTimeStamp
        Handler->>Repository: AddAsync(entity)
        Handler-->>Api: Result.Success
        Api->>UoW: SaveChangesAsync
        UoW->>DbContext: SaveChangesAsync
        DbContext->>DbContext: OnBeforeSaving sets ServerCreatedDateTime / ServerUpdatedDateTime
        DbContext->>DB: INSERT / UPDATE
        DB-->>DbContext: rows affected
        DbContext-->>UoW: completed
        UoW-->>Api: completed
        Api-->>Client: 201 / 200 / 204
    end
```

# Rules

MUST:
- `ICreationInfoModelReadOnly`, `ICreationInfoModel`, `IUpdateInfoModelReadOnly`, `IUpdateInfoModel`, and `ICommandWithTimestamp` are defined in `Shared/Timestamps`.
- `Internal Mutable` and `External Mutable` entities implement both `ICreationInfoModel` and `IUpdateInfoModel`.
- `External Immutable` entities implement `ICreationInfoModel` only.
- `Internal Immutable` entities implement none of the timestamp interfaces.
- Timestamp properties on entities are `DateTimeOffset` with `internal set`.
- Commands that create or update a timestamped entity implement `ICommandWithTimestamp` alongside `ICommand<Result<T>>`.
- `ActionTimeStamp` is the first property on commands that implement `ICommandWithTimestamp`.
- Command validators check that `ActionTimeStamp` is not `default(DateTimeOffset)` and is not greater than `DateTimeOffset.UtcNow`.
- Create handlers for mutable entities set both `UserCreatedDateTime` and `UserUpdatedDateTime` to `ActionTimeStamp`.
- Update handlers set only `UserUpdatedDateTime` to `ActionTimeStamp`.
- Create handlers for `External Immutable` entities set only `UserCreatedDateTime` to `ActionTimeStamp`.
- `AppDbContext` overrides both `SaveChanges()` and `SaveChangesAsync(CancellationToken)` and calls `OnBeforeSaving()` before delegating to the base method.
- `OnBeforeSaving()` sets `ServerCreatedDateTime` for `Added` entries that implement `ICreationInfoModel`.
- `OnBeforeSaving()` sets `ServerUpdatedDateTime` for `Added` or `Modified` entries that implement `IUpdateInfoModel`.
- `OnBeforeSaving()` uses `DateTimeOffset.UtcNow` as the server time source.
- EF configuration maps timestamp properties as required `DateTimeOffset` columns.
- Handlers never assign server timestamps.
- `AppDbContext` never assigns user timestamps.

MUST NOT:
- Add timestamp fields to `Internal Immutable` entities.
- Add update timestamp fields to `External Immutable` entities.
- Validate `ActionTimeStamp` inside handlers.
- Set user timestamps in `AppDbContext`.
- Set server timestamps in handlers.
- Use `DateTime` instead of `DateTimeOffset` for timestamp fields.
- Allow `ActionTimeStamp` to be in the future.
- Use EF attributes on entities for timestamp mapping.

SHOULD:
- Keep timestamp interfaces and the command marker free of behavior logic.
- Name the command timestamp property `ActionTimeStamp` consistently.
- Place the timestamp command marker in `Shared.Timestamps` to keep it independent of MediatR internals.

# Anti-patterns
- Assigning both user and server timestamps in the same layer.
- Checking `ActionTimeStamp > DateTimeOffset.UtcNow` inside every handler instead of the validator.
- Using a pipeline behavior to assign user timestamps — it lacks the handler's context about which entity is the target of the command.
- Adding `ServerUpdatedDateTime` to an `External Immutable` entity that is never updated.
- Using `DateTime.Now` and losing time-zone information.
- Exposing public setters on entity timestamp properties.
- Mapping timestamp columns with `[Column]` or `[Required]` attributes on the entity class.

# Check list
- [ ] `Shared/Timestamps/ICreationInfoModelReadOnly.cs` exists.
- [ ] `Shared/Timestamps/ICreationInfoModel.cs` exists.
- [ ] `Shared/Timestamps/IUpdateInfoModelReadOnly.cs` exists.
- [ ] `Shared/Timestamps/IUpdateInfoModel.cs` exists.
- [ ] `Shared/Timestamps/ICommandWithTimestamp.cs` exists.
- [ ] Mutable entities implement both `ICreationInfoModel` and `IUpdateInfoModel`.
- [ ] `External Immutable` entities implement `ICreationInfoModel` only.
- [ ] `Internal Immutable` entities implement none of the timestamp interfaces.
- [ ] Create/update commands implement `ICommandWithTimestamp`.
- [ ] Validator rejects `default(DateTimeOffset)` and future `ActionTimeStamp`.
- [ ] Create handler for mutable entity sets `UserCreatedDateTime` and `UserUpdatedDateTime`.
- [ ] Update handler sets only `UserUpdatedDateTime`.
- [ ] Create handler for `External Immutable` sets only `UserCreatedDateTime`.
- [ ] `AppDbContext` overrides `SaveChanges` and `SaveChangesAsync`.
- [ ] `OnBeforeSaving` sets `ServerCreatedDateTime` for `Added` `ICreationInfoModel` entries.
- [ ] `OnBeforeSaving` sets `ServerUpdatedDateTime` for `Added`/`Modified` `IUpdateInfoModel` entries.
- [ ] EF configuration marks timestamp columns as required `DateTimeOffset`.

# Unittest TestCases
- [ ] When command `ActionTimeStamp` is in the future Then validator returns validation error and handler does not run.
- [ ] When command `ActionTimeStamp` is `default(DateTimeOffset)` Then validator returns validation error.
- [ ] When create handler runs for mutable entity Then `UserCreatedDateTime` and `UserUpdatedDateTime` equal command `ActionTimeStamp`.
- [ ] When update handler runs for mutable entity Then only `UserUpdatedDateTime` changes and equals command `ActionTimeStamp`.
- [ ] When create handler runs for `External Immutable` entity Then only `UserCreatedDateTime` is set.
- [ ] When `SaveChangesAsync` is called on a new mutable entity Then `ServerCreatedDateTime` and `ServerUpdatedDateTime` are set.
- [ ] When `SaveChangesAsync` is called on a modified mutable entity Then `ServerUpdatedDateTime` changes and `ServerCreatedDateTime` does not.
- [ ] When `SaveChanges` (sync) is called Then `OnBeforeSaving` runs and server timestamps are set.
- [ ] When `Internal Immutable` entity is saved Then no timestamp fields are populated.
- [ ] When `External Immutable` entity is saved Then only creation timestamps are populated.
