---
name: class-entity
description: Extend entity to use Value Objects for properties with invariants and locally-owned validation conditions inside behavior methods
domain: skill
type: template
version: 20260629223200
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
---

# Goal
- Encapsulate invariant state on Entity properties into dedicated Value Object types
- Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- Enforce entity invariants and prevent invalid state by validating via a locally-owned condition inside entity behavior methods
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers
- Add `Guid` as a required immutable property on External Immutable and External Mutable entity types
- Keep `Guid` strictly as a correlation handle — never used in domain logic, domain events, or relationships
- Add `Version` as a required property on all mutable entities
- Make every mutable entity implement `IVersioned` so the concurrency infrastructure can discover and read versions without reflection
- Add creation and update timestamp properties to user-initiated entities based on classification, using `DateTimeOffset` and explicit interface implementation for mutable setters
- Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity as the gatekeeper of state

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Core Principles
- Apply ONE plateau template per class
- Entity properties are Value Object types except `Id`, `Version`, and unconstrained generic parameters; any additional validation rule forces the generic parameter to become a Value Object
- Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- Equality of value properties on Entities is evaluated by Value Object structural equality
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods validate via a condition they own — either inline or a `private static` helper on the same class — before applying changes
- Entity throws `DomainException` when the condition fails
- Multiple related conditions are composed via a `private static` helper for readability — not scattered across the method body
- A condition that depends on data owned by a related entity is enforced by calling that entity's own guarded method — never duplicated locally; when the required navigation was not loaded by the caller, the entity throws `EntityNotLoadedException` instead of `DomainException`, since the failure is a Handler defect, not invalid domain state
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- Unconstrained generic parameters are the only property exception; as soon as a validation rule applies, the property must be a Value Object
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- All public setters or method must validate to prevent invalid state
- `Guid` declared with `internal set` — set once during entity creation factory method, never changed
- Entity creation factory method receives `Guid` as a parameter — it is the caller's responsibility to supply the client-generated value
- No domain method ever reads `Guid` after creation — only the resolver and the entity factory use it
- `Version` is `uint` with `internal set` — never set by application code, only by database
- Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- Read by `ConcurrencyBehavior` via the entity loaded from the repository — never passed as a domain parameter
- `IVersioned` is defined in Shared and implemented in Domain — no dependency on BuildingBlocks or App.Infrastructure
- Entity has zero EF attributes — `[Column]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]` are all forbidden
- Entity does not know about its own table name, column names, or constraint names
- Timestamp properties are `DateTimeOffset` with `internal set`; mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` explicitly so interface setters remain public while class-level setters stay internal
- External Immutable entities implement `ICreationInfoModel` only; Internal/External Mutable implement both; Internal Immutable has no timestamp interfaces
- Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity class | {EntityName} | Order | {EntityName}.cs | Order.cs |
| Entity | {EntityName} | Order | {EntityName}.cs | Order.cs |
| Correlation Guid | `Guid` | `Guid` | `Guid` |  |
| Concurrency token | `Version` | `Version` | `uint` |  |
| Entity behavior method | {Verb}{Noun} or {Verb} | UpdateComment | {EntityName}.cs | Order.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-entity
//Plateau: default
//Version: 20260628
```

Entity must use Value Object types for properties that have invariant state or business semantics:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
}
```

Entity behavior methods validate locally before mutating:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("{ModuleName}.Order.CommentRequired", "Comment must not be empty.");

        if (comment.Length > 500)
            throw new DomainException("{ModuleName}.Order.CommentTooLong", "Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity composes several conditions for a complex invariant, using a `private static` helper for readability:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!MeetsLicensingRequirements(Age, Country))
            throw new DomainException("{ModuleName}.Driver.NotEligibleForLicense", "Driver does not meet licensing requirements for this country.");

        // ... assign license
    }

    private static bool MeetsLicensingRequirements(Age age, Country country) => country.Code switch
    {
        "US" => age.Value >= 16,
        "NL" => age.Value >= 18,
        _ => false
    };
}
```

When a condition needs data owned by a related entity — reached through a navigation the Handler must have loaded — the entity does not reimplement that data's invariant. It calls the related entity's own guarded method, which validates itself. If the navigation was not loaded, the entity throws `EntityNotLoadedException` (see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules.skill]]) instead of `DomainException` — the `Transaction` itself is valid and the request could be valid too; a missing navigation is a Handler defect, not invalid domain state:

