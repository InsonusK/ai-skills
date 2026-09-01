---
description: Sub-collection controller — list and add items in a related collection scoped to one parent entity
project_name: "{Module}.Api"
name: "{Entity}{Related}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/entity-related-controller-cs
---

# Goals
- Publish a collection that only makes sense scoped to one parent (an entity's comments, attachments, line items) — mirrors `{Entity}Controller`'s create/list shape, one level deeper in the route

# Naming convention
| use case | class name pattern | class name | route pattern | route |
| -------- | ------------------- | ---------- | -------------- | ----- |
| Sub-collection | `{Entity}{Related}Controller` | `TaskCommentsController` | `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}` | `api/tasks/{id}/comments` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/TaskCommentsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;
using {Module}.Interfaces.DTOs;

namespace {Module}.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/comments")]
public sealed class TaskCommentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> List(int taskId, CancellationToken ct)
    {
        var result = await sender.Send(new ListTaskCommentsQuery(taskId), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Add(int taskId, [FromBody] AddTaskCommentCommand command, CancellationToken ct)
    {
        if (taskId != command.TaskId)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Route id and body id do not match." });

        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return CreatedAtAction(
            nameof(SingleTaskCommentController.Get), "SingleTaskComment",
            new { taskId, commentId = result.Value.Id }, result.Value);
    }
}
```

# Rule changes

## MUST
- Route be `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}`
- `Add` return `201 Created` via `CreatedAtAction` targeting `Single{Entity}{Related}Controller.Get`, mirroring `{Entity}Controller.Create`
- Never return the parent entity in the response — this controller's payloads are the related items only

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}`
- [ ] `Add` returns 201 via `CreatedAtAction` targeting `Single{Entity}{Related}Controller.Get`
