---
name: Create new entity response dto
version: 1.0.0
description: create new entity response dto skill
---

# When to use this skill
Use this skill when you need create new entity response dto

# Input data:
You need define these information before start:
- For which entity do you need to create a response DTO? (`{EntityName}`)
- What fields must be given into response dto

# How to use it
1. Define which response dto interface you need to use from `InsonusK.Shared.Models.Template`

| IsEditable | Used Interface |
| --- | --- |
| true | IResponseGuidEditableEntity |
| false | IResponseNoGuidEditableEntity |

2. Create folder `{ProjectFolder}/Entities/{EntityName}Entity/Models`
3. Use [Response dto template](./template/reponse-dto.md) to create response dto class in file `{ProjectFolder}/Entities/{EntityName}Entity/Models/{EntityName}Response.cs`