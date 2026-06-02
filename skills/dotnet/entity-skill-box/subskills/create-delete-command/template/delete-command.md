---
name: entity-delete-command-template
description: Template of delete command
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - delete-command
---
# Template of delete command

```C#
using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;
using InsonusK.Shared.Command.Interface;
using InsonusK.Shared.Models.Common;
using InsonusK.Shared.Command.Interface.Models;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Commands;

public class {EntityName}DeleteCommand: ICommandWithEntityKeys, IRequest<Result<{EntityName}Response.Single>>, IClientActionTimeStamp
{
    public required string EntityStringId { get; init; }
    public required DateTimeOffset ActionTimeStamp { get; init; } 
    
    public required {EntityName}DeleteRequestDto Payload { get; init; }

    public IReadOnlyCollection<IEntityKey> EntityKeys => new[] { new EntityKey<{EntityName}>(this.EntityStringId, null, false) };  
}
```
