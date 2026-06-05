---
uid: 0d4baf72-1689-49d3-81e8-5ee93b22bb28
status: implemented
name: entity-concurrency-pattern
description: Implementation entity versioning and prevent concurrency changing
domain: skill
type: pattern
tags:
  - entity
  - editable
  - concurrency
  - rowversion
triggers:
  - develop editable entity
---
# Goal
Define how work version concurrency in **domain**

# Core Principles
- If entity is editable it must have concurrency field to prevent changes by old data

# Structure / Contracts
## Setup concurrency field
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|Entity]] must implement Version property to store RowVersion
```CSharp
public class SomeEditableDomainEntity
{
    public uint Version { get; internal set; }
}
```
- Inside [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill]] must implement setup Version property as concurrencyToken
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
- Implemetned [[concurrency control pattern.skill]]

# Rules
MUST:
- Entity must have RowVersion field
- Field must be setuped in EF Configuration

# Anti-patterns
- Use timestamp instead RowVersion

# Check list
- [ ] Version field added
- [ ] Version field configured in EF
- [ ] [[concurrency control pattern.skill]] implemented

# Unittest TestCases
- [ ] When entity changed Then RowVersion changed
- [ ] Prevent concurrency changes
	- When make steps
		1. load 2 entites in parallel DBContext
		2. change, save and commit first entity
		3. change and save second entity
	- Then second entity raise exception DbUpdateConcurrencyException 