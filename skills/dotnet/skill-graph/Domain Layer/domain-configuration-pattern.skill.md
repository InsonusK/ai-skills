---
uid: 63ee3f0b-7b1e-4a5d-b0b1-5b0ef7a8aa00
status: draft
name: domain-configuration-pattern
description: rules for implementing EF Core entity type configurations
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - ef-core
  - configuration
triggers:
  - ef core entity configuration
  - entity mapping
  - database schema definition
  - index configuration
aliases:
  - EF Configuration
  - EntityTypeConfiguration
  - Configuration
---
# Goal
Define a unified pattern for implementing EF Core entity type configurations. A configuration class owns all persistence concerns for one entity — indexes, relations, concurrency tokens, and value object mappings — keeping the domain entity itself free of infrastructure attributes and annotations. Without this pattern, mapping logic scatters across DbContext, entity classes, and data annotations, making persistence rules invisible and hard to enforce.

# Core Principles
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration owns all persistence concerns; entity owns all domain concerns
- Index names are constants defined on the config class — never magic strings
- Domain entity must have zero EF attributes (`[Column]`, `[Index]`, etc.)
- Configuration is the only place that knows about column names, table names, and constraints

# Structure / Contracts
## File location
```
/Domain
	/Configurations
		TodoTaskConfig.cs
		OrderConfig.cs
```

## Base shape
```CSharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        // indexes, relations, mappings go here
    }
}
```

## Index definition
Index names are static constants on the config class — referenced in tests and error handling.
```CSharp
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

## Relation definition
```CSharp
entityBuilder
    .HasMany(e => e.SubTasks)
    .WithOne(e => e.Task)
    .HasForeignKey(e => e.TaskId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

## Multi-property Value Object mapping (OwnsOne)
Required for any entity that holds a multi-property VO — see [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill]].
```CSharp
entityBuilder.OwnsOne(e => e.Cash, money =>
{
    money.Property(m => m.Amount).HasColumnName("Cash_Amount");
    money.Property(m => m.Currency).HasColumnName("Cash_Currency").HasMaxLength(3);
});
```

## Concurrency token (RowVersion)
Required for all mutable entities — see [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill]].
```CSharp
entityBuilder
    .Property(e => e.Version)
    .HasColumnName("xmin")
    .IsConcurrencyToken()
    .ValueGeneratedOnAddOrUpdate();
```

## Registration in DbContext
All configurations are registered via assembly scan — never manually per entity.
```CSharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TodoTaskConfig).Assembly);
}
```

# Rules

MUST:
- one config class per entity
- all index names defined as `public static string` constants on the config class
- `OwnsOne` configured for every multi-property Value Object property
- `IsConcurrencyToken()` configured for every mutable entity `Version` field
- registered via `ApplyConfigurationsFromAssembly` 
COULD:
- Cross module relations could be made in incfrastructure project
MUST NOT:
- use EF data annotations on domain entity (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- define table names, column names, or constraint names as inline strings
- put mapping logic in `DbContext.OnModelCreating` directly

# Anti-patterns
- Mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Skipping concurrency token config on mutable entity — concurrency control silently does nothing
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config

# Checklist
- [ ]  One config class exists per entity
- [ ]  Config class is in `/Domain/Configurations/`
- [ ]  Table-level index names defined as `public static string` constants
- [ ]  All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ]  All relations configured (`HasMany`/`HasOne`, `HasForeignKey`, `OnDelete`)
- [ ]  `OwnsOne` configured for every multi-property VO property
- [ ]  `Version` property configured as `IsConcurrencyToken()` if entity is mutable
- [ ]  No EF attributes on the domain entity class

# Unittest TestCases
- [ ]  When insert entity with duplicate Guid Then throws DbUpdateException with correct constraint name
- [ ]  When two contexts update same mutable entity concurrently Then second throws DbUpdateConcurrencyException
- [ ]  When insert entity with multi-property VO Then columns are persisted flat on entity table

# Relations
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill]] — defines which entity types require which configuration pieces
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill]] — requires Version field registered as IsConcurrencyToken
- [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill]] — requires unique index on Guid field
- [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill]] — multi-property VOs require OwnsOne mapping here