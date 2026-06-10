---
description: Collection root controller
project_name: "{Module}.Api"
name: "{Entity}Controller.cs"
change_kind: create
---

# Goals
- Handle creation (`POST`) and collection listing/filtering (`GET`) for one entity type
- Route: `/{entity}` — plural-free, kebab-case

# Core Principles
- `POST /{entity}` → `Create{Entity}Command` → `Result<{Entity}Result>` → 201 Created
- `GET /{entity}` → `Get{Entities}Query` → `Result<IReadOnlyList<{Entity}SummaryDto>>` → 200 OK
- No single-entity operations here — those belong in `Single{Entity}Controller`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Collection controller | `{Entity}Controller` | `TaskController` | `{Entity}Controller.cs` | `TaskController.cs` |

# Controller surface selection

| Operation | Belongs here | Reason |
| --- | --- | --- |
| Create entity | ✅ POST | Collection root creates |
| List / search entities | ✅ GET | Collection root lists |
| Get single entity | ❌ | Use `Single{Entity}Controller` |
| Update entity | ❌ | Use `Single{Entity}Controller` |
| Delete entity | ❌ | Use `Single{Entity}Controller` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/{Entity}/{Entity}Controller.cs
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Queries;

namespace {Module}.Api.Controllers.{Entity};

[ApiController]
[Route("{entity}")]
public sealed class {Entity}Controller : ControllerBase
{
    private readonly ISender _sender;

    public {Entity}Controller(ISender sender)
        => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Create{Entity}Result), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Create{Entity}Result>> Create(
        [FromBody] Create{Entity}Command command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(
                nameof(Single{Entity}Controller.Get),
                "Single{Entity}",
                new { id = result.Value.Id },
                result.Value),
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Create{Entity}Command.")
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<{Entity}SummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<{Entity}SummaryDto>>> GetAll(
        [FromQuery] Get{Entities}Query query,
        CancellationToken ct)
    {
        var result = await _sender.Send(query, ct);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Get{Entities}Query.")
        };
    }
}
```

# Rules

MUST:
- Named `{Entity}Controller`
- Route attribute `[Route("{entity}")]` — kebab-case, singular noun
- Handle only collection-level operations: POST create, GET list
- `[HttpPost]` maps to `Create{Entity}Command`
- `[HttpGet]` maps to `Get{Entities}Query`

MUST NOT:
- Handle `/{entity}/{id}` routes — those belong in `Single{Entity}Controller`

# Anti-patterns
- Handling single-entity operations in the collection controller
- Returning 200 for POST create — use 201 with `CreatedAtAction`

# Check list
- [ ] Named `{Entity}Controller`
- [ ] Route uses kebab-case singular noun
- [ ] POST dispatches `Create{Entity}Command`
- [ ] GET dispatches `Get{Entities}Query`
- [ ] 201 Created uses `CreatedAtAction` pointing to `Single{Entity}Controller.Get`
