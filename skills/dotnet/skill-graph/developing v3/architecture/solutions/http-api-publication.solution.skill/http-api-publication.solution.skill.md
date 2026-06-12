---
uid: 7f3e9a2b-4c1d-4e8f-9a3b-2c1d4e8f9a3b
name: http-api-publication
description: Defines the HTTP API publication layer — entity-centric Controllers as thin MediatR adapters, Minimal API for system operations, controller naming and folder conventions, Result-to-HTTP mapping with ProblemDetails, and App.Host wiring for the API layer. Requires at least one of query-integration or command-integration to provide MediatR handler targets.
domain: skill
type: architecture
version: 20260611
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
  - publish http api
creates:
  - "{Module}.Api.Controllers.{Entity}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}{Property}Controller.cs"
  - "{Module}.Api.Controllers.{Entity}{Related}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}{Related}Controller.cs"
  - "{Module}.Api.MinimalApi.{System}Endpoints.cs"
  - "{Module}.Api.Extensions.ResultExtensions.cs"
  - "App.Host.DependencyInjection.ApiRegistration.cs"
extends:
  - "{Module}.Api.csproj"
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/query-integration.solution.skill/query-integration.solution.skill.md|query-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
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

# Requirements
SOLUTION:
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj]] - hosts controllers, Minimal API endpoints, and result extensions
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts API and controller registration
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that produces `Result.Invalid` mapped to 400
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - defines `IRepository<T>` and `IReadRepository<T>` used by handlers
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs]] - handlers stage changes; controllers never call `SaveChanges`
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - handlers use read-only repository; commit is transparent
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/query-integration.solution.skill/query-integration.solution.skill.md|query-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/query-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IQuery<T>` marker for read operations
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/query-integration.solution.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs]] - controllers dispatch queries via `ISender`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `ICommand<T>` marker for write operations
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - controllers dispatch commands via `ISender`

NUGET:
- `Microsoft.AspNetCore.Mvc` {version} - provides `ControllerBase`, `[ApiController]`, `[Route]`, `ActionResult`, `ProblemDetails`
- `MediatR` {version} - provides `ISender` injected into controllers
- `Ardalis.Result` {version} - provides `Result<T>`, `ResultStatus` mapped to HTTP responses

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add controllers, extensions, and minimal API endpoints
  - [[./Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs]] - create - ToProblemDetails helper for Result error mapping
  - [[./Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs]] - create - Collection root controller
  - [[./Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs]] - create - Single entity lifecycle controller
  - [[./Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs]] - create - Addressable property controller
  - [[./Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs]] - create - Sub-collection controller
  - [[./Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs]] - create - Relationship instance controller
  - [[./Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs]] - create - System, webhook, batch, cross-aggregate endpoints
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire API registration into the composition root
  - [[./Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] - create - Controller and middleware registration

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
- `ResultStatus.Ok` → 200 OK
- `ResultStatus.Created` → 201 Created with `CreatedAtAction`
- `ResultStatus.NoContent` → 204 No Content
- `ResultStatus.Invalid` → 400 Bad Request with `ProblemDetails`
- `ResultStatus.NotFound` → 404 Not Found with `ProblemDetails`
- `ResultStatus.Conflict` → 409 Conflict with `ProblemDetails`
- `ResultStatus.Error` → 500 Internal Server Error with `ProblemDetails`
- Any other `ResultStatus` → throw `InvalidOperationException`

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
