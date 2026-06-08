---
uid: 57df5ef0-5cc6-40d0-883b-8bd4f0d3b879
orde: "3"
name: domain-configuration
description: Defines the EF Core entity type configuration pattern — one configuration class per entity that owns all persistence concerns, keeping domain entities free of infrastructure attributes
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - ef-core
  - configuration
triggers:
  - create ef configuration
  - configure entity mapping
  - define database schema
  - configure index
  - configure relation
creates:
  - Module EntityConfiguration 
  - App EntityConfiguration
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill]]"
depends_on:
  - "[[01-module-boundary.solution.skill]]"
  - "[[02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill]]"
  - "[[05-entity-base.solution.skill]]"
---
# Goal
- Define one EF Core configuration class per entity that owns all persistence concerns — indexes, relations, concurrency tokens, and value object mappings
- Keep domain entities free of EF attributes and infrastructure concerns
- Ensure all index and constraint names are constants — never magic strings — so they can be referenced in tests and error handling
- Register all configurations via assembly scan — never manually per entity

# Core Principals
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- Index and constraint names are `public static string` constants on the config class
- Domain entity must have zero EF attributes (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- Configuration is the only place that knows about column names, table names, and constraints
- All configurations registered via `ApplyConfigurationsFromAssembly` — never manually
- Cross-module foreign key configurations live in [[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Infrastructure/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]] — not in Domain config

# Depend on solutions
- [[01-module-boundary.solution.skill]] — configuration classes live in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain]]
- [[02-solution-layer-structure.solution.skill]] — [[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Infrastructure/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]] owns cross-module FK configs

# Implementation

## [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain (.csproj)]] (extended)

### Project extension

#### Goals
- Store all EF Core entity type configuration classes for this bounded context
- Own all persistence mapping concerns for this module's entities

#### Core Principals
- One config class per entity — lives in /{Module}.Domain/Configurations
- Config class is the only place that defines column names, index names, and constraints for this entity
- Domain entities have no EF attributes — all mapping is in the config class

#### Structure

##### Project Structure
```
/{Module}.Domain
  /Configurations
    TodoTaskConfig.cs
    OrderConfig.cs
```

##### Directory and class skills
| Directory \| file | Description                    | Pattern skill                                                                                                                                                     |
| ----------------- | ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| /Configurations   | One EF config class per entity | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/EntityConfiguration.class.skill\|EntityConfiguration.class.skill]] |

#### Rules
MUST:
- All EF configuration classes live in /{Module}.Domain/Configurations
- One config class per entity — no shared configs
- All configurations registered via `ApplyConfigurationsFromAssembly` in DbContext

MUST NOT:
- Place EF config classes outside /Configurations folder
- Use EF data annotations on domain entity classes
- Put mapping logic directly in `DbContext.OnModelCreating`

#### Anti-patterns
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Registering configs manually one by one in DbContext — use `ApplyConfigurationsFromAssembly`
- Sharing one config class across multiple entity types

#### Check list
- [ ] /Configurations folder exists in {Module}.Domain
- [ ] One config class per entity
- [ ] No EF attributes on any entity class in this module
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`

---

### Class extension

#### EntityConfiguration

##### Goals
- Own all persistence concerns for one entity — indexes, relations, concurrency tokens, value object mappings
- Keep the domain entity free of EF attributes and infrastructure annotations
- Ensure index and constraint names are constants so they can be referenced in tests and error handling
- Define one EF Core configuration class per entity that owns all persistence concerns

##### Core Principals
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- Index and constraint names are `public static string` constants on the config class
- Domain entity must have zero EF attributes
- Configuration is the only place that knows about column names, table names, and constraints
- Multi-property Value Object properties require `OwnsOne` mapping here
- Cross-module foreign key configurations live in App.Infrastructure — not here
##### Naming convention

| use case                | class name pattern | class name     | file name pattern  | file name          |
| ----------------------- | ------------------ | -------------- | ------------------ | ------------------ |
| Entity EF configuration | {Entity}Config     | TodoTaskConfig | {Entity}.Config.cs | TodoTask.Config.cs |

##### Implementation changes
EntityConfiguration must implement `IEntityTypeConfiguration<TEntity>`. Index names must be `public static string` constants. All mapping defined in `Configure` method.

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

##### Rule changes
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

##### Anti-patterns (extended)
- Mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Single config class shared across multiple entities — one config per entity, no exceptions
- Cross-module FK configured in Domain config — belongs in App.Infrastructure

##### Check list (extended)
- [ ] One config class exists per entity
- [ ] Config class is in /{Module}.Domain/Configurations
- [ ] `TableName` defined as `public static string` constant
- [ ] All index and constraint names defined as `public static string` constants
- [ ] All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured with `HasForeignKey` and `OnDelete`
- [ ] `OwnsOne` configured for every multi-property VO property
- [ ] No EF attributes on the domain entity class

##### Unittest TestCases (extended)
- [ ] When insert entity with duplicate unique-indexed field Then throws DbUpdateException with correct constraint name matching the constant
- [ ] When insert entity with multi-property VO Then all VO columns are persisted flat on entity table
- [ ] When entity relation configured Then navigating the relation returns correct related entities

---

#### [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity]] (extended)

##### Goals
- Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class

##### Core Principals
- Entity has zero EF attributes — `[Column]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]` are all forbidden
- Entity does not know about its own table name, column names, or constraint names

##### Implementation changes
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

##### Rule changes
MUST NOT:
- Entity have any EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`)

