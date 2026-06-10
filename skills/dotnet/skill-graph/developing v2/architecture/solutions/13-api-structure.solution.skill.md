---
uid: 5c8d2f1a-9e4b-4c7f-b2a6-e8d3f5c1a9b4
order: 13
name: api-structure
description: Defines the API layer structure — entity-centric Controllers as thin MediatR adapters, Minimal API for system operations, controller naming and folder conventions, Result-to-HTTP mapping with ProblemDetails, and App.Host wiring for the API layer
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - aspnet-core
  - api
  - controllers
  - minimal-api
  - cqrs
  - mediatr
triggers:
  - design api endpoint
  - add controller
  - create api layer
  - map result to http response
  - define route
  - thin adapter
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/CollectionController.class.skill|CollectionController.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/SingleEntityController.class.skill|SingleEntityController.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/PropertyController.class.skill|PropertyController.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/SubCollectionController.class.skill|SubCollectionController.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/RelationshipController.class.skill|RelationshipController.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/MinimalApiEndpoints.class.skill|MinimalApiEndpoints.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/ResultExtensions.class.skill|ResultExtensions.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/{Module}.Api.csproj.skill|{Module}.Api.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/10-validation.solution.skill|10-validation.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/11-unit-of-work.solution.skill|11-unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/12-query-handler.solution.skill|12-query-handler.solution.skill]]"
---

# Goal
- Define the API layer as a thin HTTP adapter over MediatR — no business logic, no domain rules, no persistence
- Define five controller types covering all entity lifecycle operations: Collection, SingleEntity, Property, SubCollection, Relationship
- Define Minimal API as the surface for system-level, webhook, batch, and cross-aggregate operations
- Define the `Result<T>` to HTTP status mapping — every `ResultStatus` documented, unexpected statuses throw
- Define `ProblemDetails` as the universal error response shape
- Define controller naming, folder structure, and `[Route]` conventions
- Wire the API layer in App.Host — controller discovery, MediatR registration

# Core Principles
- API layer is a thin HTTP adapter — map input to command/query, dispatch via `ISender`, map result to HTTP response
- Every endpoint dispatches exactly one MediatR command or query — no business logic, no orchestration
- Entity lifecycle operations always use Controllers — system and cross-entity operations use Minimal API
- All error responses use `ProblemDetails` — never raw strings or custom error shapes
- Every `ResultStatus` the handler can return has an explicit `ProducesResponseType` — unexpected statuses throw `InvalidOperationException`
- `ISender` is the only MediatR interface injected into controllers — never `IMediator`
- Controllers reference only `{Module}.Interfaces` — never Application, Domain, or Infrastructure
- API layer never references `IRepository<T>`, `IUnitOfWork`, DbContext, or any domain entity type

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines `{Module}.Api` project boundary and allowed dependencies
- [[02-solution-layer-structure.solution.skill]] — Api references only own Interfaces and BuildingBlocks
- [[09-command-handler.solution.skill]] — `ICommand`, Command records, and Result conventions consumed by controllers
- [[10-validation.solution.skill]] — `Result.Invalid` produced by pipeline, mapped to 400 in controller
- [[11-unit-of-work.solution.skill]] — commit happens transparently in pipeline; controller never calls SaveChanges
- [[12-query-handler.solution.skill]] — `IQuery`, Query records, DTO shapes consumed by controllers

# Requirements
- `Microsoft.AspNetCore.Mvc` — provides `ControllerBase`, `[ApiController]`, `[Route]`, `ActionResult`, `ProblemDetails`
- `MediatR` — provides `ISender` injected into controllers
- `Ardalis.Result` — provides `Result<T>`, `ResultStatus` mapped to HTTP responses

# Template Skill Mutations

## {Module}.Api (.csproj) (extended)

### Project extension

#### Goal
- Own all HTTP endpoint definitions for this module — controllers and Minimal API endpoint groups
- Be the only project that translates HTTP input to MediatR requests and HTTP output from MediatR results

