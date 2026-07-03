---
description: Implement timestamp interfaces based on entity classification
project_name: "{Module}.Domain"
name: "{EntityName}.cs"
element_kind: class
change_kind: extend
---

# Goals
- Add timestamp properties to the entity according to its classification.
- Keep class-level setters internal while satisfying mutable timestamp interfaces through explicit implementation.

# Core Principles
- Classification decides which interfaces are applied.
- Timestamp properties are infrastructure metadata and use `internal set`.
- Explicit interface implementation exposes a public setter for the interface without exposing a public setter on the class.

# Structure

## Project Structure
```
/{Module}.Domain
  /Entities
    {EntityName}.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity class | `{EntityName}` | `{EntityName}` | `{EntityName}.cs` | `{EntityName}.cs` |

# Implementation changes

## Internal Immutable

No timestamp fields are added.

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName}
{
    public int Id { get; internal set; }
    // domain properties only
}
```

## External Immutable

Implements `ICreationInfoModel` only.

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
using Shared.Timestamps;

public class {EntityName} : ICreationInfoModel
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }

    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime
    {
        get => ServerCreatedDateTime;
        set => ServerCreatedDateTime = value;
    }

    DateTimeOffset ICreationInfoModel.UserCreatedDateTime
    {
        get => UserCreatedDateTime;
        set => UserCreatedDateTime = value;
    }

    public static {EntityName} Create(Guid guid /*, ... */)
        => new()
        {
            Guid = guid,
            // ...
        };
}
```

## Internal Mutable

Implements `ICreationInfoModel` and `IUpdateInfoModel`; no `Guid`.

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
using Shared.Timestamps;

public class {EntityName} : ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public uint Version { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }

    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime
    {
        get => ServerCreatedDateTime;
        set => ServerCreatedDateTime = value;
    }

    DateTimeOffset ICreationInfoModel.UserCreatedDateTime
    {
        get => UserCreatedDateTime;
        set => UserCreatedDateTime = value;
    }

    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

    DateTimeOffset IUpdateInfoModel.ServerUpdatedDateTime
    {
        get => ServerUpdatedDateTime;
        set => ServerUpdatedDateTime = value;
    }

    DateTimeOffset IUpdateInfoModel.UserUpdatedDateTime
    {
        get => UserUpdatedDateTime;
        set => UserUpdatedDateTime = value;
    }
}
```

## External Mutable

Implements `ICreationInfoModel` and `IUpdateInfoModel`; has both `Guid` and `Version`.

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
using Shared.Timestamps;

public class {EntityName} : ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public uint Version { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }

    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime
    {
        get => ServerCreatedDateTime;
        set => ServerCreatedDateTime = value;
    }

    DateTimeOffset ICreationInfoModel.UserCreatedDateTime
    {
        get => UserCreatedDateTime;
        set => UserCreatedDateTime = value;
    }

    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

    DateTimeOffset IUpdateInfoModel.ServerUpdatedDateTime
    {
        get => ServerUpdatedDateTime;
        set => ServerUpdatedDateTime = value;
    }

    DateTimeOffset IUpdateInfoModel.UserUpdatedDateTime
    {
        get => UserUpdatedDateTime;
        set => UserUpdatedDateTime = value;
    }

    public static {EntityName} Create(Guid guid /*, ... */)
        => new()
        {
            Guid = guid,
            // ...
        };
}
```

# Rule changes

## MUST
- Apply timestamp interfaces based on classification.
- Use `DateTimeOffset` for all timestamp properties.
- Keep class-level timestamp setters `internal`.
- Implement mutable interface setters explicitly.

## MUST NOT
- Add timestamp fields to `Internal Immutable` entities.
- Add update timestamp fields to `External Immutable` entities.
- Expose public setters on class-level timestamp properties.

# Anti-patterns
- Adding all four timestamp fields to every entity.
- Implementing mutable interfaces implicitly with public setters.
- Forgetting explicit interface implementation and breaking compilation when setters are `internal`.

# Check list
- [ ] Classification determines applied timestamp interfaces.
- [ ] All timestamp properties are `DateTimeOffset`.
- [ ] Class-level timestamp setters are `internal`.
- [ ] Mutable interfaces are implemented explicitly.

# Unittest TestCases
- [ ] WHEN entity is `Internal Immutable` THEN it has no timestamp interfaces.
- [ ] WHEN entity is `External Immutable` THEN it implements `ICreationInfoModel` only.
- [ ] WHEN entity is `Internal Mutable` THEN it implements `ICreationInfoModel` and `IUpdateInfoModel`.
- [ ] WHEN entity is `External Mutable` THEN it implements both interfaces and keeps `Guid`.
- [ ] WHEN assigned through mutable interface THEN class-level property is updated.
- [ ] WHEN inspected from outside domain THEN class-level timestamp setter is not public.
