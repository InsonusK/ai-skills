---
name: class-entity-config
description: Configure unique index on Guid with named constant
domain: skill
type: template
version: 20260629223200
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Configure a unique database index on `Guid` as the DB-level idempotency guard
- Define the index name as a `public static string` constant for use in test assertions
- Configure `Version` as the EF concurrency token mapped to the PostgreSQL `xmin` system column
- Declare the stable business name (`VersionedEntityName`) used by the concurrency infrastructure
- Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
- Keep the domain entity free of EF attributes and infrastructure annotations
- Ensure index and constraint names are constants so they can be referenced in tests and error handling
- Define one EF Core configuration class per entity that owns all persistence concerns

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Core Principles
- Apply ONE plateau template per class
- Index name follows the convention: `UX_{TableName}_Guid`
- Unique index ensures concurrent requests that both pass the pipeline check are rejected at the DB level
- Constant name `UX_Guid` used in integration tests to assert the correct constraint name in `PostgresException`
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE` — EF raises `DbUpdateConcurrencyException` if zero rows affected
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code
- `VersionedEntityName` is the single stable business name used by `EntityVersionResolverFactory`, `IHasVersions`, and `ETagEncoder` — changing it is a breaking API change
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- `TableName`, index, and constraint names are `public const string` constants on the config class
- Domain entity must have zero EF attributes
- Configuration is the only place that knows about column names, table names, and constraints
- Multi-property Value Object properties require `OwnsOne` mapping here
- Cross-module foreign key configurations live in App.Infrastructure — not here

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Unique index name | `UX_Guid` | `UX_Guid` |  |  |
| EF entity configuration | `{EntityName}Config` | `{EntityName}Config` | `{EntityName}Config.cs` | `{EntityName}Config.cs` |
| Entity EF configuration | {Entity}Config | TodoTaskConfig | {Entity}.Config.cs | TodoTask.Config.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-entity-config
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public static string TableName = nameof({EntityName});
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // unique index — DB-level idempotency guard
        builder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);

        // Version concurrency token — solution-entity-concurrency-change.skill (if mutable)
        builder
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
```

Every mutable entity configuration must include the `Version` mapping and the `VersionedEntityName` constant:

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string TableName = nameof({EntityName});
    public const string VersionedEntityName = "{Entity}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // ... other configuration (indexes, relations)

        builder
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
```

{Entity}Config must implement `IEntityTypeConfiguration<{Entity}>`. `TableName`, index, and constraint names must be `public const string` constants. All mapping defined in `Configure` method.

Base shape:
```csharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public const string TableName = nameof(TodoTask);

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        // indexes, relations, value object mappings go here
    }
}
```

Index definition — names as constants:
```csharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public const string TableName = nameof(TodoTask);
    public const string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        entityBuilder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);
    }
}
```

Relation definition:
```csharp
entityBuilder
    .HasMany(e => e.SubTasks)
    .WithOne(e => e.Task)
    .HasForeignKey(e => e.TaskId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

Multi-property Value Object mapping via OwnsOne:
```csharp
entityBuilder.OwnsOne(e => e.Cash, money =>
{
    money.Property(m => m.Amount).HasColumnName("Cash_Amount");
    money.Property(m => m.Currency).HasColumnName("Cash_Currency").HasMaxLength(3);
});
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Entity Classification

Extend the entity configuration to match the selected classification. Only the mappings required by the classification are added.

## Internal Immutable

No changes beyond the standard Id mapping. Do not map timestamp columns.

**Dependencies**: do not implement `solution-entity-concurrency-change.skill` or `solution-external-created-entity.skill` or `solution-entity-edit-timestamp.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        // other immutable property mappings
    }
}
```

## External Immutable

Add the `Guid` mapping with a unique index. There is no `Version` concurrency token. Map creation timestamps only.

**Dependencies**: implement `solution-external-created-entity.skill` and `solution-entity-edit-timestamp.skill`; do not implement `solution-entity-concurrency-change.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string UX_Guid = "UX_{EntityName}_Guid";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();

        // other immutable property mappings
    }
}
```

## Internal Mutable

Add the `Version` concurrency token mapped to PostgreSQL `xmin`. There is no `Guid`. Map creation and update timestamps.

**Dependencies**: implement `solution-entity-concurrency-change.skill` and `solution-entity-edit-timestamp.skill`; do not implement `solution-external-created-entity.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string VersionedEntityName = "{EntityName}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();
        builder.Property(x => x.ServerUpdatedDateTime).IsRequired();
        builder.Property(x => x.UserUpdatedDateTime).IsRequired();

        // other property mappings
    }
}
```

## External Mutable

Add both the unique `Guid` index and the `Version` concurrency token mapped to `xmin`. Map creation and update timestamps.

**Dependencies**: implement `solution-entity-concurrency-change.skill`, `solution-external-created-entity.skill`, and `solution-entity-edit-timestamp.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string UX_Guid = "UX_{EntityName}_Guid";
    public const string VersionedEntityName = "{EntityName}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();
        builder.Property(x => x.ServerUpdatedDateTime).IsRequired();
        builder.Property(x => x.UserUpdatedDateTime).IsRequired();

        // other property mappings
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Rules
MUST:
	- `UX_Guid` defined as `public static string` on the config class
	- `HasDatabaseName(UX_Guid)` used — never inline string
	- `IsUnique()` on the `Guid` index
	- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
	- Every mutable entity configuration declares `public const string VersionedEntityName` with the stable business name
	- Every timestamp property that exists on the entity is mapped with `.IsRequired()`
	- `DateTimeOffset` used for all timestamp properties
	- `TableName` is `public const string`
	- One config class per entity
	- `TableName` defined as `public const string`
	- All index and constraint names defined as `public const string` constants on the config class
	- `OwnsOne` configured for every multi-property Value Object property on this entity
	- All relations configured with `HasMany`/`HasOne`, `HasForeignKey`, and `OnDelete`
	- Registered via `ApplyConfigurationsFromAssembly` — never manually
