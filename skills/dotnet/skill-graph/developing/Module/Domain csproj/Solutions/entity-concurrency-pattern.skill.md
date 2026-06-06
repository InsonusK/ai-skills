---
uid: 0d4baf72-1689-49d3-81e8-5ee93b22bb28
name: entity-concurrency-pattern
description: rules for adding the Version field and configuring the EF concurrency token on mutable entities
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - entity
  - editable
  - concurrency
  - rowversion
  - skill/pattern/solution
triggers:
  - develop editable entity
  - mutable entity concurrency
  - add Version field
  - EF concurrency token
  - prevent lost update
---
# Goal
Define how to add concurrency protection to a mutable entity in the domain model. Every entity that can be edited after creation must carry a `Version` field mapped as an EF concurrency token. Without this, the application has no way to detect or prevent lost updates.

# Core Principles
- `Version` is the domain's representation of the database row version
- EF maps `Version` to PostgreSQL `xmin` column — automatically updated on every row change
- `Version` is read-only from outside the entity — only EF sets it
- Application-level concurrency checking (ConcurrencyBehavior) reads this field — see concurrency-control.solution.skill
- This skill covers only the domain side: the field declaration and its EF configuration

# Solution
## Setup Domain — Add Version Field
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] must implement `Version` property to store `RowVersion`
```CSharp
public class SomeEditableDomainEntity
{
    public uint Version { get; internal set; }
}
```
## EF Configuration — Map as Concurrency Token
- Inside [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-configuration-pattern.skill|domain-configuration-pattern.skill]] must implement setup `Version` property as `concurrencyToken`
```CSharp
public class SomeEditableDomainEntityConfig 
	: IEntityTypeConfiguration<SomeEditableDomainEntity>
{
    public void Configure(
	    EntityTypeBuilder<SomeEditableDomainEntity> entityBuilder)
    {
        entityBuilder
          .Property(e => e.Version)
          .HasColumnName("xmin") 
          .IsConcurrencyToken()
          .ValueGeneratedOnAddOrUpdate();
    }
}
```
## Setup Concurrency validation
- Implemetned [[skills/dotnet/skill-graph/developing/Module/Application Layer/concurrency-control-pattern.skill|concurrency-control-pattern.skill]]

# Rules
MUST:
- All mutable entities (Internal Mutable, External Mutable from [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill#Entity Type Matrix|entity.skill matrix]]) have `uint Version`
- `Version` has `internal set` — never public
- EF configuration maps `Version` to `xmin` with `IsConcurrencyToken()`
MUST NOT:
- Use timestamp or application-managed counter instead of `xmin`
- Expose `Version` as public setter
- Perform concurrency checking inside the entity — that belongs in the pipeline
# Anti-patterns
- Use timestamp instead RowVersion

# Check list
- [ ] `uint Version { get; internal set; }` added to entity
- [ ] EF configuration maps `Version` to `xmin`
- [ ] `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` configured
- [ ] No public setter on `Version`
- [ ] [[skills/dotnet/skill-graph/developing/Module/Application Layer/concurrency-control-pattern.skill|concurrency-control-pattern.skill]] implemented

# Unittest TestCases
- [ ] When entity saved Then Version is non-zero
- [ ] When entity updated Then Version changes
- [ ] When two contexts load same entity, first saves, second saves Then `DbUpdateConcurrencyException` thrown

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] — identifies which entity types require this pattern
- [[files/ef-configuration.skill|ef-configuration.skill]] — EF mapping for the Version field goes in the config class
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/concurrency-control-pattern.skill|concurrency-control-pattern.skill]] — pipeline behavior that reads Version for HTTP-level conflict detection