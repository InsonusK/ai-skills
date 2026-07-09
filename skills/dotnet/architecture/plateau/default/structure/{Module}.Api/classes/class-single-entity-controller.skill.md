---
name: class-single-entity-controller
description: Single entity lifecycle controller
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
---

# Goal
- Handle the full lifecycle of one entity instance: GET, PUT, PATCH, DELETE, and action verbs
- Route: `/{entity}/{id}`
- Encode entity `Version` into `ETag` response header on every GET for a mutable entity
- Decode `If-Match` request header and return 412 before dispatch if missing or malformed
- Populate `Versions` on the update command from the decoded `If-Match` value

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Core Principles
- Apply ONE plateau template per class
- ETag format: `"<base64>"` — surrounding double quotes are part of the HTTP ETag format
- `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolverFactory` keys exactly
- If `If-Match` missing or `ETagEncoder.Decode` returns null → return `StatusCode(412)` immediately, before `_sender.Send()`
- `Versions` passed directly as command constructor argument — no manual construction in controller

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single entity controller | `Single{Entity}Controller` | `SingleTaskController` | `Single{Entity}Controller.cs` | `SingleTaskController.cs` |
| Single entity controller | `Single{Entity}Controller` | `Single{Entity}Controller` | `Single{Entity}Controller.cs` | `Single{Entity}Controller.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-single-entity-controller
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Api/Controllers/{Entity}/Single{Entity}Controller.cs
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Controllers.{Entity};

[ApiController]
[Route("{entity}/{id:int}")]
public sealed class Single{Entity}Controller : ControllerBase
{
    private readonly ISender _sender;

    public Single{Entity}Controller(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<{Entity}Dto>> Get(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new Get{Entity}Query(id), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Get{Entity}Query.")
        };
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] Update{Entity}Request request,
        CancellationToken ct)
    {
        var command = new Update{Entity}Command(id, request.Title);
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Update{Entity}Command.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new Delete{Entity}Command(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Delete{Entity}Command.")
        };
    }
}
```

GET extended with ETag encoding:

```csharp
// {Module}.Api/Controllers/{Entity}/Single{Entity}Controller.cs
[HttpGet]
[ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<{Entity}Dto>> Get(int id, CancellationToken ct)
{
    var result = await _sender.Send(new Get{Entity}Query(id), ct);

    return result.Status switch
    {
        ResultStatus.Ok => BuildOkWithETag(result.Value, id),
        ResultStatus.NotFound => NotFound(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Error => StatusCode(
            StatusCodes.Status500InternalServerError,
            ResultExtensions.ToProblemDetails(result.Errors)),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}' for Get{Entity}Query.")
    };
}

private OkObjectResult BuildOkWithETag({Entity}Dto dto, int id)
{
    var etag = ETagEncoder.Encode(new()
    {
        ["{Entity}"] = new() { [id] = dto.Version }
    });
    Response.Headers.ETag = $"\"{etag}\"";
    return Ok(dto);
}
```

