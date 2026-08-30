---
description: Single entity lifecycle controller
project_name: "{Module}.Api"
name: "Single{Entity}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/single-entity-controller-cs
---

# Goals
- Handle the full lifecycle of one entity instance: GET, PUT, PATCH, DELETE, and action verbs
- Route: `/{entity}/{id}`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Single entity controller | `Single{Entity}Controller` | `SingleTaskController` | `Single{Entity}Controller.cs` | `SingleTaskController.cs` |

# Implementation changes

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
# Rule changes

## MUST
- Named `Single{Entity}Controller`
- Route attribute `[Route("{entity}/{id:int}")]`
- Handle GET single, PUT, PATCH, DELETE, and domain action verbs on `/{entity}/{id}`
- Every controller action dispatches exactly one `ISender.Send()` call
- Entity lifecycle operations use Controllers — system/webhook/batch use Minimal API
- Controllers inject `ISender` — never `IMediator`
- Controller naming follows the five-type model: `{Entity}`, `Single{Entity}`, `Single{Entity}{Property}`, `{Entity}{Related}`, `Single{Entity}{Related}`
- Routes use kebab-case, singular nouns, `int` route constraints for IDs

## SHOULD
- `CreatedAtAction` reference the `Single{Entity}Controller.Get` method for 201 responses
- `[Route]` use `{entity}` singular noun — not plural

## MUST NOT
- Handle collection-level operations — those belong in `{Entity}Controller`
- Controller action contain business logic, validation, domain rules, or persistence
- Controller reference Application, Domain, Infrastructure, or DbContext
- Controller inject `IRepository<T>` or `IUnitOfWork`
- Minimal API replace entity-lifecycle controllers

# Anti-patterns
- Returning 200 for update/delete — use 204 NoContent
- Missing `[ProducesResponseType]` for NotFound

# Check list
- [ ] Named `Single{Entity}Controller`
- [ ] Route uses `/{entity}/{id:int}`
- [ ] GET dispatches `Get{Entity}Query`
- [ ] PUT dispatches `Update{Entity}Command`
- [ ] DELETE dispatches `Delete{Entity}Command`
- [ ] Update/delete return 204 NoContent on success

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
