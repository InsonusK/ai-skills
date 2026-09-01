---
description: Assign user timestamps from ActionTimeStamp in command handlers
project_name: "{Module}.Application"
name: "{FeatureName}.Handler.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/featurename-handler-cs
---

# Goals
- Set `UserCreatedDateTime` and/or `UserUpdatedDateTime` from the command's `ActionTimeStamp`.
- Keep the handler focused on orchestration.

# Core Principles
- Assignment is done through the mutable timestamp interface so entity class-level setters can remain `internal`.
- Server timestamps are never touched in the handler.

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /{FeatureName}
      {FeatureName}.Handler.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command handler | `{FeatureName}Handler` | `CreateTaskHandler` | `{FeatureName}.Handler.cs` | `CreateTask.Handler.cs` |

# Implementation changes

## Create handler — mutable entity

```csharp
// {Module}.Application/Features/Create{Entity}/Create{Entity}.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;
using Shared.Timestamps;
using {Module}.Domain.Entities;
using {Module}.Interfaces.Commands;

namespace {Module}.Application.Features.Create{Entity};

public class Create{Entity}Handler
    : IRequestHandler<Create{Entity}Command, Result<Create{Entity}Result>>
{
    private readonly IRepository<{EntityName}> _repository;

    public Create{Entity}Handler(IRepository<{EntityName}> repository)
        => _repository = repository;

    public async Task<Result<Create{Entity}Result>> Handle(
        Create{Entity}Command command, CancellationToken ct)
    {
        var entity = {EntityName}.Create(/* ... */);

        var timestamped = (ICreationInfoModel)entity;
        timestamped.UserCreatedDateTime = command.ActionTimeStamp;
        timestamped.UserUpdatedDateTime = command.ActionTimeStamp;

        await _repository.AddAsync(entity, ct);

        return Result.Created(new Create{Entity}Result(entity.Id));
    }
}
```

## Create handler — External Immutable entity

```csharp
// {Module}.Application/Features/Create{Entity}/Create{Entity}.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;
using Shared.Timestamps;
using {Module}.Domain.Entities;
using {Module}.Interfaces.Commands;

namespace {Module}.Application.Features.Create{Entity};

public class Create{Entity}Handler
    : IRequestHandler<Create{Entity}Command, Result<Create{Entity}Result>>
{
    private readonly IRepository<{EntityName}> _repository;

    public Create{Entity}Handler(IRepository<{EntityName}> repository)
        => _repository = repository;

    public async Task<Result<Create{Entity}Result>> Handle(
        Create{Entity}Command command, CancellationToken ct)
    {
        var entity = {EntityName}.Create(command.Guid /*, ... */);

        ((ICreationInfoModel)entity).UserCreatedDateTime = command.ActionTimeStamp;

        await _repository.AddAsync(entity, ct);

        return Result.Created(new Create{Entity}Result(entity.Id));
    }
}
```

## Update handler — mutable entity

```csharp
// {Module}.Application/Features/Update{Entity}/Update{Entity}.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;
using Shared.Timestamps;
using {Module}.Application.Specifications;
using {Module}.Domain.Entities;
using {Module}.Interfaces.Commands;

namespace {Module}.Application.Features.Update{Entity};

public class Update{Entity}Handler
    : IRequestHandler<Update{Entity}Command, Result>
{
    private readonly IRepository<{EntityName}> _repository;

    public Update{Entity}Handler(IRepository<{EntityName}> repository)
        => _repository = repository;

    public async Task<Result> Handle(
        Update{Entity}Command command, CancellationToken ct)
    {
        var entity = await _repository.FirstOrDefaultAsync(
            new {EntityName}ByIdSpec(command.{EntityName}Id), ct);

        if (entity is null)
            return Result.NotFound();

        entity.Update(/* ... */);

        ((IUpdateInfoModel)entity).UserUpdatedDateTime = command.ActionTimeStamp;

        return Result.Success();
    }
}
```

# Rule changes

## MUST
- Assign user timestamps after the domain call and before staging the entity.
- Use the mutable interface cast to assign values.
- Set both `UserCreatedDateTime` and `UserUpdatedDateTime` for mutable entities on creation.
- Set only `UserUpdatedDateTime` for updates.
- Set only `UserCreatedDateTime` for `External Immutable` entity creation.
- Never validate `ActionTimeStamp` in the handler.
- Never assign server timestamps.
- Never set user timestamps directly on the entity class if the setter is `internal`.

## SHOULD
- Avoid forgetting the interface cast and failing to compile because the setter is `internal`.
- Avoid setting `UserCreatedDateTime` on an update.
- Avoid setting `UserUpdatedDateTime` on an `External Immutable` create.

# Check list
- [ ] Mutable entity create handler sets both user timestamps.
- [ ] Update handler sets only user updated timestamp.
- [ ] External Immutable create handler sets only user created timestamp.
- [ ] Assignment uses `ICreationInfoModel` / `IUpdateInfoModel` cast.

# Unittest TestCases
- [ ] WHEN create mutable entity THEN `UserCreatedDateTime` and `UserUpdatedDateTime` equal `ActionTimeStamp`.
- [ ] WHEN update mutable entity THEN only `UserUpdatedDateTime` changes.
- [ ] WHEN create External Immutable entity THEN only `UserCreatedDateTime` is set.
- [ ] WHEN handler runs THEN no server timestamps are assigned.
