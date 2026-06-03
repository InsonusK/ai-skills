---
uid: 0d4baf72-1689-49d3-81e8-5ee93b22bb28
status: implemented
name: entity-concurency-pattern
description: Implementation entity versioning and prevent concurrency changing
domain: skill
type: pattern
tags:
  - entity
  - editable
  - concurency
  - rowversion
triggers:
  - develop editable entity
---
# Goal
Define how work version concurency in domain

# Core Principles
- If entity is editable it must have concurrency field to prevent changes by old data

# Structure / Contracts
## Setup concurrency field
- [[entity-pattern.skill|Entity]] must implement Version property to store RowVersion
```CSharp
public class SomeEditableDomainEntity
{
    public uint Version { get; internal set; }
}
```
- Inside [[domain-configuration-pattern.skill]] must implement setup Version property as ConcurencyToken
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
- Implemetned [[concurency control pattern.skill]]
# Rules
MUST:
- Entity must have RowVersion field
- Field must be setuped in EF Configuration
- [[entity-concurrency-pattern.skill]] must be applyed

# Anti-patterns
- Use timestamp instead RowVersion

# Check list
- [ ] Version field added
- [ ] Version field configured in EF
- [ ] [[concurency control pattern.skill]] implemented
- [ ] unit test usecases implemented and passed
	- [ ] When entity changed Then RowVersion changed
	- [ ] Prevent concurency changes
		- When 
			1. load 2 entites in parallel DBContext
			2. change, save and commit first entity
			3. change and save second entity
		- Then second entity raise exception DbUpdateConcurrencyException 