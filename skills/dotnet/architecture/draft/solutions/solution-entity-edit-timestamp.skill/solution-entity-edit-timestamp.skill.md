---
name: solution-entity-edit-timestamp
description: Adds server/user creation and update timestamp fields to user-initiated entities. Commands carry ActionTimeStamp, validators reject timestamps in the future, handlers assign user timestamps, and AppDbContext assigns server timestamps before SaveChanges.
domain: skill
type: architecture
version: 20260629
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - application
  - infrastructure
  - timestamp
  - audit
  - framework/mediatr
  - framework/ef-core
  - concern/architecture
  - solution/entity-edit-timestamp

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
  - "[[skills/dotnet/architecture/draft/solutions/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/draft/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/draft/solutions/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]"
  - "[[skills/dotnet/architecture/draft/solutions/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
built_on_plateau: "[[skills/dotnet/architecture/draft/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]]"
adr:
  - "[[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/adr/timestamp-handling|Timestamp handling ADR]]"
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

- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/adr/timestamp-handling|Timestamp handling ADR]]
  - Validator checks `ActionTimeStamp`; handler assigns user timestamps; `AppDbContext` assigns server timestamps.
  - Mutable entities implement both creation and update interfaces; external immutable entities implement creation only; internal immutable entities implement neither.
  - Timestamp command marker lives in `Shared.Timestamps` as `ICommandWithTimestamp`.

# Requirements

SOLUTION:
- [[skills/dotnet/architecture/draft/solutions/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]
  - [[skills/dotnet/architecture/draft/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - provides the EF Core configuration pattern for timestamp columns.
- [[skills/dotnet/architecture/draft/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[skills/dotnet/architecture/draft/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IRepository<T>` and `IReadRepository<T>` used by handlers.
  - [[skills/dotnet/architecture/draft/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create|AppDbContext.cs]] - hosts `AppDbContext`, extended here to assign server timestamps before saving.
- [[skills/dotnet/architecture/draft/solutions/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - determines whether an entity receives creation-only or creation+update timestamps.
- [[skills/dotnet/architecture/draft/solutions/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]
  - [[skills/dotnet/architecture/draft/solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]] - delegates to `AppDbContext.SaveChangesAsync`, which triggers `OnBeforeSaving`.

NUGET:
- No new packages are required for this solution. Existing packages from dependency solutions are sufficient:
  - `Microsoft.EntityFrameworkCore` - used in `AppDbContext.OnBeforeSaving`.
  - `FluentValidation` - used in command validators.
  - `Ardalis.Result` and `MediatR` - used in commands and handlers.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add timestamp contracts in `/Timestamps`
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create|ICreationInfoModelReadOnly.cs]] - create - Read-only creation timestamp contract.
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]] - create - Mutable creation timestamp contract implemented by entities.
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]] - create - Read-only update timestamp contract.
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]] - create - Mutable update timestamp contract implemented by entities.
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]] - create - Command marker that carries `ActionTimeStamp`.
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - extend - Add timestamp properties to classified entities and map them in EF configuration
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]] - extend - Implement `ICreationInfoModel` (and `IUpdateInfoModel` for mutable entities).
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend|{EntityName}Config.cs]] - extend - Configure `DateTimeOffset` timestamp columns as required.
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - extend - Commands for timestamped entities implement `ICommandWithTimestamp`
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend|{Command}.cs]] - extend - Add `ActionTimeStamp` property and `ICommandWithTimestamp` implementation.
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - extend - Handlers assign user timestamps; validators reject invalid `ActionTimeStamp`
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend|{FeatureName}.Handler.cs]] - extend - Assign `UserCreatedDateTime`/`UserUpdatedDateTime` from `ActionTimeStamp`.
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]] - extend - Ensure `ActionTimeStamp` is not default and not in the future.
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - extend - Override `SaveChanges`/`SaveChangesAsync` to set server timestamps
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]] - extend - Add `OnBeforeSaving` and set `ServerCreatedDateTime`/`ServerUpdatedDateTime`.

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

## MUST:
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend#MUST|AppDbContext.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create#MUST|ICommandWithTimestamp.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create#MUST|ICreationInfoModel.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create#MUST|ICreationInfoModelReadOnly.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create#MUST|IUpdateInfoModel.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create#MUST|IUpdateInfoModelReadOnly.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend#MUST|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend#MUST|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST|{EntityName}.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend#MUST|{EntityName}Config.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend#MUST|{Command}.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend#MUST NOT|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend#MUST NOT|AppDbContext.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend#MUST NOT|Shared.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create#MUST NOT|ICommandWithTimestamp.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create#MUST NOT|ICreationInfoModel.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create#MUST NOT|ICreationInfoModelReadOnly.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create#MUST NOT|IUpdateInfoModel.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create#MUST NOT|IUpdateInfoModelReadOnly.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend#MUST NOT|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend#MUST NOT|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend#MUST NOT|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend#MUST NOT|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend#MUST NOT|{EntityName}.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend#MUST NOT|{EntityName}Config.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST NOT|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend#MUST NOT|{Command}.cs]]

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