```csharp
public class Transaction
{
    public int Id { get; internal set; }
    public Account Account { get; internal set; }
    public decimal Amount { get; internal set; }

    public void UpdateAmount(decimal newAmount)
    {
        if (Account is null)
            throw new EntityNotLoadedException(nameof(Transaction), nameof(Account));

        Account.Withdraw(newAmount - Amount); // Account validates its own balance invariant
        Amount = newAmount;
    }
}
```

Multi-property Value Objects persisted via EF Core `OwnsOne` must be configured in the entity's EF configuration (see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration.skill]]):

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsOne(o => o.Total);
        builder.OwnsOne(o => o.CustomerEmail);
    }
}
```

Entity must be a class with `int Id` as primary identity.

```csharp
public class Currency
{
    public int Id { get; internal set; }
    public CurrencyCode Code { get; internal set; }
    public PositiveAmount Amount { get; internal set; }
}
```

External-created entity must declare `Guid` with `internal set`:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName}
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }    // ← added by this solution
    // ... other properties
    public uint Version { get; internal set; }  // ← solution-entity-concurrency-change.skill (if mutable)

    // factory method receives client-generated Guid
    public static {EntityName} Create(Guid guid, /* ... */)
        => new()
        {
            Guid = guid,
            // ...
        };
}
```

Mutable entity must declare `Version` and implement `IVersioned`:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
using Shared.Concurrency;

public class {EntityName} : IVersioned
{
    public int Id { get; internal set; }
    // ... other properties
    public uint Version { get; internal set; }   // ← added by this solution

    internal void SomeDomainMethod() { ... }
}
```

> **Note:** The stable business name used by `EntityVersionResolverFactory`, `IHasVersions`, and `ETagEncoder` lives in `{EntityName}Config.VersionedEntityName` — not on the entity class. This keeps entity metadata centralized in the EF configuration.

Entity must not contain any EF attributes:
```csharp
// CORRECT — no EF attributes
public class TodoTask
{
    public int Id { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}

// WRONG — EF attributes on entity
[Table("TodoTasks")]
public class TodoTask
{
    [Key]
    public int Id { get; internal set; }
    [Column("task_title")]
    public string Title { get; internal set; }
}
```

When behavior becomes too large for the entity, delegate to a Domain Service Extension defined in `{Behavior}Service.cs` (this solution). The entity exposes a guarded internal method for the service to call:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (total < 0)
            throw new DomainException("{ModuleName}.Order.TotalMustBePositive", "Total must be positive.");

        Total = total;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Entity Classification

Select the entity classification before implementation and apply only the properties and marker interfaces required by that classification.

## Internal Immutable

No changes beyond the base entity. The entity has only the internal `int Id` identity.

**Dependencies**: do not implement `solution-entity-concurrency-change.skill`, `solution-external-created-entity.skill`, or `solution-entity-edit-timestamp.skill`.

```csharp
public class {EntityName}
{
    public int Id { get; internal set; }

