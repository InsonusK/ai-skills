---
name: plateau-v1--class-single-entity-controller
description: Class Single{Entity}Controller in the v1 plateau
whenToUse: when publishing read/update/delete for one identified entity over REST
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
- Publish `GET`/`PUT`/`DELETE` for one identified entity

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs.create]]

# Naming convention
| use case | class name pattern | route |
| -------- | ------------------- | ----- |
| Single entity lifecycle | `Single{Entity}Controller` | `api/{kebab-case-plural}/{id}` |

# Implementation
```csharp
//Skill: class-single-entity-controller
//Plateau: v1
//Version: 20260825140000

[ApiController]
[Route("api/tasks/{id}")]
public sealed class SingleTaskController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskByIdQuery(id), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest(new ProblemDetails { Status = 400, Title = "Route id and body id do not match." });
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTaskCommand(id), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return NoContent();
    }
}
```

See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs.create]] for the full worked example with `[ProducesResponseType]` attributes.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs.create]]

# Rules
MUST:
- Route be `api/{kebab-case-plural}/{id}`
- `Update`/`Delete` return 204, never the resource body
- `Update` reject a route/body id mismatch with 400 before dispatching
MUST NOT:
- Return the updated entity body from `Update`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs.create]]

# Check list
- [ ] Route is `api/{kebab-case-plural}/{id}`
- [ ] `Update`/`Delete` return 204

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs.create]]
