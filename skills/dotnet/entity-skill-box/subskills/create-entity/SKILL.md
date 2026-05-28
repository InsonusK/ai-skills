---
name: dotnet-entity-create
version: 1.0.0
description: >
  Creates a new entity class and its configuration in a .NET project.
  Covers entity fields, constraints, relations, and soft-delete strategy.
type: guide
domain: dotnet
tags:
  - dotnet
  - entity
  - domain-model
  - configuration
  - ef-core
---

# When to use this skill
Use this skill when you need create new entity class

# Input data:
You need define these information before start:
- Entity name (`{EntityName}`)
- Is the entity a Constant entity or an Editable entity?
- Who initiates entity creation?
  - Created by backend
  - Created by other system
- Is it a composite entity? (For many-to-many relationships)
- Entity fields
  - Constraint (is Required, min max len and etc. )
  - Validation rule
  - Relations
  - Indexes and Unique Index
- Project which contain entity (`{ProjectFolder}`)
- Is it soft deleted or force deleted? (`{IsSoftDeleted}`)

# How to use it
1. Create folder `{ProjectFolder}/Entities/{EntityName}Entity`
2. Use [Entity template](./template/entity.md) to create entity class in file `{ProjectFolder}/Entities/{EntityName}Entity/{EntityName}.cs`
3. Create folder `{ProjectFolder}/Entities/{EntityName}Entity/Configs`
4. Use [Entity config template](./template/entity-config.md) to create entity config class in file `{ProjectFolder}/Entities/{EntityName}Entity/Configs/{EntityName}Config.cs` 

# Rules
1. Don't use dotnet migration. All migration must be created manually