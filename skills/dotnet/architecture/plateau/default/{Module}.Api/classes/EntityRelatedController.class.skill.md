---
uid: 3c4d45f7-b43d-4560-af12-5f40fc7df123
name: entityrelatedcontroller-class
description: Sub-collection controller
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication.solution.skill]]"
---

# Goal
- Handle a collection of related entities owned by one parent entity
- Route: `/{entity}/{id}/{related-entity}`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Sub-collection controller | `{Entity}{Related}Controller` | `TaskTagController` | `{Entity}{Related}Controller.cs` | `TaskTagController.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Implementation
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
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Rules
MUST:
	- Named `{Entity}{Related}Controller`
	- Route: `[Route("{entity}/{parentId:int}/{related}")]`
	- `[HttpGet]` lists the sub-collection, `[HttpPost]` adds to it
MUST NOT:
	- Handle relationship instance operations — those belong in `Single{Entity}{Related}Controller`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Anti-patterns
- Inline sub-collection logic instead of dispatching a command/query
- Missing `CreatedAtAction` for POST create

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Check list
- [ ] Named `{Entity}{Related}Controller`
- [ ] Route uses `/{entity}/{parentId:int}/{related}`
- [ ] GET dispatches `Get{Entity}{Relateds}Query`
- [ ] POST dispatches `Add{Entity}{Related}Command`
- [ ] 201 Created uses `CreatedAtAction` pointing to `Single{Entity}{Related}Controller.Get`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

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
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]
