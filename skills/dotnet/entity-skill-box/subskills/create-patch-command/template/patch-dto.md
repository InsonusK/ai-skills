# Template of patch dto

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}PatchRequestDto
{
    // Add fields which must be given into patch command 
    // ALL fields must be nullable/optional. Add ? to type.
    
    // Example:
    // [MaxLength({EntityName}.NameMaxLength)]
    // public string? Name { get; init; }

    public void PatchEntity({EntityName} entity, DateTimeOffset actionTimeStamp)
    {
        // Update entity fields conditionally
        
        // Example:
        // if (Name != null) entity.Name = Name;
        
        // IMPORTANT: Always update UserUpdatedDateTime
        entity.UserUpdatedDateTime = actionTimeStamp;
    }
}
```
