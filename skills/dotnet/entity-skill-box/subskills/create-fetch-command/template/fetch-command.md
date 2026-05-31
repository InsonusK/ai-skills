---
name: entity-fetch-command-template
description: Template of fetch command
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - fetch-command
---
# Template of fetch command

```csharp
using Ardalis.Result;
using MediatR;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Commands;

public class {EntityName}FetchCommand : IRequest<Result<{EntityName}Response.Many>>
{
    public {EntityName}FetchReqestDto Payload { get; init; }
}
```
