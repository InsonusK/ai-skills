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