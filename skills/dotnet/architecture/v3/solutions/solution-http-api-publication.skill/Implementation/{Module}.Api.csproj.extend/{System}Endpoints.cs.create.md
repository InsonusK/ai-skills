---
description: Minimal API endpoints for system-level, webhook, batch, and cross-aggregate operations that don't fit the entity-lifecycle Controller model
project_name: "{Module}.Api"
name: "{System}Endpoints.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/system-endpoints-cs
---

# Goals
- Give operations that aren't entity CRUD (health probes, webhook receivers, batch imports, cross-aggregate commands) a home that doesn't force-fit them into one of the five Controller types

# Core Principles
- Minimal API is for operations with no natural resource identity — a webhook receiver, a batch job, a system probe. An operation that reads/writes one identified entity belongs in a Controller, even if it would be quick to add as a Minimal API endpoint
- Same dispatch discipline as Controllers: map input, one `ISender.Send()`, map output — a Minimal API endpoint is not a place to relax the thin-adapter rule

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | ------------------ | --------- |
| System/webhook/batch endpoints | `{System}Endpoints` | `TaskImportEndpoints` | `{System}Endpoints.cs` | `TaskImportEndpoints.cs` |

# Implementation changes

```csharp
// {Module}.Api/MinimalApi/TaskImportEndpoints.cs
using MediatR;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;

namespace {Module}.Api.MinimalApi;

public static class TaskImportEndpoints
{
    public static IEndpointRouteBuilder MapTaskImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks/import", async (ImportTasksCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                var problem = result.ToProblemDetails();
                return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
            }

            return Results.Ok(result.Value);
        })
        .WithName("ImportTasks")
        .Produces<ImportTasksResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
```

Wired from `ApiRegistration` alongside `MapControllers()`:
```csharp
app.MapTaskImportEndpoints();
```

# Rule changes

## MUST
- Dispatch exactly one `ISender.Send()` per endpoint, same as a Controller action
- Declare `Produces`/`ProducesProblem` for every status the dispatched command/query can return

## MUST NOT
- Publish an operation here that addresses one identified entity — that belongs in a Controller
- Perform business logic inside the endpoint delegate

# Check list
- [ ] Every endpoint here has no natural single-entity resource identity
- [ ] Every endpoint dispatches exactly one command/query and maps its result the same way a controller would
