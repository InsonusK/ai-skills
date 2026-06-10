---
description: Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
project_name: "{Module}.Domain.csproj"
name: "{Entity}Config.cs"
change_kind: create
---

# Goals
- Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
- Keep the domain entity free of EF attributes and infrastructure annotations
- Ensure index and constraint names are constants so they can be referenced in tests and error handling
- Define one EF Core configuration class per entity that owns all persistence concerns

# Core Principals
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- Index and constraint names are `public static string` constants on the config class
- Domain entity must have zero EF attributes
- Configuration is the only place that knows about column names, table names, and constraints
- Multi-property Value Object properties require `OwnsOne` mapping here
- Cross-module foreign key configurations live in App.Infrastructure — not here

# Naming convention

| use case                | class name pattern | class name     | file name pattern  | file name          |
| ----------------------- | ------------------ | -------------- | ------------------ | ------------------ |
| Entity EF configuration | {Entity}Config     | TodoTaskConfig | {Entity}.Config.cs | TodoTask.Config.cs |

# Implementation changes

{Entity}Config must implement `IEntityTypeConfiguration<{Entity}>`. Index names must be `public static string` constants. All mapping defined in `Configure` method.

Base shape:
```csharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);

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
    public static string TableName = nameof(TodoTask);
    public static string UX_Guid = $"UX_{TableName}_Guid";

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

MUST:
- One config class per entity
- All index and constraint names defined as `public static string` constants on the config class
- `OwnsOne` configured for every multi-property Value Object property on this entity
- All relations configured with `HasMany`/`HasOne`, `HasForeignKey`, and `OnDelete`
- Registered via `ApplyConfigurationsFromAssembly` — never manually

MUST NOT:
- Use EF data annotations on the domain entity (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- Define table names, column names, or constraint names as inline strings
- Put mapping logic in `DbContext.OnModelCreating` directly
- Configure cross-module foreign keys here — those belong in App.Infrastructure

# Anti-patterns
- Mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Single config class shared across multiple entities — one config per entity, no exceptions
- Cross-module FK configured in Domain config — belongs in App.Infrastructure

# Check list
- [ ] One config class exists per entity
- [ ] Config class is in /{Module}.Domain/Configurations
- [ ] `TableName` defined as `public static string` constant
- [ ] All index and constraint names defined as `public static string` constants
- [ ] All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured with `HasForeignKey` and `OnDelete`
- [ ] `OwnsOne` configured for every multi-property VO property
- [ ] No EF attributes on the domain entity class

# Unittest TestCases
- [ ] When insert entity with duplicate unique-indexed field Then throws DbUpdateException with correct constraint name matching the constant
- [ ] When insert entity with multi-property VO Then all VO columns are persisted flat on entity table
- [ ] When entity relation configured Then navigating the relation returns correct related entities
