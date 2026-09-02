---
name: plateau-domain-service--class-endpoint
description: Class {Entity}Endpoints in the plateau-domain-service plateau — a thin HTTP adapter that maps a request to a Command/Query, dispatches via ISender, maps the Result to a status
whenToUse: when adding or editing an HTTP endpoint for a module, or the Result-to-status / ETag / If-Match handling on one
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Expose one entity's operations over HTTP as a thin adapter: bind the request, build the `{Module}.Interfaces` Command/Query, `ISender.Send`, and translate the `Result` status to an HTTP status.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]

# Core Principles
- Apply ONE plateau template per class.
- In `/{Module}.Api/Http`. Injects `ISender`; references only `{Module}.Interfaces` types.
- Fixed `Result` → status mapping: `Success`/`Ok` → 200, `Created` → 201, `NoContent` → 204, `NotFound` → 404, `Invalid` → 400, `Conflict` → 409, `Forbidden` → 403, `Unauthorized` → 401, `Error` → 500.
- **VP5 (mutable entity over HTTP):** GET sets the `ETag` header from the entity versions; PUT/PATCH decode `If-Match` into the command's `Versions` and return `412` when it is missing or malformed.
- No business logic, no repository, no domain type.

# Implementation
```csharp
// Skill: plateau-domain-service--class-endpoint
// Plateau: domain-service
// Version: 20260902000000
using MediatR;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Http;

public static class {Entity}Endpoints
{
    public static void Map{Entity}Endpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{entity}/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Get{Entity}Query(id));
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

        app.MapPost("/{entity}", async (Add{Entity}Command cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return result.IsSuccess ? Results.Created($"/{entity}/{result.Value.Id}", result.Value) : result.ToProblem();
        });
    }
}
```
`ToProblem()` is the shared `Result` → `IResult` mapping from `solution-http-api-publication`.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend.md|Single{Entity}Controller.cs.extend]]

# Rules
MUST:
- Be a thin adapter — request → Command/Query → `ISender.Send` → map `Result`; no other logic.
- Use the fixed `Result` → status mapping; reference only `{Module}.Interfaces` types.
- For a mutable entity: set `ETag` on GET; decode `If-Match` on PUT/PATCH and return `412` when absent or malformed.
- Never call a repository, reference a domain type, or contain a business rule.
- Never apply several plateau templates per class.

# Check list
- [ ] In `/{Module}.Api/Http`; injects `ISender`; only `{Module}.Interfaces` types referenced.
- [ ] Fixed `Result` → status mapping applied.
- [ ] `ETag` on GET / `If-Match` (+ `412`) on PUT/PATCH for a mutable entity.

# Unittest TestCases
- [ ] WHEN the handler returns `Conflict` THEN the endpoint responds `409`.
- [ ] WHEN PUT is sent without `If-Match` for a mutable entity THEN the endpoint responds `412`.