##### Anti-patterns (extended)
- `[Column("task_title")]` on entity property — column mapping belongs in config class
- `[Table("TodoTasks")]` on entity class — table naming belongs in config class

##### Check list (extended)
- [ ] No EF attributes present on entity class or any of its properties

---


## [[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Infrastructure/App.Infrastructure.csproj.skill|App.Infrastructure (.csproj)]] (extended)

### Project extension
#### Goals
- Host cross-module foreign key configurations that span multiple bounded contexts
- Register all module entity configurations via `ApplyConfigurationsFromAssembly` in AppDbContext

#### Core Principals
- App.Infrastructure is the only place where cross-module foreign key relationships are configured
- DbContext uses `ApplyConfigurationsFromAssembly` to automatically discover all `IEntityTypeConfiguration<T>` implementations from module Domain assemblies
- App.Infrastructure references all module Domain projects to access entities for cross-module configuration

#### Structure
##### Project Structure
```
/App.Infrastructure
  /Persistence
    /Configurations
      CrossModuleFkConfig.cs
```

##### Directory and class skills
| Directory \| file           | Description                                              | Pattern skill |
| --------------------------- | -------------------------------------------------------- | ------------- |
| /Persistence/Configurations | Cross-module foreign key and relationship configurations |               |

#### What Does NOT Belong Here
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`

#### Allowed Dependencies
- `{ModuleName}.Domain` (all modules) — required to access entities and their configurations

#### Rules
MUST:
- Register all configurations via `ApplyConfigurationsFromAssembly` scanning all module Domain assemblies in DbContext
- Place cross-module foreign key configurations in `/Persistence/Configurations`

MUST NOT:
- Define intra-module entity configurations here — those belong in `{Module}.Domain/Configurations`
- Register configurations manually one by one in `OnModelCreating`

#### Anti-patterns
- Putting module-internal entity configuration in App.Infrastructure — violates separation of concerns
- Manually registering each config class in `OnModelCreating` instead of using assembly scan

#### Check list
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] Cross-module FK configs live in `/Persistence/Configurations`
- [ ] No intra-module entity config placed in App.Infrastructure

### Class extension
#### EntityConfiguration (extended)

##### Goals
- Configure foreign key relationships and mappings that cross module boundaries

##### Core Principals
- Cross-module configuration references entities from multiple modules without redefining their intra-module mapping
- Cross-module config composes on top of existing Domain configs, never duplicates them
##### Naming convention

| use case                   | class name pattern         | class name           | file name pattern              | file name                |
| -------------------------- | -------------------------- | -------------------- | ------------------------------ | ------------------------ |
| Relation between 2 modules | {Module1}To{Module2}Config | OrderToPaymentConfig | {Module1}To{Module2}.Config.cs | OrderToPayment.Config.cs |

##### Implementation changes
Cross-module foreign key configuration lives in App.Infrastructure:

```csharp
public class OrderToPaymentConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
            .HasOne<Payment>()
            .WithMany()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

##### Rule changes
MUST:
- Configure only cross-module foreign keys and relationships
- Reference existing entity configs from Domain, not redefine intra-module mapping

MUST NOT:
- Redefine table names, column names, or indexes already owned by Domain config
- Configure intra-module relationships here


# Rules
MUST:
- One `IEntityTypeConfiguration<T>` per entity
- All index and constraint names defined as `public static string` constants
- `OwnsOne` configured for every multi-property VO property
- All configurations registered via `ApplyConfigurationsFromAssembly`
- Domain entities have zero EF attributes

MUST NOT:
- Use EF data annotations on domain entities
- Define constraint names as inline strings
- Put mapping logic in `DbContext.OnModelCreating` directly
- Configure cross-module foreign keys in Domain config

# Anti-patterns
- Mapping multi-property VO without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Registering configs manually in DbContext — use `ApplyConfigurationsFromAssembly`

# Check list
- [ ] One config class per entity in /{Module}.Domain/Configurations
- [ ] All index names defined as `public static string` constants
- [ ] All unique indexes use `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured
- [ ] `OwnsOne` configured for every multi-property VO
- [ ] No EF attributes on any domain entity
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`

# Unittest TestCases
- [ ] When insert entity with duplicate unique-indexed field Then DbUpdateException thrown with constraint name matching constant
- [ ] When insert entity with multi-property VO Then VO columns persisted flat on entity table
- [ ] When entity with relation loaded Then navigation property returns correct related entities
