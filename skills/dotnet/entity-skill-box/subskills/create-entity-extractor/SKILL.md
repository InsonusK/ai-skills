---
name: dotnet-entity-extractor

description: Creates a command extractor for a .NET entity. Typically required by generic command handlers like Get, Put, Patch, and Delete.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - entity
  - extractor
  - command
  - generic-handler
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
Use this skill when you need create new entity command extractor.
This is typically required by generic command handlers like Get, Put, Patch, and Delete.

# Input data:
You need define these information before start:
- Which entity you need to extract (`{EntityName}`)

# Required 
- You need the entity class created by [create entity skill](./../create-entity/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Services`
2. Use [Extractor template](./template/extractor.md) to create extractor class in file `{ProjectFolder}/Entities/{EntityName}Entity/Services/{EntityName}CommandExtractor.cs`.
    - Note that this class implements `IEntityCommandExtractor<{EntityName}>`.

# Rules
- Remember to write unit tests, aiming for 80% coverage
