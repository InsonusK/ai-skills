---
description: Add per-entity version resolvers to the Application layer
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/module-application-csproj
---

# Goals
- Realize `IEntityVersionResolver` for every versioned entity in the module
- Keep version loading next to the module's specifications and repositories

# Core Principles
- Each versioned entity has a dedicated `{Entity}VersionResolver` class in `{Module}.Application/Concurrency`
- Resolver's `VersionedEntityName` constant matches the Domain config constant exactly

# Structure

## Project Structure
```
/{Module}.Application
  /Concurrency
    {Entity}VersionResolver.cs
  /Specifications
    {Entity}ByIdSpec.cs
  /Features
    /{FeatureName}
      {Command}Handler.cs
      {Query}Handler.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Concurrency/{Entity}VersionResolver.cs | Reads the current version for one versioned entity |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Ardalis.Specification` | latest stable | `{Entity}ByIdSpec` inherits `Specification<T>` |

# Allowed Dependencies
- Shared
- `{Module}.Domain`
- `{Module}.Interfaces`
- MediatR

# Rules

## MUST
- Declare one `{Entity}VersionResolver` per versioned entity
- Resolver class implements `IEntityVersionResolver` from Shared
- Resolver declares `public const string VersionedEntityName` matching `{Entity}Config.VersionedEntityName`
- Resolver uses `IReadRepository<{Entity}>` and `{Entity}ByIdSpec`
- Return `0` when entity is not found
- Return current `Version` cast to `int` when found
- Never reference EF Core or DbContext directly
- Never throw when entity is missing — return `0`

## SHOULD
- Avoid implementing resolver in App.Infrastructure or BuildingBlocks — Application owns the read logic
- Avoid using reflection to read `Version` instead of `IVersioned`

# Check list
- [ ] `{Entity}VersionResolver` exists for every versioned entity
- [ ] Resolver implements `IEntityVersionResolver`
- [ ] `VersionedEntityName` constant matches Domain config
- [ ] Returns `0` for missing entity

# Unittest TestCases
- [ ] WHEN applied THEN Realize IEntityVersionResolver for every versioned entity in the module
- [ ] WHEN applied THEN Keep version loading next to the module's specifications and repositories
- [ ] WHEN applied THEN Each versioned entity has a dedicated {Entity}VersionResolver class in {Module}.Application/Concurrency
- [ ] WHEN applied THEN Resolvers use the module's {Entity}ByIdSpec and IReadRepository<{Entity}> from Shared
- [ ] WHEN applied THEN Resolver's VersionedEntityName constant matches the Domain config constant exactly
- [ ] WHEN verified THEN {Entity}VersionResolver exists for every versioned entity
- [ ] WHEN verified THEN Resolver implements IEntityVersionResolver
- [ ] WHEN verified THEN VersionedEntityName constant matches Domain config
