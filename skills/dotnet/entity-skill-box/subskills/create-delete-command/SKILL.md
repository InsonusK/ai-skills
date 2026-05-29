---
name: dotnet-entity-delete-command

description: Creates a delete command and handler for a .NET entity. Requires response DTO and entity extractor as prerequisites.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - entity
  - delete
  - command
  - handler
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
Use this skill when you need create new entity delete command and handler class

# Input data:
You need define these information before start:
- Which entity you need to validate (`{EntityName}`)

# Required 
- You need Response dto for this entity, made by [create response dto skill](./../create-response-dto/SKILL.md)
- You need an Entity Command Extractor for this entity, made by [create entity extractor skill](./../create-entity-extractor/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Commands`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Handlers`
2. Use [Delete command template](./template/delete-command.md) to create delete command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}DeleteCommand.cs`.
    - Output DTO `{EntityName}Response.Single`
3. Use [Delete handler template](./template/delete-handler.md) to create delete handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}DeleteHandler.cs`.

# Rules
- Remember to write unit tests, aiming for 80% coverage
