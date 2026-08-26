---
name: plateau-service-with-api--class-entity-controller
description: Class {Entity}Controller in the service-with-api plateau
whenToUse: when publishing the collection-level create/list operations for an entity over REST
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
- Publish `POST` (create) and `GET` (list) for one entity's collection root

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs.create]]

# Naming convention
| use case | class name pattern | route |
| -------- | ------------------- | ----- |
| Collection root | `{Entity}Controller` | `api/{kebab-case-plural}` |

# Implementation
```csharp
//Skill: class-entity-controller
//Plateau: service-with-api
//Version: 20260825120000

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return CreatedAtAction(nameof(SingleTaskController.Get), "SingleTask", new { id = result.Value.Id }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List([FromQuery] ListTasksQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok(result.Value);
    }
}
```

`List` exists only once the module has `solution-query-integration` composed. See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs.create]] for the full rationale.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs.create]]

# Rules
MUST:
- `POST` return 201 via `CreatedAtAction` targeting `Single{Entity}Controller.Get`
- Route be `api/{kebab-case-plural}`
MUST NOT:
- Perform more than one `Send()` per action

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs.create]]

# Check list
- [ ] Route is `api/{kebab-case-plural}`
- [ ] `Create` returns 201 via `CreatedAtAction`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs.create]]
