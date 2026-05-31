---
name: entity-post-dto-validator-template
description: Template of post dto validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - post-command
    - post-dto-validator
---
# Template of post dto validator

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Models;

public class {EntityName}PostRequestDtoValidator : AbstractValidator<{EntityName}PostRequestDto>
{
    public {EntityName}PostRequestDtoValidator()
    {
        // Add logic how validate DTO
    }
}
```