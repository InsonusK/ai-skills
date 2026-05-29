---
name: dotnet-entity-patch-command

description: Creates a patch command, DTO, validator, and handler for a .NET entity. Requires response DTO and entity extractor as prerequisites.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - entity
  - patch
  - partial-update
  - command
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
Use this skill when you need create new entity patch command and handler class

# Input data:
You need define these information before start:
- Which entity you need to validate (`{EntityName}`)
- Data which must be given into patch command (DTO fields)
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
2. Use [Patch dto template](./template/patch-dto.md) to create patch dto class in file `{ProjectFolder}/Entities/{EntityName}Entity/Models/{EntityName}PatchRequestDto.cs`
3. Use [Patch command template](./template/patch-command.md) to create patch command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}PatchCommand.cs`.
    - Input DTO `{EntityName}PatchRequestDto`
    - Output DTO `{EntityName}Response.Single`
4. Use [Patch dto validator template](./template/patch-dto-validator.md) to create patch dto validator class in file `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Models/{EntityName}PatchRequestDtoValidator.cs` 
5. Use [Patch handler template](./template/patch-handler.md) to create patch handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}PatchHandler.cs`.

# Rules
- DTO validation rules must be consistent with the entity entity rules
- All properties in patch dto must be optional/nullable.
- Remember to write unit tests, aiming for 80% coverage
