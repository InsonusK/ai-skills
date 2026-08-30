---
description: Relationship-instance controller — read, update, or remove one specific item inside a parent's related sub-collection
project_name: "{Module}.Api"
name: "Single{Entity}{Related}Controller.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/single-entity-related-controller-cs
---

# Goals
- Publish the per-instance operations for one item inside a sub-collection — mirrors `Single{Entity}Controller`'s read/update/delete shape, scoped under the parent

# Naming convention
| use case | class name pattern | class name | route pattern | route |
| -------- | ------------------- | ---------- | -------------- | ----- |
| Relationship instance | `Single{Entity}{Related}Controller` | `SingleTaskCommentController` | `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}/{relatedId}` | `api/tasks/{id}/comments/{commentId}` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/SingleTaskCommentController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;
using {Module}.Interfaces.DTOs;

namespace {Module}.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/comments/{commentId}")]
public sealed class SingleTaskCommentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Get(int taskId, int commentId, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskCommentByIdQuery(taskId, commentId), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return Ok(result.Value);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int taskId, int commentId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTaskCommentCommand(taskId, commentId), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        return NoContent();
    }
}
```

`Update` follows the exact same shape as `Single{Entity}Controller.Update` (route/body id check, `204` on success) when the related item is itself mutable — omitted here since not every relationship instance is.

# Rule changes

## MUST
- Route be `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}/{relatedId}`
- `Delete` return `204 No Content`

## MUST NOT
- Return the parent entity or the full sub-collection from any action here — this controller's payload is the one addressed instance

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}/{relatedId}`
- [ ] Only the operations the module's commands/queries actually support are present (`Get`/`Update`/`Delete`, as applicable)
