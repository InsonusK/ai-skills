---
name: api-structure
description: Defines how to design ASP.NET Core API surfaces as thin MediatR adapters using entity-centric Controllers and Minimal API only for system or cross-entity operations.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - aspnet-core
    - api
    - controllers
    - minimal-api
    - cqrs
    - mediatr
    - vertical-slice
---

# Goal

Design or review an ASP.NET Core API structure where every HTTP operation is assigned to the correct API surface and every endpoint remains a thin adapter over MediatR.

The result is measurable: each endpoint has an explicit route, HTTP verb, owning Controller or Minimal API group, matching Command or Query, and a response contract.

# Input data

## Required

- Domain entity or relationship name, for example `Task`, `TaskTag`, or `Task.IsComplete`.
- Operation type: collection, single entity, property, related collection, relationship instance, system operation, webhook, batch job, or cross-aggregate operation.
- HTTP route and verb.
- MediatR request type
  - Command for change data
  - Query for read data
- Request and response DTO names.

## Optional

- Existing folder structure.
- Existing Controller or Minimal API naming conventions.
- Authentication, authorization, versioning, or tenant route requirements.
- Error response conventions.
- Examples of nearby endpoints that must remain consistent.

# Rules

## Core Principle

The API layer is a thin HTTP adapter over MediatR.

It must **not** contain:

- Business logic.
- Validation logic.
- Persistence logic, including `DbContext`, EF Core queries, or repositories.
- Domain rules, calculations, state transitions, or orchestration decisions.

It must only:

- Map HTTP input to DTOs, Commands, or Queries.
- Dispatch exactly one MediatR request.
- Map the result to a standardized HTTP response.

## API Surface Selection

Use entity-centric Controllers as the primary API surface.

Use Minimal API only for:

- System orchestration.
- External integrations.
- Webhooks.
- Batch processing.
- Cross-aggregate operations.
- Infrastructure endpoints such as health checks.

If an operation belongs to an entity lifecycle, use a Controller. If an operation spans multiple entities or is system-level, use Minimal API.

## Controller Design Model

Controllers represent a domain entity or entity relationship boundary.

### Collection Controller

Use for an entity collection root.

Example:

```text
TaskController -> /task
POST /task -> CreateTaskCommand
GET /task -> GetTasksQuery
```

Responsibilities:

- Create entities.
- Search, filter, and list entities.

### Single Entity Controller

Use for the lifecycle of one entity instance.

Example:

```text
SingleTaskController -> /task/{id}
GET /task/{id} -> GetTaskQuery
PUT /task/{id} -> UpdateTaskCommand
PATCH /task/{id} -> PatchTaskCommand
DELETE /task/{id} -> DeleteTaskCommand
POST /task/{id}/sync -> SyncTaskCommand
```

### Property Controller

Use for changing one addressable entity property.

Example:

```text
SingleTaskIsCompleteController -> /task/{id}/is-complete
POST /task/{id}/is-complete -> SetTaskIsCompleteCommand
DELETE /task/{id}/is-complete -> UnsetTaskIsCompleteCommand
```

### Collection Sub-resource Controller

Use for a collection related to one entity.

Example:

```text
TaskTagController -> /task/{id}/tag
GET /task/{id}/tag -> GetTaskTagsQuery
POST /task/{id}/tag -> AddTaskTagCommand
```

### Relationship Controller

Use for a specific relationship instance between entities.

Example:

```text
SingleTaskTagController -> /task/{taskId}/tag/{tagId}
GET /task/{taskId}/tag/{tagId} -> GetTaskTagQuery
PUT /task/{taskId}/tag/{tagId} -> UpdateTaskTagCommand
PATCH /task/{taskId}/tag/{tagId} -> PatchTaskTagCommand
DELETE /task/{taskId}/tag/{tagId} -> RemoveTaskTagCommand
POST /task/{taskId}/tag/{tagId}/sync -> SyncTaskTagCommand
```

## Naming Rules

- Collection Controller: `TaskController`.
- Single entity Controller: `SingleTaskController`.
- Property Controller: `SingleTaskIsCompleteController`.
- Sub-collection Controller: `TaskTagController`.
- Relationship instance Controller: `SingleTaskTagController`.
- Write request: `CreateTaskCommand`, `UpdateTaskCommand`, `DeleteTaskCommand`.
- Read request: `GetTaskQuery`, `GetTasksQuery`.