#### Core Principals
- References only `{Module}.Interfaces` — command records, query records, and DTOs live there
- No business logic, validation logic, or persistence logic in any controller action
- One controller type per operation category — five types cover all entity lifecycle scenarios
- Minimal API used exclusively for system, webhook, batch, and cross-aggregate operations

#### Structure

##### Project Structure
```
/{Module}.Api
  /Controllers
    /{Entity}
      {Entity}Controller.cs               ← collection: POST + GET collection
      Single{Entity}Controller.cs         ← single entity: GET + PUT + PATCH + DELETE
      Single{Entity}{Property}Controller.cs ← addressable property: POST + DELETE
      /{RelatedEntity}
        {Entity}{RelatedEntity}Controller.cs       ← sub-collection: GET + POST
        Single{Entity}{RelatedEntity}Controller.cs ← relationship instance: GET + PUT + PATCH + DELETE
  /MinimalApi
    {System}Endpoints.cs
  /Extensions
    ResultExtensions.cs
  {Module}ApiRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Controllers/{Entity}/{Entity}Controller.cs | Collection root — POST create, GET list | CollectionController.class.skill |
| /Controllers/{Entity}/Single{Entity}Controller.cs | Single entity lifecycle — GET, PUT, PATCH, DELETE | SingleEntityController.class.skill |
| /Controllers/{Entity}/Single{Entity}{Property}Controller.cs | Addressable property — POST set, DELETE unset | PropertyController.class.skill |
| /Controllers/{Entity}/{Related}/{Entity}{Related}Controller.cs | Sub-collection — GET list, POST add | SubCollectionController.class.skill |
| /Controllers/{Entity}/{Related}/Single{Entity}{Related}Controller.cs | Relationship instance — GET, PUT, PATCH, DELETE | RelationshipController.class.skill |
| /MinimalApi/{System}Endpoints.cs | System, webhook, batch, cross-aggregate endpoints | MinimalApiEndpoints.class.skill |
| /Extensions/ResultExtensions.cs | ToProblemDetails helper for Result error mapping | ResultExtensions.class.skill |
| {Module}ApiRegistration.cs | Controller and MediatR registration for this module | |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.AspNetCore.Mvc` | `ControllerBase`, `[ApiController]`, `ActionResult`, `ProblemDetails` |
| `MediatR` | `ISender` injected into controllers |
| `Ardalis.Result` | `Result<T>`, `ResultStatus` mapped to HTTP responses |

#### Rules
MUST:
- Every controller action dispatches exactly one `ISender.Send()` call
- Controllers inject `ISender` — never `IMediator`
- Controllers reference only `{Module}.Interfaces` types
- All error responses use `ProblemDetails`
- Every `ResultStatus` the handler can return has an explicit `ProducesResponseType`
- Unexpected `ResultStatus` throws `InvalidOperationException`

MUST NOT:
- Controller action contain business logic, validation, domain rules, or persistence
- Controller reference Application, Domain, Infrastructure, or DbContext
- Controller inject `IRepository<T>` or `IUnitOfWork`
- Multiple `ISender.Send()` calls in one action — except Minimal API system orchestration with explicit justification

---

### Class extension

#### ResultExtensions (created)

##### Goal
- Provide a shared `ToProblemDetails` helper that converts `Result` validation errors and error messages into `ProblemDetails`
- Keep error mapping consistent across all controllers in the module

##### Implementation changes

```csharp
// {Module}.Api/Extensions/ResultExtensions.cs
public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(
        IEnumerable<ValidationError> validationErrors)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        details.Extensions["errors"] = validationErrors
            .GroupBy(e => e.Identifier)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return details;
    }

    public static ProblemDetails ToProblemDetails(IEnumerable<string> errors)
    {
        var details = new ProblemDetails
        {
            Title = "An error occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        details.Extensions["errors"] = errors.ToArray();
        return details;
    }
}
```

---

#### CollectionController (created)

##### Goal
- Handle creation (`POST`) and collection listing/filtering (`GET`) for one entity type
- Route: `/{entity}` — plural-free, kebab-case

