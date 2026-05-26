# Template of get command

```csharp
using Ardalis.Result;
using MediatR;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;
using InsonusK.Shared.Command.Interface;
using InsonusK.Shared.Command.Interface.Models;
using System.Collections.Generic;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Commands;

public class {EntityName}GetCommand : ICommandWithEntityKeys, IRequest<Result<{EntityName}Response.Single>>
{
    public required string EntityStringId { get; init; }
    public IReadOnlyCollection<IEntityKey> EntityKeys => new[] { new EntityKey<{EntityName}>(this.EntityStringId) };
}
```
