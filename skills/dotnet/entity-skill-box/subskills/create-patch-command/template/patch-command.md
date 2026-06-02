---
name: entity-patch-command-template
description: Template of patch command
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - patch-command
---
# Template of patch command

```C#
using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;
using InsonusK.Shared.Models.Common;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Commands;

public class {EntityName}PatchCommand: ICommandWithEntityKeys, IRequest<Result<{EntityName}Response.Single>>, IClientActionTimeStamp
{
    public required string EntityStringId { get; init; }
    public required string Version { get; init; }

    public required {EntityName}PatchRequestDto Payload { get; init; }
    public required DateTimeOffset ActionTimeStamp { get; init; } 

    public IReadOnlyCollection<IEntityKey> EntityKeys => new[] { new EntityKey<{EntityName}>(this.EntityStringId, Version, true) };  
}
```
