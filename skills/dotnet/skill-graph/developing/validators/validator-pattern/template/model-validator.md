---
name: validator-model-template
description: Template: Model Validator (2.4)
metadata:
  domain: dotnet
  tags:
    - dotnet
    - validator-pattern
    - model-validator
---
# Template: Model Validator (2.4)
*Composer only. No inline logic allowed.*

```csharp
using FluentValidation;

namespace {ProjectNamespace}.Validator.{Model|Entity};

public class {ModelName}Validator : AbstractValidator<{EntityName}>
{   
    public {EntityName}Validator()
    {
        // Level 2.1 or 2.2
        RuleFor(x => x.{Property}).Is{RuleName}(); 

        // Level 2.3
        RuleFor(x => x).SetValidator(new Is{ComplexRule}Validator());
    }
}
```