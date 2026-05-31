---
name: entity-validator-model-template
description: Template of entity validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - validator
    - entity-validator
---
# Template of entity validator

```csharp
using FluentValidation;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Entity;

public class {EntityName}Validator : AbstractValidator<{EntityName}>
{   
    public {EntityName}Validator()
    {
        RuleFor(x => x.Name). // Use property validators

        RuleFor(x => x). // Use sub validators for complex rules
    }
}
```