---
name: entity-validator-complex-template
description: Template of Complex validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - validator
    - complex-validator
---
# Template of Complex validator

```C#
using FluentValidation;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Entity;

public class {EntityName}{RuleName}Validator : AbstractValidator<{{ EntityName | Interface | other }}>
{   
    public {EntityName}{RuleName}Validator()
    {
        RuleFor(x => x). // Validation Logic
    }
}
```