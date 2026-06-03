---
name: entity-get-handler-template
description: Template of get handler
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - get-command
    - get-handler
---
# Template of get handler

```CSharp
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using InsonusK.Shared.Command.Interfaces;
using MediatR;
using {ProjectNamespace}.Entities.{EntityName}Entity.Commands;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Handlers;

public class {EntityName}GetHandler : IRequestHandler<{EntityName}GetCommand, Result<{EntityName}Response.Single>>
{
    private readonly ICommandContextSource _commandContextSource;

    public {EntityName}GetHandler(
        ICommandContextSource commandContextSource)
    {
        _commandContextSource = commandContextSource;
    }

    public async Task<Result<{EntityName}Response.Single>> Handle({EntityName}GetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContextSource.GetForAsync<{EntityName}>(request, cancellationToken);

        var response = {EntityName}Response.Single.FromEntity(entity);
        return Result.Success(response);
    }
}
```
