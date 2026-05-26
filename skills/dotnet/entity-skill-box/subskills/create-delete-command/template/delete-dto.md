# Template of delete dto

```csharp
using InsonusK.Shared.Models.Template;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}PatchRequestDto:IDeleteRequestWithLastStateOptionDto
{
    public bool ReturnLastState { get; init; } = false;
}
```