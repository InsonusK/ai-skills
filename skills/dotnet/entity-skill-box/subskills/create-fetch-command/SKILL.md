---
name: dotnet-entity-fetch-command
version: 1.0.0
description: >
  Creates a fetch command and handler for retrieving multiple .NET entities.
  Requires a response DTO as a prerequisite.
type: guide
domain: dotnet
tags:
  - dotnet
  - entity
  - fetch
  - query
  - list
---

# When to use this skill
Use this skill when you need to create a new entity fetch command and handler class

# Input data:
You need to define these information before start:
- Which entity you need to fetch (`{EntityName}`)

# Required 
- You need Response dto for this entity, made by [create response dto skill](./../create-response-dto/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Commands`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Handlers`
2. Use [Fetch command template](./template/fetch-command.md) to create fetch command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}FetchCommand.cs`.
    - Output DTO `{EntityName}Response.Many`
3. Use [Fetch handler template](./template/fetch-handler.md) to create fetch handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}FetchHandler.cs`.

# Rules
- Remember to write unit tests, aiming for 80% coverage
