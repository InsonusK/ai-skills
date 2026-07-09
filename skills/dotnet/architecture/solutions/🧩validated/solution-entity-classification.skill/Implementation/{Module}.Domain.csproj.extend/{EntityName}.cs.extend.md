# {EntityName}.cs - extend

Extend the entity class to match the selected classification. Only the parts required by the classification are added.

## Internal Immutable

No changes beyond the base entity. The entity has only the internal `int Id` identity.

**Dependencies**: do not implement `solution-entity-concurrency-change.skill` or `solution-external-created-entity.skill`.

```csharp
public class {EntityName}
{
    public int Id { get; private set; }

    // domain properties and factory method
}
```

## External Immutable

Add the `Guid` property and set it exactly once in the factory method. The entity has no `Version` and does not implement `IVersioned`.

**Dependencies**: implement `solution-external-created-entity.skill`; do not implement `solution-entity-concurrency-change.skill`.

```csharp
public class {EntityName}
{
    public int Id { get; private set; }
    public Guid Guid { get; internal set; }

    public static {EntityName} Create(Guid guid, /* other params */)
    {
        return new {EntityName}
        {
            Guid = guid,
            // other initializers
        };
    }
}
```

## Internal Mutable

Add the `Version` property and implement `IVersioned`. The entity has no `Guid` and does not implement `IHasGuid`.

**Dependencies**: implement `solution-entity-concurrency-change.skill`; do not implement `solution-external-created-entity.skill`.

```csharp
public class {EntityName} : IVersioned
{
    public int Id { get; private set; }
    public uint Version { get; internal set; }

    // domain properties and mutable behavior
}
```

## External Mutable

Add both `Guid` and `Version`, implement `IVersioned`, and set the `Guid` once in the factory method.

**Dependencies**: implement both `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill`.

```csharp
public class {EntityName} : IVersioned
{
    public int Id { get; private set; }
    public Guid Guid { get; internal set; }
    public uint Version { get; internal set; }

    public static {EntityName} Create(Guid guid, /* other params */)
    {
        return new {EntityName}
        {
            Guid = guid,
            // other initializers
        };
    }

    // domain properties and mutable behavior
}
```