##### Core Principals
- `POST /{entity}` → `Create{Entity}Command` → `Result<{Entity}Result>` → 201 Created
- `GET /{entity}` → `Get{Entities}Query` → `Result<IReadOnlyList<{Entity}SummaryDto>>` → 200 OK
- No single-entity operations here — those belong in `Single{Entity}Controller`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Collection controller | `{Entity}Controller` | `TaskController` | `{Entity}Controller.cs` | `TaskController.cs` |

##### Controller surface selection

| Operation | Belongs here | Reason |
| --- | --- | --- |
| Create entity | ✅ POST | Collection root creates |
| List / search entities | ✅ GET | Collection root lists |
| Get single entity | ❌ | Use `Single{Entity}Controller` |
| Update entity | ❌ | Use `Single{Entity}Controller` |
| Delete entity | ❌ | Use `Single{Entity}Controller` |

##### Implementation changes

```csharp
// Task.Api/Controllers/Task/TaskController.cs
[ApiController]
[Route("task")]
public sealed class TaskController : ControllerBase
{
    private readonly ISender _sender;

    public TaskController(ISender sender)
        => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTaskResult>> Create(
        [FromBody] CreateTaskCommand command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(
                nameof(SingleTaskController.Get),
                "SingleTask",
                new { id = result.Value.Id },
                result.Value),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for CreateTaskCommand.")
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<TaskSummaryDto>>> GetAll(
        [FromQuery] GetTasksQuery query,
        CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTasksQuery.")
        };
    }
}
```

##### Rule changes
MUST:
- Named `{Entity}Controller`
- Route attribute `[Route("{entity}")]` — kebab-case, singular noun
- Handle only collection-level operations: POST create, GET list
- `[HttpPost]` maps to `Create{Entity}Command`
- `[HttpGet]` maps to `Get{Entities}Query`

MUST NOT:
- Handle `/{entity}/{id}` routes — those belong in `Single{Entity}Controller`

---

#### SingleEntityController (created)

##### Goal
- Handle the full lifecycle of one entity instance: GET, PUT, PATCH, DELETE, and action verbs
- Route: `/{entity}/{id}`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Single entity controller | `Single{Entity}Controller` | `SingleTaskController` | `Single{Entity}Controller.cs` | `SingleTaskController.cs` |

##### Implementation changes

```csharp
// Task.Api/Controllers/Task/SingleTaskController.cs
[ApiController]
[Route("task/{id:int}")]
public sealed class SingleTaskController : ControllerBase
{
    private readonly ISender _sender;

    public SingleTaskController(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTaskQuery(id), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTaskQuery.")
        };
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTaskCommand(id, request.Title);
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for UpdateTaskCommand.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteTaskCommand(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for DeleteTaskCommand.")
        };
    }
}
```

##### Rule changes
MUST:
- Named `Single{Entity}Controller`
- Route attribute `[Route("{entity}/{id:int}")]`
- Handle GET single, PUT, PATCH, DELETE, and domain action verbs on `/{entity}/{id}`

---

#### PropertyController (created)

##### Goal
- Handle setting and unsetting one addressable boolean or optional property on an entity
- Route: `/{entity}/{id}/{property-name}` — kebab-case property name

##### Core Principals
- `POST /{entity}/{id}/{property}` → set the property → `Set{Entity}{Property}Command`
- `DELETE /{entity}/{id}/{property}` → unset/clear the property → `Unset{Entity}{Property}Command`
- Used when a property has meaningful set/unset semantics — e.g. `is-complete`, `is-archived`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property controller | `Single{Entity}{Property}Controller` | `SingleTaskIsCompleteController` | `Single{Entity}{Property}Controller.cs` | `SingleTaskIsCompleteController.cs` |

##### Implementation changes

```csharp
// Task.Api/Controllers/Task/SingleTaskIsCompleteController.cs
[ApiController]
[Route("task/{id:int}/is-complete")]
public sealed class SingleTaskIsCompleteController : ControllerBase
{
    private readonly ISender _sender;

    public SingleTaskIsCompleteController(ISender sender)
        => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new SetTaskIsCompleteCommand(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for SetTaskIsCompleteCommand.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Unset(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new UnsetTaskIsCompleteCommand(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for UnsetTaskIsCompleteCommand.")
        };
    }
}
```

