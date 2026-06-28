---
name: class-entity
description: Extend entity to use Value Objects for properties with invariants and domain rules inside behavior methods
domain: skill
type: template
version: 20260622
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification.skill]]"
---

# Goal
- Encapsulate invariant state on Entity properties into dedicated Value Object types
- Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- Keep entity validation logic DRY by delegating to reusable domain rules instead of inline conditions
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers
- Add `Guid` as a required immutable property on External Immutable and External Mutable entity types
- Keep `Guid` strictly as a correlation handle — never used in domain logic, domain events, or relationships
- Add `Version` as a required property on all mutable entities
- Make every mutable entity implement `IVersioned` so the concurrency infrastructure can discover and read versions without reflection
- Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class
- Keep entity validation logic DRY by delegating to reusable domain rules
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity as the gatekeeper of state

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Core Principals
- Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- Equality of value properties on Entities is evaluated by Value Object structural equality
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods call domain rules to validate state transitions before applying changes
- Rule returns `bool` — entity decides whether to throw `DomainException` or reject the change
- Multiple related conditions are composed from individual rules — not reimplemented inline
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
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
- Rule returns `bool` — entity decides whether to throw `DomainException`
- Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity class | {EntityName} | Order | {EntityName}.cs | Order.cs |
| Entity | {EntityName} | Order | {EntityName}.cs | Order.cs |
| Correlation Guid | `Guid` | `Guid` | `Guid` |  |
| Concurrency token | `Version` | `Version` | `uint` |  |
| Entity behavior method | {Verb}{Noun} or {Verb} | UpdateComment | {EntityName}.cs | Order.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Implementation
Entity must use Value Object types for properties that have invariant state or business semantics:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
}
```

Entity behavior methods must use domain rules to guard state changes:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (!comment.IsNotEmpty())
            throw new DomainException("Comment must not be empty.");

        if (!comment.IsMaxLength(500))
            throw new DomainException("Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity can compose multiple rules for complex invariants:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!(Age, Country).IsSatisfied())
            throw new DomainException("Driver does not meet licensing requirements for this country.");

        // ... assign license
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
    private string _code;
    public string Code {
	    public get => this._code;
      public set {
		    if (value == "")
		        throw new DomainException("Invalid code");
		    this._code = value;
		  }  
		}
    
		public int Amount {get; internal set;}   
    internal void SetAmount(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Invalid amount");

        this.Amount = amount;
    }

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

Entity behavior methods must validate state through domain rules before mutating:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (!comment.IsNotEmpty())
            throw new DomainException("Comment must not be empty.");

        if (!comment.IsMaxLength(500))
            throw new DomainException("Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity can compose contextual rules for complex invariants:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!(Age, Country).IsSatisfied())
            throw new DomainException("Driver does not meet licensing requirements for this country.");

        // ... assign license
    }
}
```

When behavior becomes too large for the entity, delegate to a Domain Service Extension defined in `{Behavior}Service.cs` (this solution).

Entity may expose guarded internal methods for use by domain service extensions:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (!total.IsPositive())
            throw new DomainException("Total must be positive.");

        Total = total;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Entity Classification

Select the entity classification before implementation and apply only the properties and marker interfaces required by that classification.

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Rules
MUST:
	- Use Value Object on Entity property when the value has invariant state or carries business semantics
	- Configure multi-property Value Objects with `OwnsOne` in the entity's EF configuration
	- Call domain rules inside entity methods before mutating state
	- Throw `DomainException` when a rule returns `false` — the entity enforces, the rule only predicates
	- Use the most specific rule available (primitive, VO, or contextual) for the condition being checked
	- Entity has `int Id` with `internal set`
	- All public property setters or methods must validation state
	- `Id` used in all domain logic, persistence, relationships, and internal APIs
	- `Guid` declared as `public Guid Guid { get; internal set; }`
	- Set exactly once in the entity factory method — never reassigned
	- Present on External Immutable and External Mutable entity types only
	- All mutable entities have `public uint Version { get; internal set; }`
	- All mutable entities implement `IVersioned`
	- Throw `DomainException` when a rule returns `false`
	- Use the most specific rule available (primitive, VO, or contextual)
	- Keep the entity as the single gatekeeper for each property mutation
MUST NOT:
	- Use primitive type on Entity property when the value carries business meaning or invariant constraints
	- Reimplement rule logic inline inside entity methods — always delegate to existing rules
	- Mutate state before validating with rules
	- Allow invalid state to persist silently
	- Use `public` setters on any entity property
	- `Guid` used in domain logic, domain events, or as a foreign key in relationships
	- `Guid` reassigned after entity creation
	- Internal entity types (no external creation) have `Guid`
	- Immutable entities have `Version` — they are never updated
	- Application code assign `Version` — it is controlled exclusively by the database
	- Entity have any EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`)
	- Reimplement rule logic inline inside entity methods
	- Let a service extension expose a second public way to change a property that is already changed by an entity method

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Anti-patterns
- `public string Title { get; set; }` — public setter without validation
- Placing entity in Application or Interfaces project — entities belong in Domain only
- `Guid` with `public set` — application code must never modify it
- `Version` with `public set` — application code must never modify it
- Reading `Version` via reflection instead of `IVersioned` in `ConcurrencyBehavior`
- `[Column("task_title")]` on entity property — column mapping belongs in config class
- `[Table("TodoTasks")]` on entity class — table naming belongs in config class
- `[Index]` on entity class — index configuration belongs in config class
- `[ForeignKey]` on entity property — relation configuration belongs in config class

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] All public property setters and methods has validation
- [ ] Entity placed in /{Module}.Domain/Entities
- [ ] `Guid Guid { get; internal set; }` present on external-created entity
- [ ] `Guid` set in factory method
- [ ] Immutable after creation
- [ ] `uint Version { get; internal set; }` present on mutable entity
- [ ] Mutable entity implements `IVersioned`
- [ ] Immutable entities do not have `Version`
- [ ] No EF attributes present on entity class or any of its properties

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate invariant state on Entity properties into dedicated Value Object types
- [ ] WHEN applied THEN Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- [ ] WHEN applied THEN Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- [ ] WHEN applied THEN Keep entity validation logic DRY by delegating to reusable domain rules instead of inline conditions
- [ ] WHEN applied THEN Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- [ ] WHEN applied THEN Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- [ ] WHEN applied THEN Equality of value properties on Entities is evaluated by Value Object structural equality
- [ ] WHEN applied THEN Entity defines consistency — it decides when and how to enforce invariants
- [ ] WHEN applied THEN Entity methods call domain rules to validate state transitions before applying changes
- [ ] WHEN applied THEN Rule returns bool — entity decides whether to throw DomainException or reject the change
- [ ] WHEN applied THEN Multiple related conditions are composed from individual rules — not reimplemented inline
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
- [ ] WHEN applied THEN Keep entity validation logic DRY by delegating to reusable domain rules
- [ ] WHEN applied THEN Extract bulky logic to {Module}.Domain/Services while keeping the entity as the gatekeeper of state
- [ ] WHEN applied THEN Rule returns bool — entity decides whether to throw DomainException
- [ ] WHEN applied THEN Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation
- [ ] WHEN naming 'Entity behavior method' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
