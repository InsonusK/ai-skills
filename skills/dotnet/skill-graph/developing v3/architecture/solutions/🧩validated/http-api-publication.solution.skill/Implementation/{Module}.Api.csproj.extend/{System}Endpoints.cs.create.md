---
description: System, webhook, batch, cross-aggregate endpoints
project_name: "{Module}.Api"
name: "{System}Endpoints.cs"
element_kind: class
change_kind: create
---

# Goals
- Group system-level, webhook, batch, and cross-aggregate endpoints outside the entity-centric controller model
- Use `IEndpointRouteBuilder` extension methods — one class per system concern

# Core Principles
- Use Minimal API only when the operation does not belong to a single entity lifecycle
- Still dispatches exactly one MediatR command or query per endpoint — same dispatch rule as controllers
- Groups organized by system concern — not by entity

# API surface selection decision table

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

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| System endpoint group | `{System}Endpoints` | `WebhookEndpoints` | `{System}Endpoints.cs` | `WebhookEndpoints.cs` |

# Implementation changes

```csharp
// {Module}.Api/MinimalApi/{System}Endpoints.cs
using Ardalis.Result;
using MediatR;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;

namespace {Module}.Api.MinimalApi;

public static class {System}Endpoints
{
    public static IEndpointRouteBuilder Map{System}Endpoints(
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

# Rules

MUST:
- Use only for non-entity-lifecycle operations — system, webhook, batch, cross-aggregate
- Each Minimal API endpoint still dispatches exactly one MediatR command or query
- All error responses use `Results.Problem` or `Results.BadRequest` with `ProblemDetails`
- Unexpected `ResultStatus` throws `InvalidOperationException`

MUST NOT:
- Replace entity-lifecycle Controllers with Minimal API — controllers are mandatory for entity operations

# Anti-patterns
- Using Minimal API for entity CRUD
- Multiple `sender.Send()` calls in one endpoint without system-level justification
- Returning custom error shapes instead of `Results.Problem`

# Check list
- [ ] Named `{System}Endpoints`
- [ ] Extension method on `IEndpointRouteBuilder`
- [ ] Each endpoint dispatches exactly one command or query
- [ ] Error responses use `Results.Problem` or `Results.BadRequest`
- [ ] Unexpected `ResultStatus` throws `InvalidOperationException`

# Unittest TestCases
- [ ] WHEN applied THEN Group system-level, webhook, batch, and cross-aggregate endpoints outside the entity-centric controller model
- [ ] WHEN applied THEN Use IEndpointRouteBuilder extension methods — one class per system concern
- [ ] WHEN applied THEN Use Minimal API only when the operation does not belong to a single entity lifecycle
- [ ] WHEN applied THEN Still dispatches exactly one MediatR command or query per endpoint — same dispatch rule as controllers
- [ ] WHEN applied THEN Groups organized by system concern — not by entity
- [ ] WHEN verified THEN Named {System}Endpoints
- [ ] WHEN verified THEN Extension method on IEndpointRouteBuilder
- [ ] WHEN verified THEN Each endpoint dispatches exactly one command or query
- [ ] WHEN verified THEN Error responses use Results.Problem or Results.BadRequest
- [ ] WHEN verified THEN Unexpected ResultStatus throws InvalidOperationException
- [ ] WHEN naming 'System endpoint group' THEN pattern matches convention
