---
name: validator-complex-template
description: Template: Complex Validator (2.3)
metadata:
  domain: dotnet
  tags:
    - dotnet
    - validator-pattern
    - complex-validator
---
# Template: Complex Validator (2.3)
*Use for rules involving multiple properties.*

```csharp
using FluentValidation;

namespace {ProjectNamespace}.Validator.Models;

public class Is{Rule}Validator : AbstractValidator<{{ EntityName | Interface | other }}>
{   
    public static string Code = "IsNot{Rule}"
    public Is{RuleName}Validator()
    {
        RuleFor(x => x). 
            // Validation Logic
            .WithErrorCode(Code)
            .WithMessage("{{ error description }}");
    }
}
```