##### Rule changes
MUST:
- Named `Single{Entity}{Property}Controller`
- Route: `[Route("{entity}/{id:int}/{property-name}")]` — property name in kebab-case
- `[HttpPost]` sets the property, `[HttpDelete]` unsets it

---

#### SubCollectionController (created)

##### Goal
- Handle a collection of related entities owned by one parent entity
- Route: `/{entity}/{id}/{related-entity}`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Sub-collection controller | `{Entity}{Related}Controller` | `TaskTagController` | `{Entity}{Related}Controller.cs` | `TaskTagController.cs` |

##### Implementation changes

```csharp
// Task.Api/Controllers/Task/Tag/TaskTagController.cs
[ApiController]
[Route("task/{taskId:int}/tag")]
public sealed class TaskTagController : ControllerBase
{
    private readonly ISender _sender;

    public TaskTagController(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<TaskTagDto>>> GetAll(
        int taskId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTaskTagsQuery(taskId), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTaskTagsQuery.")
        };
    }

    [HttpPost]
    [ProducesResponseType(typeof(AddTaskTagResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddTaskTagResult>> Add(
        int taskId,
        [FromBody] AddTaskTagRequest request,
        CancellationToken ct)
    {
        var command = new AddTaskTagCommand(taskId, request.TagId);
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(
                nameof(SingleTaskTagController.Get),
                "SingleTaskTag",
                new { taskId, tagId = result.Value.TagId },
                result.Value),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for AddTaskTagCommand.")
        };
    }
}
```

##### Rule changes
MUST:
- Named `{Entity}{Related}Controller`
- Route: `[Route("{entity}/{parentId:int}/{related}")]`
- `[HttpGet]` lists the sub-collection, `[HttpPost]` adds to it

---

#### RelationshipController (created)

##### Goal
- Handle one specific relationship instance identified by both parent and child IDs
- Route: `/{entity}/{entityId}/{related}/{relatedId}`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Relationship instance controller | `Single{Entity}{Related}Controller` | `SingleTaskTagController` | `Single{Entity}{Related}Controller.cs` | `SingleTaskTagController.cs` |

##### Implementation changes

```csharp
// Task.Api/Controllers/Task/Tag/SingleTaskTagController.cs
[ApiController]
[Route("task/{taskId:int}/tag/{tagId:int}")]
public sealed class SingleTaskTagController : ControllerBase
{
    private readonly ISender _sender;

    public SingleTaskTagController(ISender sender)
        => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(TaskTagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskTagDto>> Get(
        int taskId, int tagId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTaskTagQuery(taskId, tagId), ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for GetTaskTagQuery.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(
        int taskId, int tagId, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveTaskTagCommand(taskId, tagId), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for RemoveTaskTagCommand.")
        };
    }
}
```

##### Rule changes
MUST:
- Named `Single{Entity}{Related}Controller`
- Route: `[Route("{entity}/{entityId:int}/{related}/{relatedId:int}")]`

---

#### MinimalApiEndpoints (created)

##### Goal
- Group system-level, webhook, batch, and cross-aggregate endpoints outside the entity-centric controller model
- Use `IEndpointRouteBuilder` extension methods — one class per system concern

##### Core Principals
- Use Minimal API only when the operation does not belong to a single entity lifecycle
- Still dispatches exactly one MediatR command or query per endpoint — same dispatch rule as controllers
- Groups organized by system concern — not by entity

##### API surface selection decision table

| Operation type | API surface | Reason |
| --- | --- | --- |
| Create entity | Controller | Entity lifecycle |
| Get / list entity | Controller | Entity lifecycle |
| Update / delete entity | Controller | Entity lifecycle |
| Set / unset entity property | Controller | Entity lifecycle |
| Add / remove sub-collection item | Controller | Entity lifecycle |
| Webhook receiver | Minimal API | External integration |
| Health check | Minimal API | Infrastructure |
| Batch import / export | Minimal API | Cross-aggregate operation |
| System sync job trigger | Minimal API | System orchestration |
| Cross-aggregate read (no single owner) | Minimal API | No single entity owner |

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| System endpoint group | `{System}Endpoints` | `WebhookEndpoints` | `{System}Endpoints.cs` | `WebhookEndpoints.cs` |

