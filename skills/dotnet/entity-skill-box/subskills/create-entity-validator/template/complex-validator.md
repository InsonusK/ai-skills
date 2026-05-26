# Template of Complex validator

```csharp
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