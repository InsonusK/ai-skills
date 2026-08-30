---
description: Addressable-property controller — read or replace one property of an entity, without touching the rest of it
project_name: "{Module}.Api"
name: "Single{Entity}{Property}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/single-entity-property-controller-cs
---

# Goals
- Give a property worth addressing on its own (frequently updated, independently authorized, or independently cacheable) its own narrow route, instead of forcing every change through the full-entity `Update`

# Core Principles
- Only introduce this controller for a property genuinely worth addressing independently — most properties change through `Single{Entity}Controller.Update` and never need this. Adding one speculatively, for a property nobody updates in isolation, is the anti-pattern this class exists to avoid

# Naming convention
| use case | class name pattern | class name | route pattern | route |
| -------- | ------------------- | ---------- | -------------- | ----- |
| Addressable property | `Single{Entity}{Property}Controller` | `SingleTaskStatusController` | `api/{kebab-case-plural}/{id}/{kebab-case-property}` | `api/tasks/{id}/status` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/SingleTaskStatusController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Controllers;

[ApiController]
[Route("api/tasks/{id}/status")]
public sealed class SingleTaskStatusController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskStatusQuery(id), ct);
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskStatusCommand command, CancellationToken ct)
    {
        if (id != command.TaskId)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Route id and body id do not match." });

        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return NoContent();
    }
}
```

# Rule changes

## MUST
- Route be `api/{kebab-case-plural}/{id}/{kebab-case-property}`
- `Update` reject a route/body id mismatch with `400`, before dispatching, same as `Single{Entity}Controller`

## MUST NOT
- Exist for a property that isn't independently updated or read anywhere in the module — see Core Principles

# Check list
- [ ] Introduced only because a real caller reads or writes this property in isolation from the rest of the entity
- [ ] Route is `api/{kebab-case-plural}/{id}/{kebab-case-property}`
