---
name: plateau-service-with-api--class-system-endpoints
description: Class {System}Endpoints in the service-with-api plateau
whenToUse: when publishing an operation with no natural resource identity — a health probe, a webhook receiver, a batch job
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
- Give system-level, webhook, batch, and cross-aggregate operations a home outside the five entity-lifecycle controller types

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Naming convention
| use case | class name pattern | file name |
| -------- | ------------------- | --------- |
| System/webhook/batch endpoints | `{System}Endpoints` | `{System}Endpoints.cs` |

# Implementation
```csharp
//Skill: class-system-endpoints
//Plateau: service-with-api
//Version: 20260825120000

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
        });

        return app;
    }
}
```

See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]] for the full worked example with `Produces`/`ProducesProblem`.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Rules
MUST NOT:
- Publish an operation here that addresses one identified entity — that belongs in a Controller

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Check list
- [ ] Every endpoint here has no natural single-entity resource identity

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]
