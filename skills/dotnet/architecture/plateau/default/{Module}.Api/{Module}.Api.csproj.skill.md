---
uid: c5d2d4b7-018c-48ca-9c1b-15d586365897
name: module-api-csproj
description: Expose HTTP endpoints as thin MediatR adapters for this module
domain: skill
type: template
version: 20260622
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification-solution.skill]]"
---

# Goal
- Expose HTTP endpoints as thin MediatR adapters for this module
- Own all HTTP endpoint definitions for this module — controllers and Minimal API endpoint groups
- Be the only project that translates HTTP input to MediatR requests and HTTP output from MediatR results
- Provide the API-layer mapping from `ConflictResult<T>` to HTTP 409 with the existing entity result body
- Keep the idempotent create endpoint consistent with the rest of the result-based API
- Add ETag header to all GET responses for mutable entities
- Add `If-Match` header extraction, 412 guard, and `Versions` population to all PUT and PATCH endpoints

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Core Principals
- Api is a thin adapter — no business logic, no domain rules
- Api references only its own Interfaces project for contracts
- References only `{Module}.Interfaces` — command records, query records, and DTOs live there
- No business logic, validation logic, or persistence logic in any controller action
- One controller type per operation category — five types cover all entity lifecycle scenarios
- Minimal API used exclusively for system, webhook, batch, and cross-aggregate operations
- API layer is still a thin adapter — no business logic, no domain rules
- `ConflictResult<Create{Entity}Result>` is detected by type and mapped to a 409 response
- Successful creation returns 201 Created with the entity result and a `Location` header pointing to the GET endpoint
- Validation and error statuses continue to use `ProblemDetails` via the existing `ResultExtensions` from http-api-publication-solution.skill
- `Create{Entity}Result` contains only the entity Id, so the 409 body is `{ id: ... }`
- ETag format: `"<base64>"` — surrounding double quotes are part of the HTTP ETag format
- `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolverFactory` keys exactly
- If `If-Match` missing or `ETagEncoder.Decode` returns null → return `StatusCode(412)` immediately, before `_sender.Send()`
- `Versions` passed directly as command constructor argument — no manual construction in controller

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Api
```


## Project Structure
```
/{ModuleName}.Api
  /Controllers
  {ModuleName}.Api.csproj
```

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
```

```
/{Module}.Api
  /Extensions
    ConflictResultExtensions.cs
```

