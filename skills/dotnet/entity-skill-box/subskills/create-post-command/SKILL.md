---
name: dotnet-entity-post-command
version: 1.0.0
description: >
  Creates a post command, DTO, validator, and handler for a .NET entity.
  Requires a response DTO as a prerequisite.
type: guide
domain: dotnet
tags:
  - dotnet
  - entity
  - post
  - create
  - command
---

# When to use this skill
Use this skill when you need create new entity post command and handler class

# Input data:
You need define these information before start:
- Which entity you need to validate (`{EntityName}`)
- Data which must be given into post command (DTO fields)
- Validation rules for DTO

# Required 
- You need Response dto for this entity, made by [create response dto skill](./../create-response-dto/SKILL.md)

# How to use it
1. Create folders:
    - `{ProjectFolder}/Entities/{EntityName}Entity/Models`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Commands`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Handlers`
    - `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Models`
2. Use [Post dto template](./template/post-dto.md) to create post dto class in file `{ProjectFolder}/Entities/{EntityName}Entity/Models/{EntityName}PostRequestDto.cs`
3. Use [Post command template](./template/post-command.md) to create post command class in file `{ProjectFolder}/Entities/{EntityName}Entity/Commands/{EntityName}PostCommand.cs`.
    - Input DTO `{EntityName}PostRequestDto`
    - Output DTO `{EntityName}Response.Single` or `{EntityName}Response.Many`
4. Use [Post dto validator template](./template/post-dto-validator.md) to create post dto validator class in file `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Models/{EntityName}PostRequestDtoValidator.cs` 
5. Use [Post handler template](./template/post-handler.md) to create post handler class in file `{ProjectFolder}/Entities/{EntityName}Entity/Handlers/{EntityName}PostHandler.cs`.

# Rules
- DTO validation rules must be consistent with the entity entity rules
- Remember to write unit tests, aiming for 80% coverage