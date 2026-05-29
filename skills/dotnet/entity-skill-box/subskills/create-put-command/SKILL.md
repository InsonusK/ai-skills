---
name: dotnet-entity-put-command

description: Creates a put command, DTO, validator, and handler for a .NET entity. Requires response DTO and entity extractor as prerequisites.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - entity
  - put
  - update
  - command
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
Use this skill when you need create new entity put command and handler class

# Input data:
You need define these information before start:
- Which entity you need to validate (`{EntityName}`)
- Data which must be given into put command (DTO fields)
- Validation rules for DTO

# Required 
- You need Response dto for this entity, made by [create response dto skill](./../create-response-dto/SKILL.md)
- You need an Entity Command Extractor for this entity, made by [create entity extractor skill](./../create-entity-extractor/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Models`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Commands`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Handlers`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Models`
2. Use [Put dto template](./template/put-dto.md) to create put dto class in file `{ProjectFolder}/Entities/{EntityName}Entity/Models/{EntityName}PutRequestDto.cs`
3. Use [Put command template](./template/put-command.md) to create put command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}PutCommand.cs`.
    - Input DTO `{EntityName}PutRequestDto`
    - Output DTO `{EntityName}Response.Single`
4. Use [Put dto validator template](./template/put-dto-validator.md) to create put dto validator class in file `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Models/{EntityName}PutRequestDtoValidator.cs` 
5. Use [Put handler template](./template/put-handler.md) to create put handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}PutHandler.cs`.

# Rules
- DTO validation rules must be consistent with the entity entity rules
- Remember to write unit tests, aiming for 80% coverage