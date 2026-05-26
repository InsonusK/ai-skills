# Template of post dto validator

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Models;

public class {EntityName}PutRequestDtoValidator : AbstractValidator<{EntityName}PutRequestDto>
{
    public {EntityName}PutRequestDtoValidator()
    {
        // Add logic how validate DTO
    }
}
```