##### Implementation changes

```csharp
// Task.Api/MinimalApi/WebhookEndpoints.cs
public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/github", async (
            [FromBody] GitHubWebhookPayload payload,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ProcessGitHubWebhookCommand(payload.EventType, payload.Data), ct);

            return result.Status switch
            {
                ResultStatus.NoContent => Results.NoContent(),
                ResultStatus.Invalid => Results.BadRequest(
                    ResultExtensions.ToProblemDetails(result.ValidationErrors)),
                ResultStatus.Error => Results.Problem(
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status500InternalServerError),
                _ => throw new InvalidOperationException(
                    $"Unexpected result status '{result.Status}' for ProcessGitHubWebhookCommand.")
            };
        });

        return app;
    }
}
```

##### Rule changes
MUST:
- Use only for non-entity-lifecycle operations — system, webhook, batch, cross-aggregate
- Each Minimal API endpoint still dispatches exactly one MediatR command or query
- All error responses use `Results.Problem` or `Results.BadRequest` with `ProblemDetails`
- Unexpected `ResultStatus` throws `InvalidOperationException`

MUST NOT:
- Replace entity-lifecycle Controllers with Minimal API — controllers are mandatory for entity operations

---

### Class extension

#### ResultStatus to HTTP mapping (reference)

The complete standard mapping between `Ardalis.Result.ResultStatus` and HTTP responses:

| ResultStatus | HTTP Status | Response body | `ProducesResponseType` |
| --- | --- | --- | --- |
| `Ok` | 200 OK | Typed DTO | `typeof(TDto), 200` |
| `Created` | 201 Created | Typed DTO via `CreatedAtAction` | `typeof(TDto), 201` |
| `NoContent` | 204 No Content | Empty | `204` |
| `Invalid` | 400 Bad Request | `ProblemDetails` with field errors | `typeof(ProblemDetails), 400` |
| `NotFound` | 404 Not Found | `ProblemDetails` | `typeof(ProblemDetails), 404` |
| `Conflict` | 409 Conflict | `ProblemDetails` | `typeof(ProblemDetails), 409` |
| `Error` | 500 Internal Server Error | `ProblemDetails` | `typeof(ProblemDetails), 500` |
| Any other | — | throw `InvalidOperationException` | — |

Every controller action must:
1. Declare a `[ProducesResponseType]` for every `ResultStatus` the handler can return
2. Map every declared status in the `switch` expression
3. Throw `InvalidOperationException` in the `_` (default) arm

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register controllers from all module Api assemblies
- Register Minimal API endpoint groups
- Configure ASP.NET Core JSON and ProblemDetails middleware

#### Structure

##### Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs    ← solution 08 + 11
    PipelineRegistration.cs      ← solutions 09–11
    ApiRegistration.cs
  Program.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/ApiRegistration.cs | Controller registration and middleware configuration | |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.AspNetCore` | `AddControllers`, `MapControllers`, `AddProblemDetails` |

---

### Class extension

#### ApiRegistration (created)

##### Goal
- Register controllers from all module Api assemblies via `AddControllers` with assembly parts
- Configure `ProblemDetails` as the standard error response format

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| API DI registration | `ApiRegistration` | `ApiRegistration` | `ApiRegistration.cs` | `ApiRegistration.cs` |

##### Implementation changes

```csharp
// App.Host/DependencyInjection/ApiRegistration.cs
public static class ApiRegistration
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(TaskController).Assembly)
            .AddApplicationPart(typeof(TimeLogController).Assembly)
            .AddApplicationPart(typeof(UserController).Assembly);

        services.AddProblemDetails();

        return services;
    }
}
```

#### Program.cs (extended)

##### Goal
- Wire API registration and map controllers and Minimal API endpoint groups in the request pipeline

##### Implementation changes

```csharp
// App.Host/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddPipeline()
    .AddRepositories()
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration)
    .RegisterAppQueries();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

