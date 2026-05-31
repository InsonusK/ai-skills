---
name: entity-post-dto-template
description: Template of post dto
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - post-command
    - post-dto
---
# Template of post dto

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using InsonusK.Shared.Models.Common;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}PostRequestDto : {EntityName}.IBody,IGuidModel
{
    // Add fields which must be given into post command 
    // If field is required in entity, add [Required] attribute
    // If field is nullable in entity, add ? to type

    public {EntityName} ToNewEntity(DateTimeOffset actionTimeStamp)
    {
        return new {EntityName}
        {
            // Define how to fill entity from DTO
            ...
            // Fill datetime info
            UserCreatedDateTime = actionTimeStamp,
            UserUpdatedDateTime = actionTimeStamp
        };
    }
}
```