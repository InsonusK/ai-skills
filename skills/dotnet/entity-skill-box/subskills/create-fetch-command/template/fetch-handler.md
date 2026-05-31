---
name: entity-fetch-handler-template
description: Template of fetch handler
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - fetch-command
    - fetch-handler
---
# Template of fetch handler

```csharp
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using {ProjectNamespace}.Entities.{EntityName}Entity.Commands;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;
using InsonusK.Shared.DataBase.Spec;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Handlers;

public class {EntityName}FetchHandler : IRequestHandler<{EntityName}FetchCommand, Result<{EntityName}Response.Many>>
{
    private readonly IReadRepositoryBase<{EntityName}> _repository;

    public {EntityName}FetchHandler(IReadRepositoryBase<{EntityName}> repository)
    {
        _repository = repository;
    }

    public async Task<Result<{EntityName}Response.Many>> Handle({EntityName}FetchCommand request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAsync(new FetchSpec(request.Payload), cancellationToken);
        
        var response = {EntityName}Response.Many.FromEntity(entities);
        return Result.Success(response);
    }
}
```
