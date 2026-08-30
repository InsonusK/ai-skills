---
name: solution-http-api-publication
description: Defines the HTTP API publication layer — entity-centric Controllers as thin MediatR adapters, Minimal API for system operations, controller naming and folder conventions, Result-to-HTTP mapping with ProblemDetails, and App.Host wiring for the API layer. Requires at least one of query-integration or command-integration to provide MediatR handler targets.
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - stack/dotnet
  - framework/aspnet-core
  - api
  - controllers
  - minimal-api
  - cqrs
  - framework/mediatr
  - concern/architecture
  - solution/http-api-publication

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
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]"
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
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create|{Module}.Api.csproj]] - hosts controllers, Minimal API endpoints, and result extensions
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts API and controller registration
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - provides `ValidationBehavior` that produces `Result.Invalid` mapped to 400
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - defines `IRepository<T>` and `IReadRepository<T>` used by handlers
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]] - handlers stage changes; controllers never call `SaveChanges`
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create|IReadRepository.cs]] - handlers use read-only repository; commit is transparent
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IQuery<T>` marker for read operations
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create|IQuery.cs]] - controllers dispatch queries via `ISender`
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `ICommand<T>` marker for write operations
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create|ICommand.cs]] - controllers dispatch commands via `ISender`

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
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend|{Module}.Api.csproj]] - extend - Add controllers, extensions, and minimal API endpoints
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create|ResultExtensions.cs]] - create - ToProblemDetails helper for Result error mapping
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create|{Entity}Controller.cs]] - create - Collection root controller
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create|Single{Entity}Controller.cs]] - create - Single entity lifecycle controller
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create|Single{Entity}{Property}Controller.cs]] - create - Addressable property controller
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create|{Entity}{Related}Controller.cs]] - create - Sub-collection controller
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create|Single{Entity}{Related}Controller.cs]] - create - Relationship instance controller
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create|{System}Endpoints.cs]] - create - System, webhook, batch, cross-aggregate endpoints
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire API registration into the composition root
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]] - create - Controller and middleware registration

# Rules

## MUST:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create#MUST|ApiRegistration.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend#MUST|{Module}.Api.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create#MUST|ResultExtensions.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create#MUST|Single{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create#MUST|Single{Entity}{Property}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create#MUST|Single{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create#MUST|{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create#MUST|{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create#MUST|{System}Endpoints.cs]]
- API layer is a thin HTTP adapter — map input, dispatch once, map output
- All error responses use `ProblemDetails`

## MUST NOT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create#MUST NOT|ApiRegistration.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend#MUST NOT|{Module}.Api.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create#MUST NOT|ResultExtensions.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create#MUST NOT|Single{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create#MUST NOT|Single{Entity}{Property}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create#MUST NOT|Single{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create#MUST NOT|{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create#MUST NOT|{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create#MUST NOT|{System}Endpoints.cs]]
- Undocumented HTTP responses returned — every response shape declared in `ProducesResponseType`
- Include routes from one module in another module's Swagger definition

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
