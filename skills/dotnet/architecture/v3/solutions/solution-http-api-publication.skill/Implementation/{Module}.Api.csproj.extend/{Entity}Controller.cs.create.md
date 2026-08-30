---
description: Collection-root controller — create a new entity (POST) and list the collection (GET) — the entry point for an entity's lifecycle
project_name: "{Module}.Api"
name: "{Entity}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/entity-controller-cs
---

# Goals
- Publish the two collection-level operations every entity needs: create one, list many
- Give `POST` a `201 Created` response pointing at the newly-created resource, per REST convention

# Naming convention
| use case | class name pattern | class name | route pattern | route |
| -------- | ------------------- | ---------- | -------------- | ----- |
| Collection root | `{Entity}Controller` | `TasksController` | `api/{kebab-case-plural}` | `api/tasks` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/TasksController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;
using {Module}.Interfaces.DTOs;

namespace {Module}.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return CreatedAtAction(
            nameof(SingleTaskController.Get), "SingleTask", new { id = result.Value.Id }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List(
        [FromQuery] ListTasksQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return Ok(result.Value);
    }
}
```

- `List` only exists once `solution-query-integration` gives the module a query to dispatch — a write-only module has `Create` and no `List` action, which is a complete, valid application of this class (see the parent solution's Boundaries).
- `Create`'s `CreatedAtAction` points at `Single{Entity}Controller.Get` by its controller name (`"SingleTask"`, i.e. the class name minus `Controller`) — this is the one place the two controllers are coupled, and it exists precisely so a client can immediately re-fetch what it just created.

# Rule changes

## MUST
- `POST` return `201 Created` via `CreatedAtAction` pointing at `Single{Entity}Controller.Get`, never a bare `200`
- Route be `api/{kebab-case-plural-noun}`
- Every action dispatch exactly one `ISender.Send()`
- `[ProducesResponseType]` declared for every status the dispatched command/query can return

## MUST NOT
- Perform more than one `Send()` per action
- Reference `{Module}.Application`/`{Module}.Domain` or any repository/DbContext type

# Check list
- [ ] Route is `api/{kebab-case-plural}`
- [ ] `Create` returns 201 via `CreatedAtAction` targeting `Single{Entity}Controller.Get`
- [ ] `List` present only when the module has a query to dispatch
- [ ] Every `ResultStatus` the dispatched command/query returns has a `[ProducesResponseType]`