    // domain properties and factory method
}
```

## External Immutable

Add the `Guid` property and creation timestamps. Set `Guid` exactly once in the factory method. The entity has no `Version`, no `IVersioned`, and no `IUpdateInfoModel`.

**Dependencies**: implement `solution-external-created-entity.skill` and `solution-entity-edit-timestamp.skill`; do not implement `solution-entity-concurrency-change.skill`.

```csharp
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

Add the `Version` property and implement `IVersioned`. Add creation and update timestamps. The entity has no `Guid` and does not implement `IHasGuid`.

**Dependencies**: implement `solution-entity-concurrency-change.skill` and `solution-entity-edit-timestamp.skill`; do not implement `solution-external-created-entity.skill`.

```csharp
public class {EntityName} : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public uint Version { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

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

    // domain properties and mutable behavior
}
```

## External Mutable

Add both `Guid` and `Version`, implement `IVersioned`, add creation and update timestamps, and set the `Guid` once in the factory method.

**Dependencies**: implement `solution-entity-concurrency-change.skill`, `solution-external-created-entity.skill`, and `solution-entity-edit-timestamp.skill`.

```csharp
public class {EntityName} : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public uint Version { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]

# Rules
MUST:
	- Use Value Object on Entity property when the value has invariant state or carries business semantics
	- Entity properties other than `Id` and `Version` must be Value Object types, unless they are unconstrained generic parameters
	- If a property has any validation rule beyond the generic type's contract, the generic type must be replaced with a Value Object
	- Configure multi-property Value Objects with `OwnsOne` in the entity's EF configuration
	- Validate via a locally-owned condition (inline or a `private static` helper) inside entity methods before mutating state
	- Throw `DomainException` when that condition fails
	- When a condition needs a related entity's own data, call that entity's guarded method instead of duplicating the condition
	- Throw `EntityNotLoadedException` (not `DomainException`) when a behavior method requires a navigation the caller did not load
	- Entity has `int Id` with `internal set`
	- All public property setters or methods must validation state
	- `Id` used in all domain logic, persistence, relationships, and internal APIs
	- `Guid` declared as `public Guid Guid { get; internal set; }`
	- Set exactly once in the entity factory method — never reassigned
	- Present on External Immutable and External Mutable entity types only
	- All mutable entities have `public uint Version { get; internal set; }`
	- All mutable entities implement `IVersioned`
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Keep the entity as the single gatekeeper for each property mutation
MUST NOT:
	- Use primitive type on Entity property when the value carries business meaning or invariant constraints
	- Expose a primitive Entity property when a Value Object could enforce the same invariants
	- Mutate state before validating
	- Allow invalid state to persist silently
	- Assume a related entity's navigation is loaded without checking, when the method needs that entity's data
	- Reimplement a related entity's own invariant instead of calling its guarded method
	- Use `public` setters on any entity property
	- `Guid` used in domain logic, domain events, or as a foreign key in relationships
	- `Guid` reassigned after entity creation
	- Internal entity types (no external creation) have `Guid`
	- Immutable entities have `Version` — they are never updated
	- Application code assign `Version` — it is controlled exclusively by the database
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties expose `public set`
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties expose `public set`
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties expose `public set`
	- Entity have any EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`)
	- Let a service extension expose a second public way to change a property that is already changed by an entity method
	- Let a service extension bypass entity methods and write directly to properties

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `public string Title { get; set; }` — public setter without validation
- Using a related entity's property without checking it was loaded, then getting a `NullReferenceException` instead of a clear `EntityNotLoadedException`
- Throwing `DomainException` when a required navigation is missing — that hides a Handler defect behind a client-facing validation error
- Copying a related entity's balance/state check into the calling entity instead of delegating to that entity's own guarded method
- Entity property that is not `Id`, `Version`, or an unconstrained generic is a primitive type
- Placing entity in Application or Interfaces project — entities belong in Domain only
- `Guid` with `public set` — application code must never modify it
- `Version` with `public set` — application code must never modify it
- Reading `Version` via reflection instead of `IVersioned` in `ConcurrencyBehavior`
- `[Column("task_title")]` on entity property — column mapping belongs in config class
- `[Table("TodoTasks")]` on entity class — table naming belongs in config class
- `[Index]` on entity class — index configuration belongs in config class
- `[ForeignKey]` on entity property — relation configuration belongs in config class
- Inconsistent timestamp interfaces across entity classifications
- Implementing timestamp interfaces implicitly with public setters
- Inconsistent timestamp interfaces across entity classifications
- Implementing timestamp interfaces implicitly with public setters
- Inconsistent timestamp interfaces across entity classifications
- Implementing timestamp interfaces implicitly with public setters

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] Entity uses Value Object for every property except `Id`, `Version`, and unconstrained generic parameters
- [ ] Generic properties have no additional validation rules; otherwise they are replaced with Value Objects
- [ ] All public property setters and methods has validation
- [ ] Entity methods requiring a related entity's data check the navigation is loaded and throw `EntityNotLoadedException` when it is not
- [ ] Entity placed in /{Module}.Domain/Entities
- [ ] `Guid Guid { get; internal set; }` present on external-created entity
- [ ] `Guid` set in factory method
- [ ] Immutable after creation
- [ ] `uint Version { get; internal set; }` present on mutable entity
- [ ] Mutable entity implements `IVersioned`
- [ ] Immutable entities do not have `Version`
- [ ] `Internal Immutable` entities have no timestamp interfaces
- [ ] `External Immutable` entities implement `ICreationInfoModel` only
- [ ] `Internal Mutable` / `External Mutable` entities implement `ICreationInfoModel` and `IUpdateInfoModel`
- [ ] Timestamp properties use `DateTimeOffset` with `internal set`
- [ ] Mutable interface setters implemented explicitly
- [ ] No EF attributes present on entity class or any of its properties

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate invariant state on Entity properties into dedicated Value Object types
- [ ] WHEN applied THEN Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- [ ] WHEN applied THEN Enforce entity invariants and prevent invalid state by validating via a locally-owned condition inside entity behavior methods
- [ ] WHEN applied THEN Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- [ ] WHEN applied THEN Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- [ ] WHEN applied THEN Equality of value properties on Entities is evaluated by Value Object structural equality
- [ ] WHEN applied THEN Entity defines consistency — it decides when and how to enforce invariants
- [ ] WHEN applied THEN Entity methods validate via a locally-owned condition before applying changes
- [ ] WHEN applied THEN DomainException is thrown when the condition fails
- [ ] WHEN applied THEN Multiple related conditions are composed via a private static helper — not scattered inline
- [ ] WHEN an entity method requires data from a related entity that was not loaded THEN it throws EntityNotLoadedException, not DomainException
- [ ] WHEN an entity method requires a related entity's data and the navigation is loaded THEN it delegates to that entity's own guarded method instead of duplicating the condition
- [ ] WHEN naming 'Entity class' THEN pattern matches convention
- [ ] When entity created Then Id is default (0) until persisted
- [ ] WHEN applied THEN Add Guid as a required immutable property on External Immutable and External Mutable entity types
- [ ] WHEN applied THEN Keep Guid strictly as a correlation handle — never used in domain logic, domain events, or relationships
- [ ] WHEN applied THEN Guid declared with internal set — set once during entity creation factory method, never changed
- [ ] WHEN applied THEN Entity creation factory method receives Guid as a parameter — it is the caller's responsibility to supply the client-generated value
- [ ] WHEN applied THEN No domain method ever reads Guid after creation — only the resolver and the entity factory use it
- [ ] WHEN verified THEN Guid Guid { get; internal set; } present on external-created entity
- [ ] WHEN verified THEN Guid set in factory method
- [ ] WHEN verified THEN Immutable after creation
- [ ] WHEN naming 'Correlation Guid' THEN pattern matches convention
- [ ] WHEN applied THEN Add Version as a required property on all mutable entities
- [ ] WHEN applied THEN Version is uint with internal set — never set by application code, only by database
- [ ] WHEN applied THEN Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- [ ] WHEN applied THEN Read by ConcurrencyBehavior via the entity loaded from the repository — never passed as a domain parameter
- [ ] WHEN applied THEN All mutable entities implement IVersioned
- [ ] WHEN verified THEN uint Version { get; internal set; } present on mutable entity
- [ ] WHEN verified THEN Mutable entity implements IVersioned
- [ ] WHEN verified THEN Immutable entities do not have Version
- [ ] WHEN naming 'Concurrency token' THEN pattern matches convention
- [ ] THEN it ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class
- [ ] WHEN applied THEN Entity has zero EF attributes — [Column], [Index], [ForeignKey], [ConcurrencyCheck] are all forbidden
- [ ] WHEN applied THEN Entity does not know about its own table name, column names, or constraint names
- [ ] WHEN verified THEN No EF attributes present on entity class or any of its properties
- [ ] WHEN naming 'Entity' THEN pattern matches convention
- [ ] WHEN applied THEN Extract bulky logic to {Module}.Domain/Services while keeping the entity as the gatekeeper of state
- [ ] WHEN applied THEN Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation
- [ ] WHEN naming 'Entity behavior method' THEN pattern matches convention
- [ ] WHEN entity is `Internal Immutable` THEN it has no timestamp interfaces
- [ ] WHEN entity is `External Immutable` THEN it implements `ICreationInfoModel` only
- [ ] WHEN entity is `Internal Mutable` THEN it implements `ICreationInfoModel` and `IUpdateInfoModel`
- [ ] WHEN entity is `External Mutable` THEN it implements both interfaces and keeps `Guid`
- [ ] WHEN assigned through mutable interface THEN class-level property is updated
- [ ] WHEN inspected from outside domain THEN class-level timestamp setter is not public
- [ ] WHEN entity is `Internal Immutable` THEN it has no timestamp interfaces
- [ ] WHEN entity is `External Immutable` THEN it implements `ICreationInfoModel` only
- [ ] WHEN entity is `Internal Mutable` THEN it implements `ICreationInfoModel` and `IUpdateInfoModel`
- [ ] WHEN entity is `External Mutable` THEN it implements both interfaces and keeps `Guid`
- [ ] WHEN assigned through mutable interface THEN class-level property is updated
- [ ] WHEN inspected from outside domain THEN class-level timestamp setter is not public
- [ ] WHEN entity is `Internal Immutable` THEN it has no timestamp interfaces
- [ ] WHEN entity is `External Immutable` THEN it implements `ICreationInfoModel` only
- [ ] WHEN entity is `Internal Mutable` THEN it implements `ICreationInfoModel` and `IUpdateInfoModel`
- [ ] WHEN entity is `External Mutable` THEN it implements both interfaces and keeps `Guid`
- [ ] WHEN assigned through mutable interface THEN class-level property is updated
- [ ] WHEN inspected from outside domain THEN class-level timestamp setter is not public

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create|EntityNotLoadedException.cs]]

