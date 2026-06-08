---
uid:
name: concurrency-control
description: defines the full optimistic concurrency flow — Version field, ETag encoding, If-Match validation, and ConcurrencyBehavior pipeline check
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - concurrency
  - etag
  - optimistic-locking
triggers:
  - optimistic concurrency
  - prevent lost update
  - etag if-match
  - version check
---
# Goal
Define how to prevent lost updates when multiple clients modify the same entity concurrently. Client receives an ETag on GET containing encoded entity versions. On update, client sends ETag as If-Match. API decodes it, pipeline validates versions before handler runs. Without this solution, the last writer silently wins and earlier changes are overwritten with no indication to the user.

# Core Principles
- ETag encodes all entity versions involved in the request as base64 JSON
- Missing If-Match returns 412 — not 400 or 409
- `ConcurrencyBehavior` validates versions before handler runs — returns 409 on mismatch
- Entity name string used as key — never C# type name — decouples HTTP contract from assembly names
- EF concurrency token is the final guard — pipeline is the early client-friendly check

# Depend on
- command-handling.solution.skill.md — ConcurrencyBehavior runs inside command pipeline

# Flow
```
GET /task/2
← 200 OK
← ETag: "eyJUYXNrIjp7IjIiOjN9fQ=="  (decoded: {"Task":{"2":3}})

Client modifies data

PUT /task/2 { title: "New title" }
→ If-Match: "eyJUYXNrIjp7IjIiOjN9fQ=="
    ↓
Controller checks If-Match present → 412 if missing or malformed
Controller decodes ETag → populates command.Versions
    ↓
ConcurrencyBehavior loads entity → compares Version
    ↓ (match)
Handler runs → domain call → UnitOfWorkBehavior commits
← 200 OK with new ETag

    ↓ (mismatch — another client updated in the meantime)
ConcurrencyBehavior returns Result.Conflict
← 409 Conflict
```

# Implementation

## {Entity}.cs — `{Module}.Domain/Entities`
Add `Version` property — set only by EF via xmin.
```csharp
public class Task
{
    public int Id { get; private set; }
    public uint Version { get; private set; }  // mapped to PostgreSQL xmin
}
```

## {Entity}Config.cs — `{Module}.Domain/Configurations`
```csharp
builder
    .Property(e => e.Version)
    .HasColumnName("xmin")
    .IsConcurrencyToken()
    .ValueGeneratedOnAddOrUpdate();
```

## IHasVersions.cs — `BuildingBlocks`
```csharp
public interface IHasVersions
{
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

## {CommandName}Command.cs — `{Module}.Interfaces/Commands`
Update commands implement `IHasVersions`.
```csharp
public record UpdateTaskCommand(
    int TaskId,
    string Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

## ConcurrencyBehavior.cs — `BuildingBlocks`
Runs after ValidationBehavior, before UnitOfWorkBehavior.
```csharp
public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var entityType = _resolver.Resolve(entityName);
            if (entityType is null)
                return (TResponse)Result.Error($"Unknown entity: {entityName}");

            foreach (var (id, expectedVersion) in idVersions)
            {
                var entity = await GetEntityByIdAsync(entityType, id, ct);
                if (entity is null) return (TResponse)Result.NotFound();
                if (entity.Version != expectedVersion)
                    return (TResponse)Result.Conflict($"{entityName}#{id} was modified.");
            }
        }
        return await next();
    }
}
```

## Single{Entity}Controller.cs — `{Module}.Api/Controllers`
GET encodes ETag. PUT/PATCH checks If-Match and decodes.
```csharp
// GET — encode ETag
var etag = ETagEncoder.Encode(new() { ["Task"] = new() { [id] = result.Value.Version } });
Response.Headers.ETag = $"\"{etag}\"";

// PUT — decode If-Match
if (string.IsNullOrEmpty(ifMatch)) return StatusCode(412);
var versions = ETagEncoder.Decode(ifMatch);
if (versions is null) return StatusCode(412);
var command = new UpdateTaskCommand(id, request.Title, versions);
```

# Example
```
Client A: GET /task/2  → ETag: "eyJ..." (version 3)
Client B: GET /task/2  → ETag: "eyJ..." (version 3)

Client A: PUT /task/2 If-Match: "eyJ..." → version matches → 200 OK (version now 4)

Client B: PUT /task/2 If-Match: "eyJ..." → version mismatch (expected 3, actual 4)
← 409 Conflict — client B must re-fetch and retry
```

# Rules
MUST:
- All mutable entities have `uint Version` mapped to `xmin`
- All update commands implement `IHasVersions`
- ETag encoded as base64 JSON — never plain version number
- Controller checks If-Match on all PUT/PATCH — returns 412 if missing or malformed
- `ConcurrencyBehavior` runs after `ValidationBehavior`, before `UnitOfWorkBehavior`
- Entity name keys are stable strings — never C# type names
MUST NOT:
- Return 400 for missing If-Match — 412 is correct
- Handler check versions manually — `ConcurrencyBehavior` owns this
- Skip ETag on GET responses for mutable entities

# Anti-patterns
- Missing If-Match returns 400 — must be 412
- Version as plain `uint` property on command — does not scale to multi-entity updates
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — unit of work starts before version validated
- ETag containing only primary entity version — misses secondary entity conflicts

# Checklist
- [ ] `uint Version` on all mutable entities
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()`
- [ ] All update commands implement `IHasVersions`
- [ ] GET endpoints include ETag header
- [ ] PUT/PATCH endpoints check If-Match — return 412 if missing
- [ ] `ConcurrencyBehavior` registered between ValidationBehavior and UnitOfWorkBehavior
- [ ] Entity name keys are stable strings

# Unittest TestCases
- [ ] When If-Match missing Then 412 returned before MediatR dispatch
- [ ] When version matches Then handler runs successfully
- [ ] When version mismatch Then ConcurrencyBehavior returns Result.Conflict
- [ ] When entity not found during version check Then Result.NotFound returned
- [ ] When ETag encoded and decoded Then versions round-trip losslessly

# Relations
- command-handling.solution.skill.md — pipeline position of ConcurrencyBehavior
- entity-concurrency.class.skill.md — Version field on entity
- ef-configuration.class.skill.md — xmin concurrency token configuration
- api-controller.class.skill.md — If-Match checking and ETag encoding
