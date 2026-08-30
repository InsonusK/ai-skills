---
name: plateau-v1--class-single-entity-related-controller
description: Class Single{Entity}{Related}Controller in the v1 plateau
whenToUse: when publishing read/update/remove for one specific item inside a parent's sub-collection
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Publish `GET`/`DELETE` (and `PUT` when the related item is itself mutable) for one addressed item inside a sub-collection

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs.create]]

# Naming convention
| use case | class name pattern | route |
| -------- | ------------------- | ----- |
| Relationship instance | `Single{Entity}{Related}Controller` | `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}/{relatedId}` |

# Implementation
```csharp
//Skill: class-single-entity-related-controller
//Plateau: v1
//Version: 20260825140000

[ApiController]
[Route("api/tasks/{taskId}/comments/{commentId}")]
public sealed class SingleTaskCommentController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CommentDto>> Get(int taskId, int commentId, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskCommentByIdQuery(taskId, commentId), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return Ok(result.Value);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int taskId, int commentId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTaskCommentCommand(taskId, commentId), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return NoContent();
    }
}
```

See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs.create]] for the full worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs.create]]

# Rules
MUST NOT:
- Return the parent entity or the full sub-collection from any action here

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs.create]]

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}/{kebab-case-related-plural}/{relatedId}`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs.create]]
