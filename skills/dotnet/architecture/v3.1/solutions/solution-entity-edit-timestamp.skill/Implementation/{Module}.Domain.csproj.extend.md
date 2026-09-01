---
description: Add timestamp properties to classified entities and map them in EF configuration
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/module-domain-csproj
---

# Goals
- Add the correct timestamp interfaces to each entity based on its classification.
- Keep entity property setters internal while still satisfying mutable timestamp contracts through explicit interface implementation.
- Map timestamp columns in the entity configuration class.

# Core Principles
- Timestamp fields are part of the entity because they describe the entity's lifecycle.
- Only entities that are created and/or updated by user initiative receive timestamp fields.
- Persistence mapping belongs to the configuration class; entities remain free of EF attributes.

# Structure

## Project Structure
```
/{Module}.Domain
  /Entities
    {EntityName}.cs
  /Configurations
    {EntityName}Config.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Entities/{EntityName}.cs | Entity class implementing timestamp interfaces according to classification |
| /Configurations/{EntityName}Config.cs | EF configuration mapping timestamp columns |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IEntityTypeConfiguration<>` and property mapping APIs. |

# Allowed Dependencies
- Shared

# Rules

## MUST
- `External Immutable` entities implement `ICreationInfoModel` only.
- `Internal Mutable` and `External Mutable` entities implement both `ICreationInfoModel` and `IUpdateInfoModel`.
- `Internal Immutable` entities implement none of the timestamp interfaces.
- Timestamp properties are `DateTimeOffset` with `internal set`.
- Mutable timestamp interface setters are implemented explicitly so the class-level setter remains `internal`.
- EF configuration maps timestamp properties as required `DateTimeOffset` columns.
- Never add timestamp fields to `Internal Immutable` entities.
- Never add update timestamp fields to `External Immutable` entities.
- Never use EF attributes on the entity class or properties.
- Never use `DateTime` instead of `DateTimeOffset`.

## SHOULD
- Avoid adding all four timestamp fields to every entity regardless of classification.
- Avoid exposing public setters on entity timestamp properties.
- Avoid mapping timestamp columns with `[Column]` attributes.

# Check list
- [ ] Entity classification determines which timestamp interfaces are implemented.
- [ ] Entity timestamp properties have `internal set`.
- [ ] Entity implements mutable interface setters explicitly.
- [ ] Entity configuration maps timestamp columns as required.