PUT extended with If-Match guard and Versions population:

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Update(
    int id,
    [FromBody] Update{Entity}Request request,
    [FromHeader(Name = "If-Match")] string? ifMatch,
    CancellationToken ct)
{
    // 412 — If-Match not supplied or malformed
    if (string.IsNullOrEmpty(ifMatch))
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var versions = ETagEncoder.Decode(ifMatch);
    if (versions is null)
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var command = new Update{Entity}Command(id, request.Title, versions);
    var result = await _sender.Send(command, ct);

    return result.Status switch
    {
        ResultStatus.NoContent => NoContent(),
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
            $"Unexpected result status '{result.Status}' for Update{Entity}Command.")
    };
}
```

> **Note:** `{Entity}Dto` must include `Version` as a property so the controller can encode it into the ETag:
> ```csharp
> public record {Entity}Dto(int Id, string Title, string Status, int AssigneeId, uint Version);
> ```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Rules
MUST:
	- Named `Single{Entity}Controller`
	- Route attribute `[Route("{entity}/{id:int}")]`
	- Handle GET single, PUT, PATCH, DELETE, and domain action verbs on `/{entity}/{id}`
	- GET for mutable entity sets `Response.Headers.ETag` with encoded versions
	- PUT/PATCH checks `If-Match` presence — returns 412 immediately if missing
	- PUT/PATCH calls `ETagEncoder.Decode` — returns 412 if result is null
	- `Versions` passed to command from decoded `If-Match` — never constructed in controller
	- 412 added to `[ProducesResponseType]` on all PUT/PATCH endpoints for mutable entities
	- DTO returned by GET for mutable entity includes `Version` field
MUST NOT:
	- Handle collection-level operations — those belong in `{Entity}Controller`
	- GET for immutable entity set ETag header — immutable entities have no version
	- `Versions` hardcoded or constructed in controller — always from decoded `If-Match`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Returning 200 for update/delete — use 204 NoContent
- Missing `[ProducesResponseType]` for NotFound
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- Controller returns 400 for missing `If-Match` — 412 Precondition Failed is correct

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Check list
- [ ] Named `Single{Entity}Controller`
- [ ] Route uses `/{entity}/{id:int}`
- [ ] GET dispatches `Get{Entity}Query`
- [ ] PUT dispatches `Update{Entity}Command`
- [ ] DELETE dispatches `Delete{Entity}Command`
- [ ] Update/delete return 204 NoContent on success
- [ ] GET sets `Response.Headers.ETag`
- [ ] PUT/PATCH checks `If-Match`
- [ ] 412 returned if `If-Match` missing or malformed
- [ ] `Versions` passed to command from decoded `If-Match`
- [ ] 412 declared in `[ProducesResponseType]`
- [ ] DTO includes `Version` field
- [ ] `switch` default arm throws `InvalidOperationException`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Handle the full lifecycle of one entity instance: GET, PUT, PATCH, DELETE, and action verbs
- [ ] WHEN applied THEN Route: /{entity}/{id}
- [ ] WHEN verified THEN Named Single{Entity}Controller
- [ ] WHEN verified THEN Route uses /{entity}/{id:int}
- [ ] WHEN verified THEN GET dispatches Get{Entity}Query
- [ ] WHEN verified THEN PUT dispatches Update{Entity}Command
- [ ] WHEN verified THEN DELETE dispatches Delete{Entity}Command
- [ ] WHEN verified THEN Update/delete return 204 NoContent on success
- [ ] WHEN naming 'Single entity controller' THEN pattern matches convention
- [ ] WHEN applied THEN Encode entity Version into ETag response header on every GET for a mutable entity
- [ ] WHEN applied THEN Decode If-Match request header and return 412 before dispatch if missing or malformed
- [ ] WHEN applied THEN Populate Versions on the update command from the decoded If-Match value
- [ ] WHEN applied THEN ETag format: "<base64>" — surrounding double quotes are part of the HTTP ETag format
- [ ] WHEN applied THEN `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolverFactory` keys exactly
- [ ] WHEN applied THEN If If-Match missing or ETagEncoder.Decode returns null → return StatusCode(412) immediately, before _sender.Send()
- [ ] WHEN applied THEN Versions passed directly as command constructor argument — no manual construction in controller
- [ ] WHEN verified THEN GET sets Response.Headers.ETag
- [ ] WHEN verified THEN PUT/PATCH checks If-Match
- [ ] WHEN verified THEN 412 returned if If-Match missing or malformed
- [ ] WHEN verified THEN Versions passed to command from decoded If-Match
- [ ] WHEN verified THEN 412 declared in [ProducesResponseType]
- [ ] WHEN verified THEN DTO includes Version field
- [ ] WHEN verified THEN switch default arm throws InvalidOperationException

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend|Single{Entity}Controller.cs]]
