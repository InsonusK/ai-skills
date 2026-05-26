# Template of delete dto

```csharp
using InsonusK.Shared.Models.Template;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}FetchRequestDto:IFetchRequest
{
    public int Page  { get; init; } = 0;
    public int PageSize  { get; init; } =0;
}
```