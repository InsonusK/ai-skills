---
name: plateau-offline-sync-service--class-entity-config
description: Class {Entity}Config in the plateau-offline-sync-service plateau — the EF Core IEntityTypeConfiguration that owns one entity's persistence mapping in {Module}.Domain/Configurations
whenToUse: when creating or editing an entity's EF configuration — a table, index, relation, concurrency-token, value-object mapping, or timestamp column
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Own every persistence concern for one entity — table/column/index/constraint names (as constants), relations, the concurrency token, value-object mappings, and timestamp columns — so the entity carries zero EF attributes.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[../../../../../solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `IEntityTypeConfiguration<{Entity}>` in `/{Module}.Domain/Configurations`, one per entity.
- `public const string TableName` and `public const string` for every index / constraint name — never a magic string.
- Multi-property `{ValueObject}` mapped with `OwnsOne`; single-property with a value converter.
- **VP5:** `public const string VersionedEntityName` (the stable business name the concurrency infrastructure routes on); `Version` mapped `IsConcurrencyToken()` (+ `ValueGeneratedOnAddOrUpdate()` against `xmin` in production).
- **VP7:** timestamp columns mapped as required `DateTimeOffset`.
- Registered only by `ApplyConfigurationsFromAssembly` — never by hand. Cross-module FKs live in `App.Infrastructure`, not here.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-entity-config
// Plateau: domain-service
// Version: 20260902000000
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {Module}.Domain.Entities;
using {Module}.Domain.ValueObjects;

namespace {Module}.Domain.Configurations;

public sealed class {Entity}Config : IEntityTypeConfiguration<{Entity}>
{
    public const string TableName = nameof({Entity});
    public const string VersionedEntityName = "{Entity}";               // VP5

    public void Configure(EntityTypeBuilder<{Entity}> b)
    {
        b.ToTable(TableName);
        b.HasKey(e => e.Id);
        b.Property(e => e.Title).HasConversion(t => t.Value, v => new {ValueObject}(v)).IsRequired();
        b.Property(e => e.Version).IsConcurrencyToken();                 // VP5 (xmin in production)
        b.Property(e => e.ServerCreatedDateTime).IsRequired();          // VP7
        b.Property(e => e.UserCreatedDateTime).IsRequired();
        b.Property(e => e.ServerUpdatedDateTime).IsRequired();
        b.Property(e => e.UserUpdatedDateTime).IsRequired();
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.extend.md|{Entity}Config.cs.extend]]
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.extend.md|{Entity}Config.cs.extend]]

# Rules
MUST:
- One `IEntityTypeConfiguration<{Entity}>` per entity in `/Configurations`; `TableName` and all index/constraint names as `public const string`.
- Map every multi-property `{ValueObject}` with `OwnsOne`; configure every relation with `HasForeignKey` + `OnDelete`.
- Declare `public const string VersionedEntityName` and map `Version` `IsConcurrencyToken()` for a mutable entity; map timestamp columns as required `DateTimeOffset` for a user-initiated entity.
- Never use an EF data annotation on the entity; never put mapping in `DbContext.OnModelCreating`; never configure a cross-module FK here.
- Never apply several plateau templates per class.

# Check list
- [ ] `IEntityTypeConfiguration<{Entity}>` in `/Configurations`; `TableName` + names as `const`.
- [ ] `OwnsOne` for multi-property VOs; relations have `HasForeignKey` + `OnDelete`.
- [ ] `VersionedEntityName` + `Version` `IsConcurrencyToken()` iff mutable; timestamp columns required `DateTimeOffset` iff user-initiated.
- [ ] Registered only by `ApplyConfigurationsFromAssembly`.

# Unittest TestCases
- [ ] WHEN a duplicate unique-indexed value is inserted THEN the failure carries the constraint-name constant.
- [ ] WHEN an entity with a multi-property VO is persisted THEN its columns are flat on the entity table.
