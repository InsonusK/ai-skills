---
name: solution-http-api-publication
description: The REST transport for a module's inbound sync API (VP8) — entity-centric Controllers as thin MediatR adapters, Minimal API for system operations, naming/folder conventions, Result-to-HTTP mapping with ProblemDetails, and the AddHttpApi() partial that plugs into solution-api-project's ApiRegistration. Command dispatch is common; GET actions require persistence (VP2).
whenToUse: when publishing a module's commands/queries over HTTP — adding a controller, defining a route, or mapping a Result to an HTTP response
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - framework/aspnet-core
  - api
  - controllers
  - minimal-api
  - cqrs
  - framework/mediatr
  - solution/http-api-publication
  - stack/dotnet

creates:
  - "{Module}.Api.Controllers.{Entity}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}{Property}Controller.cs"
  - "{Module}.Api.Controllers.{Entity}{Related}Controller.cs"
  - "{Module}.Api.Controllers.Single{Entity}{Related}Controller.cs"
  - "{Module}.Api.MinimalApi.{System}Endpoints.cs"
  - "{Module}.Api.Extensions.ResultExtensions.cs"
  - "{Module}.Api.{Module}ApiSwaggerRegistration.cs"
  - App.Host.DependencyInjection.HttpApiRegistration.cs
extends:
  - "{Module}.Api.csproj"
  - App.Host.DependencyInjection.ApiRegistration.cs
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/adr/reads-require-persistence.md|GET actions require persistence (VP2); command dispatch is common]]"
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
- API layer never references `IRepository<T>`, `IUnitOfWork`, DbContext, or any domain entity type — controllers only ever see `ICommand<T>`/`IQuery<T>` and DTOs from `{Module}.Interfaces`
- Command dispatch is common (`solution-mediator-integration`), so POST/PUT/PATCH/DELETE actions are always available. `GET` actions are only meaningful once the read side exists — that is `solution-query-integration` (VP2). A module with no persistence gets a **write-only** REST surface; the same solution application, minus GET actions.

# Boundaries
- Optional, and independent of [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] (VP9). Apply this alone for HTTP-only, gRPC alone for gRPC-only, or both — both extend the same `partial ApiRegistration` from [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] and dispatch through the same `ISender`.
- `solution-repository-integration` is not a dependency — the controller never references `IRepository<T>`, only the handler behind `ISender.Send()` does. Whether persistence exists is invisible from this solution's files, except that GET actions are pointless without a query handler to back them.
- See the ADR: this is why v3's "requires command or query integration" constraint is inert in v3.1 — command dispatch is common, and the read side is VP2.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj]] - the project this solution adds controllers to
  - [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] - the `partial` class this solution adds `AddHttpApi()` to
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - the markers a controller dispatches
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - produces `Result.Invalid` mapped to 400
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] (VP2, for GET actions — not required; write-only API without it)
  - provides the repository-backed query handlers GET actions dispatch to

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
  - `src/Modules/{Module}/{Module}.Api/{Module}ApiSwaggerRegistration.cs`
  - `src/Modules/{Module}/{Module}.Api/{Module}ApiSwaggerRegistration.cs`
  - `src/Modules/{Module}/{Module}.Api/{Module}ApiSwaggerRegistration.cs`
  - `src/App/App.Host/Program.cs`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add controllers, extensions, and minimal API endpoints
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs]] - create - ToProblemDetails helper for Result error mapping
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md|{Entity}Controller.cs]] - create - Collection root controller
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md|Single{Entity}Controller.cs]] - create - Single entity lifecycle controller
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs]] - create - Addressable property controller
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md|{Entity}{Related}Controller.cs]] - create - Sub-collection controller
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md|Single{Entity}{Related}Controller.cs]] - create - Relationship instance controller
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md|{System}Endpoints.cs]] - create - System, webhook, batch, cross-aggregate endpoints
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md|{Module}ApiSwaggerRegistration.cs]] - create - Per-module Swagger document metadata and route-matching predicate
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire API registration into the composition root
  - [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] - create - Controller and middleware registration

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md#MUST|ApiRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md#MUST|{Module}.Api.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md#MUST|ResultExtensions.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.create.md#MUST|Single{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md#MUST|Single{Entity}{Property}Controller.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Related}Controller.cs.create.md#MUST|Single{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}Controller.cs.create.md#MUST|{Entity}Controller.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Entity}{Related}Controller.cs.create.md#MUST|{Entity}{Related}Controller.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{System}Endpoints.cs.create.md#MUST|{System}Endpoints.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md#MUST|{Module}ApiSwaggerRegistration.cs]]
- API layer is a thin HTTP adapter — map input, dispatch once, map output
- All error responses use `ProblemDetails`
- Never undocumented HTTP responses returned — every response shape declared in `ProducesResponseType`
- Never include routes from one module in another module's Swagger definition

## SHOULD
- Avoid business logic in controller action: `if (task.IsComplete) return Conflict(...)` — belongs in domain
- Avoid multiple `_sender.Send()` calls in one controller action without explicit system-level justification
- Avoid returning 200 for a create operation — use 201 with `CreatedAtAction`
- Avoid missing `[ProducesResponseType]` for a `ResultStatus` the handler can return — undocumented response
- Avoid swallowing unexpected `ResultStatus` with a fallback 500 — throw `InvalidOperationException`
- Avoid using `IMediator` instead of `ISender` — `ISender` is the correct interface for request dispatch
- Avoid minimal API used for entity CRUD — entity lifecycle belongs in typed controllers

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
