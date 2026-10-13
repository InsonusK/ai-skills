---
description: Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
project_name: "{Module}.Domain"
name: "{Entity}Config.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-configuration
  - element/entity-config-cs
---

# Goals
- Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
- Keep the domain entity free of EF attributes and infrastructure annotations
- Ensure index and constraint names are constants so they can be referenced in tests and error handling
- Define one EF Core configuration class per entity that owns all persistence concerns

# Core Principles
- Multi-property Value Object properties require `OwnsOne` mapping here

# Naming convention

| use case                | class name pattern | class name     | file name pattern  | file name          |
| ----------------------- | ------------------ | -------------- | ------------------ | ------------------ |
| Entity EF configuration | {Entity}Config     | TodoTaskConfig | {Entity}.Config.cs | TodoTask.Config.cs |

# Implementation changes

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

# Rule changes

## MUST
- One config class per entity
- `TableName` defined as `public const string`
- All index and constraint names defined as `public const string` constants on the config class
- `OwnsOne` configured for every multi-property Value Object property on this entity
- All relations configured with `HasMany`/`HasOne`, `HasForeignKey`, and `OnDelete`
- Registered via `ApplyConfigurationsFromAssembly` — never manually
- Never use EF data annotations on the domain entity (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- Never define table names, column names, or constraint names as inline strings
- Never use `static` instead of `const` for `TableName`, index, or constraint names
- Never put mapping logic in `DbContext.OnModelCreating` directly
- Never configure cross-module foreign keys here — those belong in App.Infrastructure
## SHOULD
- Avoid mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Avoid hardcoded index name strings — breaks error handling that matches constraint names
- Avoid using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Avoid single config class shared across multiple entities — one config per entity, no exceptions
- Avoid cross-module FK configured in Domain config — belongs in App.Infrastructure

# Check list
- [ ] One config class exists per entity
- [ ] Config class is in /{Module}.Domain/Configurations
- [ ] All index and constraint names defined as `public const string`
- [ ] All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured with `HasForeignKey` and `OnDelete`
- [ ] `OwnsOne` configured for every multi-property VO property
- [ ] No EF attributes on the domain entity class

# Unittest TestCases
- [ ] When insert entity with duplicate unique-indexed field Then throws DbUpdateException with correct constraint name matching the constant
- [ ] When insert entity with multi-property VO Then all VO columns are persisted flat on entity table
- [ ] When entity relation configured Then navigating the relation returns correct related entities