## MediatR Boundary

Each endpoint maps to exactly one:

- Command for write operations.
- Query for read operations.

Do not mix reads and writes in one endpoint. Do not dispatch multiple MediatR requests from one endpoint unless the endpoint is a system-level Minimal API orchestration and the orchestration is explicitly required.

## DTO Mapping Rules

Allowed mappings:

- Route values to Query or Command identifiers.
- Request body to Command DTO.
- Query string to Query DTO.
- MediatR result to response DTO.

Forbidden mappings:

- Business transformations.
- Domain decisions.
- Validation rules.
- Database lookups.

## Response Rules

All responses must use a consistent API contract:

- Success returns a typed DTO or `NoContent`.
- Errors return `ProblemDetails`.
- Created resources return `CreatedAtAction` or an equivalent typed route response.
- Validation errors are produced by the Application layer validation pipeline, not by Controller logic.

## Ardalis.Result Response Mapping

For every Command or Query, determine whether the response is wrapped in `Ardalis.Result.Result<T>` or returned as a plain response DTO.

When the response uses `Ardalis.Result`, define the allowed `ResultStatus` values and map each status to an explicit `ProducesResponseType` and HTTP response.

Example mapping:

```text
ResultStatus.Ok -> 200 OK -> ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)
ResultStatus.Created -> 201 Created -> ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)
ResultStatus.NoContent -> 204 No Content -> ProducesResponseType(StatusCodes.Status204NoContent)
ResultStatus.Invalid -> 400 Bad Request -> ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)
ResultStatus.NotFound -> 404 Not Found -> ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)
ResultStatus.Conflict -> 409 Conflict -> ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)
ResultStatus.Error -> 500 Internal Server Error -> ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)
```

When a Command or Query returns a plain response DTO without `Ardalis.Result`, define the endpoint-specific rule that determines `ProducesResponseType`.

Examples:

```text
CreateTaskCommand returns TaskDto -> 201 Created + ProblemDetails errors from exception middleware.
GetTaskQuery returns TaskDto -> 200 OK + 404 only if the handler can signal not found by exception or nullable contract.
DeleteTaskCommand returns Unit -> 204 No Content + ProblemDetails errors from exception middleware.
```

Every possible response produced by the Command or Query must have a matching `ProducesResponseType`. If the API receives an unexpected `ResultStatus`, null state, response variant, or unhandled response shape, it must throw an exception instead of returning an undocumented HTTP response.

Example:

```csharp
return result.Status switch
{
    ResultStatus.Created => CreatedAtAction(
        nameof(SingleTaskController.Get),
        "SingleTask",
        new { id = result.Value.Id },
        result.Value),
    ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
    ResultStatus.Conflict => Conflict(ToProblemDetails(result.Errors)),
    _ => throw new InvalidOperationException(
        $"Unexpected result status '{result.Status}' for CreateTaskCommand.")
};
```

## Suggested Folder Structure

```text
/Api
  /Controllers
    /Task
      TaskController.cs
      SingleTaskController.cs
      /Tag
        TaskTagController.cs
        SingleTaskTagController.cs
        SingleTaskIsCompleteController.cs
  /MinimalApi
    WebhookEndpoints.cs
    SyncEndpoints.cs
    SystemEndpoints.cs

/Application
  /Features
    /Task
      /CreateTask
        CreateTaskCommand.cs
        CreateTaskHandler.cs
      /GetTask
        GetTaskQuery.cs
        GetTaskHandler.cs
      /UpdateTask
        UpdateTaskCommand.cs
        UpdateTaskHandler.cs
      /DeleteTask
        DeleteTaskCommand.cs
        DeleteTaskHandler.cs
      /SyncTask
        SyncTaskCommand.cs
        SyncTaskHandler.cs
```

# Work steps

1. Classify the operation.
   Expected output:

   ```text
   Operation: mark task as complete
   Classification: entity property operation
   API surface: Controller
   Controller: SingleTaskIsCompleteController
   Route: /task/{id}/is-complete
   ```

2. Select Controller or Minimal API using the API surface selection rule.
   Expected output:

   ```text
   POST /webhooks/github -> Minimal API because it is an external integration.
   DELETE /task/{id} -> Controller because it belongs to entity lifecycle.
   ```

