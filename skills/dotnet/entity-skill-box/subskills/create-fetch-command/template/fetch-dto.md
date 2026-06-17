---
name: entity-fetch-dto-template
description: Template of delete dto
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - fetch-command
    - fetch-dto
---
# Template of delete dto

```CSharp
using InsonusK.Shared.Models.Template;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public class {EntityName}FetchRequestDto:IFetchRequest
{
    public int Page  { get; init; } = 0;
    public int PageSize  { get; init; } =0;
}
```