MUST NOT:
	- Use inline string for index name
	- `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL
	- Map timestamp columns on `Internal Immutable` entities
	- Map update timestamps on `External Immutable` entities
	- Map timestamp columns on `Internal Immutable` entities
	- Map update timestamps on `External Immutable` entities
	- Map timestamp columns on `Internal Immutable` entities
	- Map update timestamps on `External Immutable` entities
	- `VersionedEntityName` be derived from `TableName` or `nameof({EntityName})` — it is an explicit business contract
	- Use EF data annotations on the domain entity (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
	- Define table names, column names, or constraint names as inline strings
	- Use `static` instead of `const` for `TableName`, index, or constraint names
	- Put mapping logic in `DbContext.OnModelCreating` directly
	- Configure cross-module foreign keys here — those belong in App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Inline string for database index name — hard to reference in tests
- `Version` mapped to a regular column without `IsConcurrencyToken()` — loses database-level protection
- Putting `VersionedEntityName` on the entity class — spreads configuration across the domain instead of keeping it in the config
- Mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Single config class shared across multiple entities — one config per entity, no exceptions
- Cross-module FK configured in Domain config — belongs in App.Infrastructure
- Mapping timestamp columns inconsistently across entities
- Allowing nullable timestamp columns
- Using `DateTime` column types
- Mapping timestamp columns inconsistently across entities
- Allowing nullable timestamp columns
- Using `DateTime` column types
- Mapping timestamp columns inconsistently across entities
- Allowing nullable timestamp columns
- Using `DateTime` column types

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Check list
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`
- [ ] `Version` mapped to `xmin`
- [ ] `.IsConcurrencyToken()` called
- [ ] `.ValueGeneratedOnAddOrUpdate()` called
- [ ] `VersionedEntityName` declared as `public const string`
- [ ] `TableName` declared as `public const string`
- [ ] One config class exists per entity
- [ ] Config class is in /{Module}.Domain/Configurations
- [ ] `TableName` defined as `public const string`
- [ ] All index and constraint names defined as `public const string`
- [ ] All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured with `HasForeignKey` and `OnDelete`
- [ ] `OwnsOne` configured for every multi-property VO property
- [ ] No EF attributes on the domain entity class
- [ ] `Internal Immutable` config does not map timestamp columns
- [ ] `External Immutable` config maps creation timestamps only
- [ ] Mutable entity config maps creation and update timestamps
- [ ] Mapped timestamp properties are `.IsRequired()`
- [ ] Timestamp properties use `DateTimeOffset`
- [ ] `Internal Immutable` config does not map timestamp columns
- [ ] `External Immutable` config maps creation timestamps only
- [ ] Mutable entity config maps creation and update timestamps
- [ ] Mapped timestamp properties are `.IsRequired()`
- [ ] Timestamp properties use `DateTimeOffset`
- [ ] `Internal Immutable` config does not map timestamp columns
- [ ] `External Immutable` config maps creation timestamps only
- [ ] Mutable entity config maps creation and update timestamps
- [ ] Mapped timestamp properties are `.IsRequired()`
- [ ] Timestamp properties use `DateTimeOffset`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

