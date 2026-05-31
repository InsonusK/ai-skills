---
name: entity-put-command-template
description: Template of property validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - put-command
---
# Template of property validator

```csharp
using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;
using InsonusK.Shared.Models.Common;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Commands;

public class {EntityName}PutCommand: ICommandWithEntityKeys, IRequest<Result<{EntityName}Response.Single>>, IClientActionTimeStamp
{
    public required string EntityStringId { get; init; }
    public required string Version { get; init; }

    public required {EntityName}PutRequestDto Payload { get; init; }
    public required DateTimeOffset ActionTimeStamp { get; init; } 

    public IReadOnlyCollection<IEntityKey> EntityKeys => new[] { new EntityKey<{EntityName}>(this.EntityStringId, Version, true) };  
}
```