3. Define the route, verb, request, and response contract.
   Expected output:

   ```text
   POST /task
   Request: CreateTaskCommand
   Response: Result<TaskDto>
   Success: 201 Created
   Error: ProblemDetails
   ```

4. Determine whether the Command or Query returns `Ardalis.Result`.
   Expected output:

   ```text
   CreateTaskCommand returns Result<TaskDto>.
   Allowed statuses: Created, Invalid, Conflict, Error.
   ProducesResponseType mappings:
   - Created -> TaskDto, 201
   - Invalid -> ProblemDetails, 400
   - Conflict -> ProblemDetails, 409
   - Error -> ProblemDetails, 500
   Unexpected statuses throw InvalidOperationException.
   ```

5. If the Command or Query returns a plain response, define how `ProducesResponseType` is selected.
   Expected output:

   ```text
   GetTasksQuery returns IReadOnlyCollection<TaskDto>.
   ProducesResponseType mappings:
   - Success -> IReadOnlyCollection<TaskDto>, 200
   - Unhandled errors -> ProblemDetails from exception middleware, 500
   Unexpected null response throws InvalidOperationException.
   ```

6. Place the endpoint in the expected folder.
   Expected output:

   ```text
   /Api/Controllers/Task/TaskController.cs
   /Application/Features/Task/CreateTask/CreateTaskCommand.cs
   /Application/Features/Task/CreateTask/CreateTaskHandler.cs
   ```

7. Implement the API endpoint as a thin MediatR adapter.
   Example:

   ```csharp
   [ApiController]
   [Route("task")]
   public sealed class TaskController : ControllerBase
   {
       private readonly ISender _sender;

       public TaskController(ISender sender)
       {
           _sender = sender;
       }

       [HttpPost]
       [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
       [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
       [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
       [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
       public async Task<ActionResult<TaskDto>> Create(
           [FromBody] CreateTaskCommand command,
           CancellationToken cancellationToken)
       {
           var result = await _sender.Send(command, cancellationToken);

           return result.Status switch
           {
               ResultStatus.Created => CreatedAtAction(
                   nameof(SingleTaskController.Get),
                   "SingleTask",
                   new { id = result.Value.Id },
                   result.Value),
               ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
               ResultStatus.Conflict => Conflict(ToProblemDetails(result.Errors)),
               ResultStatus.Error => StatusCode(
                   StatusCodes.Status500InternalServerError,
                   ToProblemDetails(result.Errors)),
               _ => throw new InvalidOperationException(
                   $"Unexpected result status '{result.Status}' for CreateTaskCommand.")
           };
       }
   }
   ```

8. Validate that no API endpoint contains forbidden logic and every Command or Query response has a documented HTTP mapping.
   Expected output:

   ```text
   No DbContext usage in API.
   No domain calculations in API.
   One endpoint dispatches one MediatR request.
   ResultStatus.Created maps to 201 Created.
   ResultStatus.Invalid maps to 400 Bad Request.
   Unexpected ResultStatus throws InvalidOperationException.
   ```

# Check list

- [ ] All required input data is known or explicitly derived from existing code.
- [ ] The operation is classified as collection, single entity, property, sub-resource, relationship, Minimal API system operation, or Minimal API cross-entity operation.
- [ ] Entity lifecycle operations use Controllers.
- [ ] System, webhook, batch, infrastructure, and cross-aggregate operations use Minimal API.
- [ ] Controller name follows the naming rules.
- [ ] Route shape matches the selected Controller model.
- [ ] Each endpoint dispatches exactly one Command or Query.
- [ ] The API layer contains no business logic, validation logic, persistence logic, or domain rules.
- [ ] DTO mapping is limited to HTTP input and output mapping.
- [ ] Responses are standardized with typed success responses and `ProblemDetails` for errors.
- [ ] Each Command or Query response contract is classified as `Ardalis.Result` wrapped or plain response.
- [ ] Every allowed `Ardalis.Result.ResultStatus` has an explicit `ProducesResponseType` mapping.
- [ ] Plain responses define the endpoint-specific logic used to select `ProducesResponseType`.
- [ ] Every possible Command or Query response is checked against the declared `ProducesResponseType` mappings.
- [ ] Unexpected result statuses, null states, response variants, or unhandled response shapes throw an exception.
- [ ] Files are placed in the suggested API and Application feature structure or the repository established equivalent.
- [ ] Examples and expected outputs use real routes, request names, response names, and code.
