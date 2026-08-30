---
description: Single-entity lifecycle controller — read, update, and delete one identified resource
project_name: "{Module}.Api"
name: "Single{Entity}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/single-entity-controller-cs
---

# Goals
- Publish the three per-instance operations every addressable entity needs: read one, update one, delete one

# Naming convention
| use case | class name pattern | class name | route pattern | route |
| -------- | ------------------- | ---------- | -------------- | ----- |
| Single entity lifecycle | `Single{Entity}Controller` | `SingleTaskController` | `api/{kebab-case-plural}/{id}` | `api/tasks/{id}` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/SingleTaskController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;
using {Module}.Interfaces.DTOs;

namespace {Module}.Api.Controllers;

[ApiController]
[Route("api/tasks/{id}")]
public sealed class SingleTaskController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskByIdQuery(id), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return Ok(result.Value);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Route id and body id do not match." });

        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTaskCommand(id), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return NoContent();
    }
}
```

- `id != command.Id` is the one check that isn't a `Send()` — it's a transport-shape check (route vs. body agreement), not business logic, so it doesn't count against the "exactly one dispatch" rule
- `Update`/`Delete` return `204 No Content`, never the updated resource body — a client that wants to see the result re-`GET`s it

# Rule changes

## MUST
- Route be `api/{kebab-case-plural}/{id}`
- `Update`/`Delete` return `204 No Content` on success
- `Update` reject a route/body id mismatch with `400`, before dispatching

## MUST NOT
- Return the updated entity body from `Update`
- Perform business validation of the route/body id match beyond equality — anything deeper belongs in the command validator

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}`
- [ ] `Get`, `Update`, `Delete` present only for operations the module's commands/queries actually support
- [ ] `Update` checks route id against body id before dispatching
- [ ] `Update`/`Delete` return 204, never 200 with a body
