---
name: entity-class-template
description: Template of entity
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - entity
---
# Template of entity

```C#
using System.ComponentModel.DataAnnotations;
using InsonusK.Share.Database.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity;

public class {EntityName} : {{ baseEntity }}, {EntityName}.IBody, {% if isSoftDeleted %} IDeleteStatusEntity {% endif %}
{   
    // all constaint must be defined as const
    // this is example of constaint
    public const int NameMaxLength = 15;
    public const int DescriptionMaxLength = 50;

    // all properties must be defined as public and internal set
    // this is example of property
    [MaxLength(NameMaxLength)]
    [Required]
    public string Name { get; internal set; } = "";

    [MaxLength(DescriptionMaxLength)]
    public string Description { get; internal set; } = "";

    // all properties must implement IBody interface
    public interface IBody
    {
        // all properties must implement IBody interface. 
        // All properties must be read-only
        // this is example of body
        string Name { get; }
        string Description { get; }
    }
}
```

## Base Entity

Use library `InsonusK.Share.Database` to get base entity.

| Is constant | Is created by other system | Is Composite     | Base Entity |
|-------------|----------------------------|----------------- |---------------|
| True        | False                      | False            | EntityBase |
| True        | False or True              | True             | ConstantNoGuidEntity |
| True        | True                       | False            | ConstantGuidEntity |
| False       | False or True              | True             | EditableNoGuidEntity |
| False       | True                       | False            | EditableGuidEntity |
| False       | False                      | False            | EditableNoGuidEntity |

## Is Soft Deleted

If entity is soft deleted, it must implement IDeleteStatusEntity interface.