# Unittest TestCases
- [ ] WHEN applied THEN Configure a unique database index on Guid as the DB-level idempotency guard
- [ ] WHEN applied THEN Define the index name as a public static string constant for use in test assertions
- [ ] WHEN applied THEN Index name follows the convention: UX_{TableName}_Guid
- [ ] WHEN applied THEN Unique index ensures concurrent requests that both pass the pipeline check are rejected at the DB level
- [ ] WHEN applied THEN Constant name UX_Guid used in integration tests to assert the correct constraint name in PostgresException
- [ ] WHEN verified THEN UX_Guid constant defined on entity configuration class
- [ ] WHEN verified THEN Unique index on Guid configured with HasDatabaseName(UX_Guid) and IsUnique()
- [ ] WHEN naming 'Unique index name' THEN pattern matches convention
- [ ] WHEN applied THEN Configure Version as the EF concurrency token mapped to the PostgreSQL xmin system column
- [ ] WHEN applied THEN xmin is a PostgreSQL system column — automatically incremented on every row update
- [ ] WHEN applied THEN IsConcurrencyToken() tells EF to include Version in WHERE clause on UPDATE — EF raises DbUpdateConcurrencyException if zero rows affected
- [ ] WHEN applied THEN ValueGeneratedOnAddOrUpdate() tells EF the value comes from the database — never from application code
- [ ] WHEN applied THEN VersionedEntityName declared as public const string with the stable business name
- [ ] WHEN verified THEN Version mapped to xmin
- [ ] WHEN verified THEN .IsConcurrencyToken() called
- [ ] WHEN verified THEN .ValueGeneratedOnAddOrUpdate() called
- [ ] WHEN verified THEN VersionedEntityName declared as public const string
- [ ] WHEN verified THEN TableName declared as public const string
- [ ] WHEN naming 'EF entity configuration' THEN pattern matches convention
- [ ] When insert entity with duplicate unique-indexed field Then throws DbUpdateException with correct constraint name matching the constant
- [ ] When insert entity with multi-property VO Then all VO columns are persisted flat on entity table
- [ ] When entity relation configured Then navigating the relation returns correct related entities
- [ ] WHEN config inspected for `External Immutable` THEN only creation timestamps are mapped
- [ ] WHEN config inspected for mutable entity THEN creation and update timestamps are mapped
- [ ] WHEN migration generated THEN timestamp columns are non-nullable `DateTimeOffset`
- [ ] WHEN `Internal Immutable` config inspected THEN no timestamp columns are mapped
- [ ] WHEN config inspected for `External Immutable` THEN only creation timestamps are mapped
- [ ] WHEN config inspected for mutable entity THEN creation and update timestamps are mapped
- [ ] WHEN migration generated THEN timestamp columns are non-nullable `DateTimeOffset`
- [ ] WHEN `Internal Immutable` config inspected THEN no timestamp columns are mapped
- [ ] WHEN config inspected for `External Immutable` THEN only creation timestamps are mapped
- [ ] WHEN config inspected for mutable entity THEN creation and update timestamps are mapped
- [ ] WHEN migration generated THEN timestamp columns are non-nullable `DateTimeOffset`
- [ ] WHEN `Internal Immutable` config inspected THEN no timestamp columns are mapped

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]

