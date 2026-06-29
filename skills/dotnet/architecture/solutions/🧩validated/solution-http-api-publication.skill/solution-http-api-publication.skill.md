---
name: solution-http-api-publication
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
  - "{Module}.Api.{Module}ApiSwaggerRegistration.cs"
  - App.Host.DependencyInjection.ApiRegistration.cs
extends:
  - "{Module}.Api.csproj"
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
---

# Goal
- Define the API layer as a thin HTTP adapter over MediatR — no business logic, no domain rules, no persistence
- Define five controller types covering all entity lifecycle operations: Collection, SingleEntity, Property, SubCollection, Relationship
- Define Minimal API as the surface for system-level, webhook, batch, and cross-aggregate operations
- Define the `Result<T>` to HTTP status mapping — every `ResultStatus` documented, unexpected statuses throw
- Define `ProblemDetails` as the universal error response shape
- Define controller naming, folder structure, and `[Route]` conventions
- Wire the API layer in App.Host — controller discovery, MediatR registration

# Capabilities
- Thin HTTP adapter layer with no business logic leakage
- Consistent controller naming and route conventions
- Standardized `Result<T>` to HTTP status mapping
- Uniform `ProblemDetails` error responses
- Clear separation between entity lifecycle controllers and system Minimal APIs
- Modular Swagger/OpenAPI definitions surfaced per module in Swagger UI

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create|{Module}.Api.csproj]] - hosts controllers, Minimal API endpoints, and result extensions
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts API and controller registration
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that produces `Result.Invalid` mapped to 400
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - defines `IRepository<T>` and `IReadRepository<T>` used by handlers
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs]] - handlers stage changes; controllers never call `SaveChanges`
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - handlers use read-only repository; commit is transparent
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IQuery<T>` marker for read operations
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs]] - controllers dispatch queries via `ISender`
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `ICommand<T>` marker for write operations
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - controllers dispatch commands via `ISender`

NUGET:
- `Microsoft.AspNetCore.Mvc` {version} - provides `ControllerBase`, `[ApiController]`, `[Route]`, `ActionResult`, `ProblemDetails`
- `MediatR` {version} - provides `ISender` injected into controllers
- `Ardalis.Result` {version} - provides `Result<T>`, `ResultStatus` mapped to HTTP responses

SWAGGER:
- Each `{Module}.Api` project declares its own Swagger definition metadata in a public static class named `{Module}ApiSwaggerRegistration`.
  - `DocumentName` — string constant used as the Swagger document name (e.g., `"tag"`, `"task"`, `"timelog"`).
  - `Title` — string constant used as `OpenApiInfo.Title`.
  - `Version` — string constant used as `OpenApiInfo.Version`.
  - `MatchesRoute(string? relativePath)` — static method that returns `true` when the given API route belongs to this module.
- `App.Host/Program.cs` imports the `*.Api` namespaces of all modules that publish HTTP APIs.
- `App.Host/Program.cs` registers one `SwaggerDoc` per module using `{Module}ApiSwaggerRegistration.DocumentName`, `Title`, and `Version`.
- `App.Host/Program.cs` provides a single `DocInclusionPredicate` that delegates route matching to the corresponding `{Module}ApiSwaggerRegistration.MatchesRoute`.
- `App.Host/Program.cs` registers one `SwaggerEndpoint` per module in `UseSwaggerUI` so that each module appears as a separate definition in the Swagger UI dropdown.
- Each Swagger definition contains only the routes that belong to that module; the default single-document `v1` convention is not used.
- Example reference implementations:
  - `src/Modules/TagModule/TaskUnderControl.Srv.TagModule.Api/TagModuleApiSwaggerRegistration.cs`
  - `src/Modules/TaskModule/TaskUnderControl.Srv.TaskModule.Api/TaskModuleApiSwaggerRegistration.cs`
  - `src/Modules/TimeLogModule/TaskUnderControl.Srv.TimeLogModule.Api/TimeLogModuleApiSwaggerRegistration.cs`
  - `src/App/App.Host/Program.cs`

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
- Each module that publishes HTTP APIs exposes a public static `{Module}ApiSwaggerRegistration` class in `{Module}.Api`
- `{Module}ApiSwaggerRegistration` declares `DocumentName`, `Title`, `Version`, and `MatchesRoute(string? relativePath)`
- `App.Host` registers one `SwaggerDoc` per module using the module's `{Module}ApiSwaggerRegistration` constants
- `App.Host` provides a single `DocInclusionPredicate` that delegates route matching to `{Module}ApiSwaggerRegistration.MatchesRoute`
- `App.Host` registers one `SwaggerEndpoint` per module in `UseSwaggerUI`

MUST NOT:
- Controller action contain business logic, validation, domain rules, or persistence
- Controller reference Application, Domain, Infrastructure, or DbContext
- Controller inject `IRepository<T>` or `IUnitOfWork`
- Minimal API replace entity-lifecycle controllers
- Undocumented HTTP responses returned — every response shape declared in `ProducesResponseType`
- Declare Swagger document metadata (document name, title, version, route matching) in `App.Host`
- Use a single monolithic `v1` Swagger document that contains all module routes
- Include routes from one module in another module's Swagger definition

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
- [ ] Each `{Module}.Api` contains `{Module}ApiSwaggerRegistration.cs`
- [ ] `{Module}ApiSwaggerRegistration` declares `DocumentName`, `Title`, `Version`, and `MatchesRoute`
- [ ] `App.Host` imports all `{Module}.Api` namespaces
- [ ] `App.Host` registers one `SwaggerDoc` per module
- [ ] `DocInclusionPredicate` delegates route matching to `{Module}ApiSwaggerRegistration.MatchesRoute`
- [ ] `UseSwaggerUI` registers one `SwaggerEndpoint` per module
- [ ] Swagger UI dropdown lists every module definition by `Title`
- [ ] No single `v1` document containing all routes is registered
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
