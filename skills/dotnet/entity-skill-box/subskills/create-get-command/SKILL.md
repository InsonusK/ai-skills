---
name: dotnet-entity-get-command
version: 1.0.0
description: >
  Creates a get command and handler for retrieving a single .NET entity.
  Requires response DTO and entity extractor as prerequisites.
type: guide
domain: dotnet
tags:
  - dotnet
  - entity
  - get
  - query
  - single
---

# When to use this skill
Use this skill when you need create new entity get command and handler class

# Input data:
You need define these information before start:
- Which entity you need to get (`{EntityName}`)

# Required 
- You need Response dto for this entity, made by [create response dto skill](./../create-response-dto/SKILL.md)
- You need an Entity Command Extractor for this entity, made by [create entity extractor skill](./../create-entity-extractor/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Commands`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Handlers`
2. Use [Get command template](./template/get-command.md) to create get command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}GetCommand.cs`.
    - Output DTO `{EntityName}Response.Single`
3. Use [Get handler template](./template/get-handler.md) to create get handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}GetHandler.cs`.

# Rules
- Remember to write unit tests, aiming for 80% coverage
