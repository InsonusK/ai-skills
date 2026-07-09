---
name: class-entity-related-controller
description: Sub-collection controller
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]"
---

# Goal
- Handle a collection of related entities owned by one parent entity
- Route: `/{entity}/{id}/{related-entity}`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Sub-collection controller | `{Entity}{Related}Controller` | `TaskTagController` | `{Entity}{Related}Controller.cs` | `TaskTagController.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-entity-related-controller
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Api/Controllers/{Entity}/{Related}/{Entity}{Related}Controller.cs
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Controllers.{Entity}.{Related};

[ApiController]
[Route("{entity}/{parentId:int}/{related}")]
public sealed class {Entity}{Related}Controller : ControllerBase
{
    private readonly ISender _sender;

    public {Entity}{Related}Controller(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<{Related}Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<{Related}Dto>>> GetAll(
        int parentId, CancellationToken ct)
    {
        var result = await _sender.Send(new Get{Entity}{Relateds}Query(parentId), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Get{Entity}{Relateds}Query.")
        };
    }

    [HttpPost]
    [ProducesResponseType(typeof(Add{Entity}{Related}Result), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Add{Entity}{Related}Result>> Add(
        int parentId,
        [FromBody] Add{Entity}{Related}Request request,
        CancellationToken ct)
    {
        var command = new Add{Entity}{Related}Command(parentId, request.RelatedId);
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(
                nameof(Single{Entity}{Related}Controller.Get),
                "Single{Entity}{Related}",
                new { parentId, relatedId = result.Value.RelatedId },
                result.Value),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Add{Entity}{Related}Command.")
        };
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Rules
MUST:
	- Named `{Entity}{Related}Controller`
	- Route: `[Route("{entity}/{parentId:int}/{related}")]`
	- `[HttpGet]` lists the sub-collection, `[HttpPost]` adds to it
MUST NOT:
	- Handle relationship instance operations — those belong in `Single{Entity}{Related}Controller`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Inline sub-collection logic instead of dispatching a command/query
- Missing `CreatedAtAction` for POST create

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Check list
- [ ] Named `{Entity}{Related}Controller`
- [ ] Route uses `/{entity}/{parentId:int}/{related}`
- [ ] GET dispatches `Get{Entity}{Relateds}Query`
- [ ] POST dispatches `Add{Entity}{Related}Command`
- [ ] 201 Created uses `CreatedAtAction` pointing to `Single{Entity}{Related}Controller.Get`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Handle a collection of related entities owned by one parent entity
- [ ] WHEN applied THEN Route: /{entity}/{id}/{related-entity}
- [ ] WHEN verified THEN Named {Entity}{Related}Controller
- [ ] WHEN verified THEN Route uses /{entity}/{parentId:int}/{related}
- [ ] WHEN verified THEN GET dispatches Get{Entity}{Relateds}Query
- [ ] WHEN verified THEN POST dispatches Add{Entity}{Related}Command
- [ ] WHEN verified THEN 201 Created uses CreatedAtAction pointing to Single{Entity}{Related}Controller.Get
- [ ] WHEN naming 'Sub-collection controller' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]]
