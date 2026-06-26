---

name: class-module-api-controller 
description: defines how to implement an ASP.NET Core Controller as a thin MediatR adapter for entity lifecycle operations 
domain: skill 
type: class 
tags:
- skill/pattern/class
- dotnet
- api
- aspnet-core
- controller 
triggers:
- implement controller
- add controller endpoint
- entity lifecycle endpoint

---

# Goal

Define how to implement an ASP.NET Core Controller that handles entity lifecycle HTTP operations. A Controller is a thin adapter — it maps HTTP input to a Command or Query, dispatches it via MediatR, and maps the result to an HTTP response. Nothing else.

# Core Principles

- Controller dispatches exactly one Command or Query per action
- Controller maps `Result<T>` status to HTTP response — no business decisions
- Every possible `ResultStatus` is explicitly handled — unexpected statuses throw
- `If-Match` header decoded here for update commands — see solution-concurrency-control.skill.md
- `ConflictException<T>` caught here for Guid-carrying commands — see solution-guid-resolving.skill.md

# Governed by

- solution-command-handling.skill.md — full pipeline the controller sits at the top of
- solution-guid-resolving.skill.md — ConflictException handling for creation endpoints
- solution-concurrency-control.skill.md — If-Match decoding for update endpoints

# Structure

## Place in csproj

Defined in `csproj-module-api.skill.md`

```
/{ModuleName}.Api
  /Controllers
    /{EntityName}
      {EntityName}Controller.cs
      Single{EntityName}Controller.cs
```

## Naming convention

```
class name:
  rule: controller type prefix + EntityName + Controller suffix
  pattern: {Prefix}{EntityName}Controller
  example: SingleTaskController

file name:
  rule: matches class name exactly
  pattern: {Prefix}{EntityName}Controller.cs
  example: SingleTaskController.cs
```

# Controller Types and Routes

|Type|Class pattern|Route|
|---|---|---|
|Collection|`{Entity}Controller`|`/{entity}`|
|Single entity|`Single{Entity}Controller`|`/{entity}/{id}`|
|Property|`Single{Entity}{Property}Controller`|`/{entity}/{id}/{property}`|
|Sub-resource|`{Entity}{Related}Controller`|`/{entity}/{id}/{related}`|
|Relationship|`Single{Entity}{Related}Controller`|`/{entity}/{entityId}/{related}/{relatedId}`|

# Implementation Example

## Collection Controller

```csharp
[ApiController]
[Route("task")]
public sealed class TaskController : ControllerBase
{
    private readonly ISender _sender;

    public TaskController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateTaskResult>> Create(
        [FromBody] CreateTaskCommand command,
        CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(command, ct);

            return result.Status switch
            {
                ResultStatus.Created => CreatedAtAction(
                    nameof(SingleTaskController.Get),
                    "SingleTask",
                    new { id = result.Value.Id },
                    result.Value),
                ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
                _ => throw new InvalidOperationException(
                    $"Unexpected result status '{result.Status}' for CreateTaskCommand.")
            };
        }
        catch (ConflictException<Result<CreateTaskResult>> ex)
        {
            return Conflict(ex.Existing.Value);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskSummaryDto>>> GetAll(
        [FromQuery] GetTasksQuery query,
        CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTasksQuery.")
        };
    }
}
```

## Single Entity Controller

```csharp
[ApiController]
[Route("task/{id}")]
public sealed class SingleTaskController : ControllerBase
{
    private readonly ISender _sender;

    public SingleTaskController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTaskQuery(id), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTaskQuery.")
        };
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ifMatch))
            return StatusCode(412);

        var versions = ETagEncoder.Decode(ifMatch);
        if (versions is null)
            return StatusCode(412);

        var command = new UpdateTaskCommand(id, request.Title, versions);
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(),
            ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for UpdateTaskCommand.")
        };
    }
}
```

# Rules

MUST:

- Inject `ISender` — never `IMediator` directly
- Every action maps to exactly one Command or Query
- Every `ResultStatus` has explicit handling in the switch
- Unexpected `ResultStatus` throws `InvalidOperationException`
- `[ProducesResponseType]` declared for every possible HTTP status
- `If-Match` checked before dispatch on all update actions — return 412 if missing
- `ConflictException<T>` caught on all creation actions that use `IHasGuid` MUST NOT:
- Contain business logic or domain decisions
- Query DbContext, repositories, or any persistence
- Dispatch more than one MediatR request per action
- Return undocumented HTTP status codes

# Anti-patterns

- `if (result.Value == null) return NotFound()` — use `ResultStatus.NotFound` from handler
- Missing `_ => throw` arm in switch — silent undocumented response returned
- Controller checks user permissions inline — belongs in authorization middleware or handler
- Missing `[ProducesResponseType]` for a status the handler can return

# Checklist

- [ ] Class is `sealed`
- [ ] Only `ISender` injected
- [ ] Every action dispatches exactly one Command or Query
- [ ] Result switch has explicit arm for every expected status
- [ ] Result switch has `_ => throw InvalidOperationException` arm
- [ ] `[ProducesResponseType]` for every HTTP status
- [ ] Update actions check `If-Match` — return 412 if missing or malformed
- [ ] Creation actions catch `ConflictException<T>` if command implements `IHasGuid`

# Unittest TestCases

- [ ] When handler returns expected status Then correct HTTP status returned
- [ ] When handler returns unexpected status Then InvalidOperationException thrown
- [ ] When If-Match missing on update Then 412 returned before MediatR dispatch
- [ ] When ConflictException thrown Then 409 returned with existing entity data

# Relations

- csproj-module-api.skill.md — project this controller lives in
- class-api-minimal-endpoint.skill.md — alternative surface for system operations
- solution-command-handling.skill.md — pipeline this controller dispatches into
- solution-guid-resolving.skill.md — ConflictException caught here
- solution-concurrency-control.skill.md — If-Match decoded here