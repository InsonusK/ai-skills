# Template of post dto

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using InsonusK.Shared.Models.Common;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}PutRequestDto : {EntityName}.IBody
{
    // Add fields which must be given into post command 
    // If field is required in entity, add [Required] attribute
    // If field is nullable in entity, add ? to type

    public void UpdateEntity({EntityName} entity, DateTimeOffset actionTimeStamp)
    {
        // Update entity fields
        // Example:
        // entity.Name = Name;
        
        // IMPORTANT: Always update UserUpdatedDateTime
        entity.UserUpdatedDateTime = actionTimeStamp;
    }
}
```