```
/{Module}.Api
  /Controllers
    Single{Entity}Controller.cs    ← extended with ETag and If-Match handling
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /Controllers | HTTP endpoints |  |
| /Controllers/{Entity}/{Entity}Controller.cs | Collection root — POST create, GET list | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/EntityController.class.skill.md|EntityController.class.skill]] |
| /Controllers/{Entity}/Single{Entity}Controller.cs | Single entity lifecycle — GET, PUT, PATCH, DELETE | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/SingleEntityController.class.skill.md|SingleEntityController.class.skill]] |
| /Controllers/{Entity}/Single{Entity}{Property}Controller.cs | Addressable property — POST set, DELETE unset | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/SingleEntityPropertyController.class.skill.md|SingleEntityPropertyController.class.skill]] |
| /Controllers/{Entity}/{Related}/{Entity}{Related}Controller.cs | Sub-collection — GET list, POST add | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/EntityRelatedController.class.skill.md|EntityRelatedController.class.skill]] |
| /Controllers/{Entity}/{Related}/Single{Entity}{Related}Controller.cs | Relationship instance — GET, PUT, PATCH, DELETE | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/SingleEntityRelatedController.class.skill.md|SingleEntityRelatedController.class.skill]] |
| /MinimalApi/{System}Endpoints.cs | System, webhook, batch, cross-aggregate endpoints | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/SystemEndpoints.class.skill.md|SystemEndpoints.class.skill]] |
| /Extensions/ResultExtensions.cs | ToProblemDetails helper for Result error mapping | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/ResultExtensions.class.skill.md|ResultExtensions.class.skill]] |
| /Extensions/ConflictResultExtensions.cs | Maps `ConflictResult<T>` to HTTP 409 with existing entity result body | [[skills/dotnet/architecture/plateau/default/{Module}.Api/classes/ConflictResultExtensions.class.skill.md|ConflictResultExtensions.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Handler implementations — belong to Application

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## Allowed Dependencies
- {Module}.Interfaces (own module only)
- Shared
- Ardalis.Result
- Microsoft.AspNetCore.Mvc
- BuildingBlocks
- {Module}.Interfaces

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Rules
MUST:
	- Every endpoint dispatches exactly one MediatR command or query
	- Api references only own Interfaces and BuildingBlocks
	- Every controller action dispatches exactly one `ISender.Send()` call
	- Controllers inject `ISender` — never `IMediator`
	- Controllers reference only `{Module}.Interfaces` types
	- All error responses use `ProblemDetails`
	- Every `ResultStatus` the handler can return has an explicit `ProducesResponseType`
	- Unexpected `ResultStatus` throws `InvalidOperationException`
	- `ConflictResultExtensions` defined in `{Module}.Api/Extensions/ConflictResultExtensions.cs`
	- Map `ConflictResult<Create{Entity}Result>` to HTTP 409 with the result body
	- Return 201 Created with entity result and `Location` header on successful creation
	- `Create{Entity}Result` contains only the entity Id
	- GET for mutable entity sets `Response.Headers.ETag` with encoded versions
	- PUT/PATCH checks `If-Match` presence — returns 412 if missing or malformed
	- `Versions` passed to command from decoded `If-Match` — never constructed in controller
	- 412 added to `[ProducesResponseType]` on all PUT/PATCH endpoints for mutable entities
	- DTO returned by GET for mutable entity includes `Version` field
MUST NOT:
	- Api reference Domain directly
	- Api reference Application directly
	- Api contain business logic, validation logic, or domain rules
	- Controller action contain business logic, validation, domain rules, or persistence
	- Controller reference Application, Domain, Infrastructure, or DbContext
	- Controller inject `IRepository<T>` or `IUnitOfWork`
	- Multiple `ISender.Send()` calls in one action — except Minimal API system orchestration with explicit justification
	- Put business logic or domain rules in the extension method
	- Return ProblemDetails for `ConflictResult<Create{Entity}Result>` — the idempotent create contract returns the existing result
	- GET for immutable entity set ETag header — immutable entities have no version
	- `Versions` hardcoded or constructed in controller — always from decoded `If-Match`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Anti-patterns
- Injecting a repository or DbContext into a controller — use MediatR dispatch only
- Writing business logic in a controller action — belongs in Domain
- Referencing Application project from Api — Api knows only Interfaces contracts
- Business logic in controller action
- Multiple `_sender.Send()` calls without system-level justification
- Using `IMediator` instead of `ISender`
- Mapping `ConflictResult<T>` to a ProblemDetails body — breaks the idempotent create contract
- Inline conflict mapping in every controller action instead of a shared extension
- ETag encoding only primary entity version — misses secondary entity conflicts
- Controller returns 400 for missing `If-Match` — 412 Precondition Failed is correct

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Check list
- [ ] Api.csproj does not reference Domain
- [ ] Api.csproj does not reference Application
- [ ] Every controller action dispatches exactly one MediatR request
- [ ] No business logic in any controller
- [ ] Project references only `{Module}.Interfaces`
- [ ] `/Controllers` folder exists with entity subfolders
- [ ] `/Extensions/ResultExtensions.cs` exists
- [ ] Controller actions dispatch exactly one `ISender.Send()`
- [ ] `ISender` injected — never `IMediator`
- [ ] `ConflictResultExtensions` defined in `{Module}.Api/Extensions/ConflictResultExtensions.cs`
- [ ] `ConflictResult<Create{Entity}Result>` mapped to 409 with the result body
- [ ] GET sets `Response.Headers.ETag`
- [ ] PUT/PATCH checks `If-Match`
- [ ] 412 returned if `If-Match` missing or malformed
- [ ] `Versions` passed to command from decoded `If-Match`
- [ ] 412 declared in `[ProducesResponseType]`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]
