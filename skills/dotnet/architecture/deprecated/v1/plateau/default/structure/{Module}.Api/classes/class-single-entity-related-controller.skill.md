---
name: class-single-entity-related-controller
description: Relationship instance controller
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]"
---

# Goal
- Handle one specific relationship instance identified by both parent and child IDs
- Route: `/{entity}/{entityId}/{related}/{relatedId}`

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Relationship instance controller | `Single{Entity}{Related}Controller` | `SingleTaskTagController` | `Single{Entity}{Related}Controller.cs` | `SingleTaskTagController.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-single-entity-related-controller
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Api/Controllers/{Entity}/{Related}/Single{Entity}{Related}Controller.cs
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Controllers.{Entity}.{Related};

[ApiController]
[Route("{entity}/{entityId:int}/{related}/{relatedId:int}")]
public sealed class Single{Entity}{Related}Controller : ControllerBase
{
    private readonly ISender _sender;

    public Single{Entity}{Related}Controller(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof({Entity}{Related}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<{Entity}{Related}Dto>> Get(
        int entityId, int relatedId, CancellationToken ct)
    {
        var result = await _sender.Send(new Get{Entity}{Related}Query(entityId, relatedId), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Get{Entity}{Related}Query.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(
        int entityId, int relatedId, CancellationToken ct)
    {
        var result = await _sender.Send(new Remove{Entity}{Related}Command(entityId, relatedId), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Remove{Entity}{Related}Command.")
        };
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Rules
MUST:
	- Named `Single{Entity}{Related}Controller`
	- Route: `[Route("{entity}/{entityId:int}/{related}/{relatedId:int}")]`
MUST NOT:
	- Handle collection-level operations — those belong in `{Entity}{Related}Controller`

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Using this controller for sub-collection list/add operations
- Missing route constraints on IDs

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Check list
- [ ] Named `Single{Entity}{Related}Controller`
- [ ] Route uses `/{entity}/{entityId:int}/{related}/{relatedId:int}`
- [ ] GET dispatches `Get{Entity}{Related}Query`
- [ ] DELETE dispatches `Remove{Entity}{Related}Command`

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Handle one specific relationship instance identified by both parent and child IDs
- [ ] WHEN applied THEN Route: /{entity}/{entityId}/{related}/{relatedId}
- [ ] WHEN verified THEN Named Single{Entity}{Related}Controller
- [ ] WHEN verified THEN Route uses /{entity}/{entityId:int}/{related}/{relatedId:int}
- [ ] WHEN verified THEN GET dispatches Get{Entity}{Related}Query
- [ ] WHEN verified THEN DELETE dispatches Remove{Entity}{Related}Command
- [ ] WHEN naming 'Relationship instance controller' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]]
