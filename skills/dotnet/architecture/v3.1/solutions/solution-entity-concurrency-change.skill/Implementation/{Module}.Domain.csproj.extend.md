---
description: Add Version concurrency token to every mutable entity
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/module-domain-csproj
---

# Goals
- Add `Version` concurrency token to every mutable entity, implement `IVersioned`, and configure it as the PostgreSQL `xmin` system column; declare `VersionedEntityName` in the config class

# Core Principles
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE`
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code

# Structure

## Project Structure
```
/{Module}.Domain
  /Entities
    {Entity}.cs          ← extended with Version property
  /Configurations
    {Entity}Config.cs    ← extended with Version mapping
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Entities/{Entity}.cs | Mutable entity with uint Version property implementing IVersioned |
| /Configurations/{Entity}Config.cs | EF configuration mapping Version to xmin and declaring VersionedEntityName |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IsConcurrencyToken()`, `ValueGeneratedOnAddOrUpdate()`, `HasColumnName()` |

# Allowed Dependencies
- Shared

# Rules

## MUST
- Every mutable entity has a `Version` property
- Every mutable entity implements `IVersioned`
- Every mutable entity config class declares `VersionedEntityName`
- `Version` configured as `IsConcurrencyToken()` mapping to `xmin` in EF configuration
- Never application code set or read `Version` for any purpose other than concurrency checking — it is a database concern

## SHOULD
- Avoid `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL

# Check list
- [ ] `uint Version { get; internal set; }` present on every mutable entity
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
