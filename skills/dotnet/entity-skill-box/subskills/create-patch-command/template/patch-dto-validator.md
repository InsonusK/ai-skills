---
name: entity-patch-dto-validator-template
description: Template of patch dto validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - patch-command
    - patch-dto-validator
---
# Template of patch dto validator

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Models;

public class {EntityName}PatchRequestDtoValidator : AbstractValidator<{EntityName}PatchRequestDto>
{
    public {EntityName}PatchRequestDtoValidator()
    {
        // Add logic how validate DTO
    }
}
```
