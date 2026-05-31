---
name: entity-delete-dto-template
description: Template of delete dto
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - delete-command
    - delete-dto
---
# Template of delete dto

```csharp
using InsonusK.Shared.Models.Template;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}PatchRequestDto:IDeleteRequestWithLastStateOptionDto
{
    public bool ReturnLastState { get; init; } = false;
}
```