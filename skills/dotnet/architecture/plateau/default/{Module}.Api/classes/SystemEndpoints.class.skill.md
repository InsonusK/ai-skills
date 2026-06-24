---
uid: 3e20f00a-5780-4fa2-b858-2254cb8590ac
name: systemendpoints-class
description: System, webhook, batch, cross-aggregate endpoints
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication-solution.skill]]"
---

# Goal
- Group system-level, webhook, batch, and cross-aggregate endpoints outside the entity-centric controller model
- Use `IEndpointRouteBuilder` extension methods — one class per system concern

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Core Principals
- Use Minimal API only when the operation does not belong to a single entity lifecycle
- Still dispatches exactly one MediatR command or query per endpoint — same dispatch rule as controllers
- Groups organized by system concern — not by entity

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| System endpoint group | `{System}Endpoints` | `WebhookEndpoints` | `{System}Endpoints.cs` | `WebhookEndpoints.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Rules
MUST:
	- Use only for non-entity-lifecycle operations — system, webhook, batch, cross-aggregate
	- Each Minimal API endpoint still dispatches exactly one MediatR command or query
	- All error responses use `Results.Problem` or `Results.BadRequest` with `ProblemDetails`
	- Unexpected `ResultStatus` throws `InvalidOperationException`
MUST NOT:
	- Replace entity-lifecycle Controllers with Minimal API — controllers are mandatory for entity operations

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Anti-patterns
- Using Minimal API for entity CRUD
- Multiple `sender.Send()` calls in one endpoint without system-level justification
- Returning custom error shapes instead of `Results.Problem`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

# Check list
- [ ] Named `{System}Endpoints`
- [ ] Extension method on `IEndpointRouteBuilder`
- [ ] Each endpoint dispatches exactly one command or query
- [ ] Error responses use `Results.Problem` or `Results.BadRequest`
- [ ] Unexpected `ResultStatus` throws `InvalidOperationException`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs.create]]