// Minimal API endpoint groups
app.MapWebhookEndpoints();

app.Run();
```

##### Rule changes
MUST:
- `UseExceptionHandler()` registered before `MapControllers()` — unhandled exceptions produce `ProblemDetails`
- All module Api assemblies added as application parts
- All Minimal API endpoint groups mapped explicitly

---

# Rules

MUST:
- API layer is a thin HTTP adapter — map input, dispatch once, map output
- Every controller action dispatches exactly one `ISender.Send()` call
- Entity lifecycle operations use Controllers — system/webhook/batch use Minimal API
- Controllers inject `ISender` — never `IMediator`
- All error responses use `ProblemDetails`
- Every `ResultStatus` handler can return has an explicit `[ProducesResponseType]`
- Unexpected `ResultStatus` throws `InvalidOperationException` in `switch` default arm
- Controller naming follows the five-type model: `{Entity}`, `Single{Entity}`, `Single{Entity}{Property}`, `{Entity}{Related}`, `Single{Entity}{Related}`
- Routes use kebab-case, singular nouns, `int` route constraints for IDs

MUST NOT:
- Controller action contain business logic, validation, domain rules, or persistence
- Controller reference Application, Domain, Infrastructure, or DbContext
- Controller inject `IRepository<T>` or `IUnitOfWork`
- Minimal API replace entity-lifecycle controllers
- Undocumented HTTP responses returned — every response shape declared in `ProducesResponseType`

SHOULD:
- `[Route]` use `{entity}` singular noun — not plural
- `CreatedAtAction` reference the `Single{Entity}Controller.Get` method for 201 responses

# Anti-patterns
- Business logic in controller action: `if (task.IsComplete) return Conflict(...)` — belongs in domain
- Multiple `_sender.Send()` calls in one controller action without explicit system-level justification
- Returning 200 for a create operation — use 201 with `CreatedAtAction`
- Missing `[ProducesResponseType]` for a `ResultStatus` the handler can return — undocumented response
- Swallowing unexpected `ResultStatus` with a fallback 500 — throw `InvalidOperationException`
- Using `IMediator` instead of `ISender` — `ISender` is the correct interface for request dispatch
- Minimal API used for entity CRUD — entity lifecycle belongs in typed controllers

# Check list
- [ ] Each module has `/Controllers` and optionally `/MinimalApi` folders in `{Module}.Api`
- [ ] Controller naming follows five-type model
- [ ] Each controller route uses kebab-case singular noun
- [ ] Every controller action dispatches exactly one `ISender.Send()`
- [ ] `ISender` injected — never `IMediator`
- [ ] All error responses use `ProblemDetails` via `ResultExtensions`
- [ ] Every `ResultStatus` handler can return has `[ProducesResponseType]`
- [ ] `switch` default arm throws `InvalidOperationException`
- [ ] 201 Created responses use `CreatedAtAction` pointing to `Single{Entity}Controller.Get`
- [ ] Minimal API used only for non-entity-lifecycle operations
- [ ] All module Api assemblies added as application parts in App.Host
- [ ] `UseExceptionHandler()` registered before `MapControllers()`
- [ ] `AddProblemDetails()` registered in DI

# Unittest TestCases
- [ ] When handler returns `Result.Created` Then controller returns 201 with `Location` header
- [ ] When handler returns `Result.NotFound` Then controller returns 404 with `ProblemDetails` body
- [ ] When handler returns `Result.Invalid` Then controller returns 400 with field-level error details
- [ ] When handler returns `Result.Conflict` Then controller returns 409 with `ProblemDetails` body
- [ ] When handler returns `Result.NoContent` Then controller returns 204 with empty body
- [ ] When handler returns unexpected `ResultStatus` Then controller throws `InvalidOperationException`
- [ ] When `POST /{entity}` called Then `Create{Entity}Command` dispatched via `ISender`
- [ ] When `GET /{entity}/{id}` called Then `Get{Entity}Query` dispatched via `ISender`
- [ ] When `DELETE /{entity}/{id}` called Then `Delete{Entity}Command` dispatched via `ISender`
