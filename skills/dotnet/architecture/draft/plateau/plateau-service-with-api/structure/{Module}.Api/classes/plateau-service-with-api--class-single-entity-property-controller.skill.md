---
name: class-single-entity-property-controller
description: Class Single{Entity}{Property}Controller in the service-with-api plateau
whenToUse: when a property is worth addressing independently of the full entity — frequently updated, independently authorized, or independently cacheable
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
- Publish read/replace for one property in isolation, without forcing the full-entity `Update`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Naming convention
| use case | class name pattern | route |
| -------- | ------------------- | ----- |
| Addressable property | `Single{Entity}{Property}Controller` | `api/{kebab-case-plural}/{id}/{kebab-case-property}` |

# Implementation
```csharp
//Skill: class-single-entity-property-controller
//Plateau: service-with-api
//Version: 20260825120000

[ApiController]
[Route("api/tasks/{id}/status")]
public sealed class SingleTaskStatusController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskStatusQuery(id), ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskStatusCommand command, CancellationToken ct)
    {
        if (id != command.TaskId) return BadRequest(new ProblemDetails { Status = 400, Title = "Route id and body id do not match." });
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) { var p = result.ToProblemDetails(); return StatusCode(p.Status ?? 500, p); }
        return NoContent();
    }
}
```

Only introduced when a real caller reads/writes this property in isolation — see [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]].

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Rules
MUST NOT:
- Exist for a property nobody updates or reads in isolation

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Check list
- [ ] Introduced only because a real caller needs it in isolation

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]
