---
uid: b734acb5-0450-4fe4-ac4c-31046c97ccf6
name: class-conflict-result-extensions
description: Maps ConflictResult<T> to HTTP 409 with existing entity result body
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
---

# Goal
- Provide a controller helper that maps `Result<Create{Entity}Result>` from an external-created create command to the correct HTTP response
- Return 201 Created with the entity result and `Location` header on success
- Return 409 Conflict with the existing entity result body when the command short-circuited on a duplicate Guid
- Delegate validation and error statuses to the existing ProblemDetails helpers

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Core Principals
- Thin adapter — no business logic, no domain rules
- `ConflictResult<Create{Entity}Result>` is detected by type so the response status is 409
- Both 201 and 409 return the same response type (`Create{Entity}Result`) — the API contract is symmetric
- `Create{Entity}Result` for external-created entities contains only the entity Id, so the 409 body is `{ id: ... }`
- All other statuses use the standard ProblemDetails mapping from solution-http-api-publication.skill

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Conflict result extensions | `ConflictResultExtensions` | `ConflictResultExtensions` | `ConflictResultExtensions.cs` | `ConflictResultExtensions.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Implementation
```csharp
// {Module}.Api/Extensions/ConflictResultExtensions.cs
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace {Module}.Api.Extensions;

public static class ConflictResultExtensions
{
    public static ActionResult<Create{Entity}Result> ToCreatedOrConflictResult(
        this Result<Create{Entity}Result> result,
        ControllerBase controller,
        string actionName,
        string controllerName)
    {
        return result.Status switch
        {
            ResultStatus.Created => controller.CreatedAtAction(
                actionName,
                controllerName,
                new { id = result.Value.Id },
                result.Value),

            ResultStatus.Conflict when result is ConflictResult<Create{Entity}Result>
                => controller.Conflict(result.Value),

            ResultStatus.Invalid => controller.BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),

            ResultStatus.Error => controller.StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),

            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for external-created command.")
        };
    }
}
```

Example usage in `{Entity}Controller`:

```csharp
[HttpPost]
[ProducesResponseType(typeof(Create{Entity}Result), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(Create{Entity}Result), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<Create{Entity}Result>> Create(
    [FromBody] Create{Entity}Command command,
    CancellationToken ct)
{
    var result = await _sender.Send(command, ct);

    return result.ToCreatedOrConflictResult(
        this,
        nameof(Single{Entity}Controller.Get),
        "Single{Entity}");
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Rules
MUST:
	- Detect `ConflictResult<Create{Entity}Result>` by type and return 409 with the result body
	- Return 201 Created with entity result and `Location` header on `ResultStatus.Created`
	- Return 400/500 ProblemDetails for `Invalid` and `Error` statuses
	- Throw `InvalidOperationException` for unexpected `ResultStatus` values
	- `Create{Entity}Result` contains only the entity Id
MUST NOT:
	- Return ProblemDetails for `ConflictResult<Create{Entity}Result>`
	- Contain business logic or domain rules
	- Allow `Create{Entity}Result` to carry fields beyond the entity Id for external-created entities

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Anti-patterns
- Mapping `ConflictResult<T>` to ProblemDetails — breaks the idempotent create contract
- Duplicating this mapping in every external-created controller action
- `Create{Entity}Result` with fields beyond `Id` for external-created entities — violates "server returns only Id"

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Check list
- [ ] `ConflictResultExtensions` defined in `{Module}.Api/Extensions/ConflictResultExtensions.cs`
- [ ] `ConflictResult<Create{Entity}Result>` detected by type
- [ ] 409 response body contains the existing `Create{Entity}Result`
- [ ] 201 response uses `CreatedAtAction`
- [ ] `Create{Entity}Result` contains only the entity Id

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN ResultStatus.Created returns 201 with entity result and Location header
- [ ] WHEN applied THEN ConflictResult<Create{Entity}Result> returns 409 with the existing result body
- [ ] WHEN applied THEN ResultStatus.Invalid returns 400 ProblemDetails
- [ ] WHEN applied THEN ResultStatus.Error returns 500 ProblemDetails
- [ ] WHEN applied THEN Unexpected ResultStatus throws InvalidOperationException
- [ ] WHEN naming 'Conflict result extensions' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs.create]]
