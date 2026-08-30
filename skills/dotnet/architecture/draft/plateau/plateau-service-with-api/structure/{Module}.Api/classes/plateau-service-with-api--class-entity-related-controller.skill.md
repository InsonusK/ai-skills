---
name: plateau-service-with-api--class-entity-related-controller
description: Class {Entity}{Related}Controller in the service-with-api plateau
whenToUse: when publishing a sub-collection scoped to one parent entity — comments, attachments, line items
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Publish list/add for a collection that only makes sense scoped to one parent

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Naming convention
| use case | class name pattern | route |
| -------- | ------------------- | ----- |
| Sub-collection | `{Entity}{Related}Controller` | `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}` |

# Implementation
```csharp
//Skill: class-entity-related-controller
//Plateau: service-with-api
//Version: 20260825120000

[ApiController]
[Route("api/tasks/{taskId}/comments")]
public sealed class TaskCommentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> List(int taskId, CancellationToken ct)
    {
        var result = await sender.Send(new ListTaskCommentsQuery(taskId), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> Add(int taskId, [FromBody] AddTaskCommentCommand command, CancellationToken ct)
    {
        if (taskId != command.TaskId) return BadRequest(new ProblemDetails { Status = 400, Title = "Route id and body id do not match." });
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return CreatedAtAction(nameof(SingleTaskCommentController.Get), "SingleTaskComment", new { taskId, commentId = result.Value.Id }, result.Value);
    }
}
```

See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]] for the full worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Rules
MUST:
- `Add` return 201 via `CreatedAtAction` targeting `Single{Entity}{Related}Controller.Get`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}`
- [ ] `Add` returns 201 via `CreatedAtAction`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs.create]]
