---
description: Catch ConflictException and return 409 with existing entity body
name: "{Entity}Controller.cs"
change_kind: extend
---

# Goals
- Wrap `ISender.Send()` call in a try/catch for `ConflictException<Result<T>>` on POST create actions
- Return 409 with the existing entity body from `ex.Existing.Value` — client recovers without a second GET

# Core Principles
- `try/catch` wraps only the `_sender.Send()` call and its result switch — not the entire action
- Catches `ConflictException<Result<Create{Entity}Result>>` — typed to the specific command result
- Returns `Conflict(ex.Existing.Value)` — the existing entity body, not a `ProblemDetails`
- `ConflictException` is only possible from `GuidResolvingBehavior` — no other pipeline component throws it
- `[ProducesResponseType]` for 409 must use the entity result type — not `ProblemDetails` — because the body is the existing entity

# Implementation changes
POST action extended with ConflictException catch:

```csharp
// {Module}.Api/Controllers/{Entity}/{Entity}Controller.cs
[HttpPost]
[ProducesResponseType(typeof(Create{Entity}Result), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(Create{Entity}Result), StatusCodes.Status409Conflict)]  // ← entity body, not ProblemDetails
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<Create{Entity}Result>> Create(
    [FromBody] Create{Entity}Command command,
    CancellationToken ct)
{
    try
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
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Create{Entity}Command.")
        };
    }
    catch (ConflictException<Result<Create{Entity}Result>> ex)
    {
        // duplicate Guid — entity already exists
        // return existing entity body so client can recover without a second GET
        return Conflict(ex.Existing.Value);
    }
}
```

# Rules

MUST:
- `try/catch (ConflictException<Result<Create{Entity}Result>> ex)` wraps the `_sender.Send()` call
- 409 `[ProducesResponseType]` uses `typeof(Create{Entity}Result)` — not `typeof(ProblemDetails)`
- `Conflict(ex.Existing.Value)` returns the entity body — not `Conflict(new ProblemDetails(...))`

MUST NOT:
- Catch `ConflictException` with a generic `Exception` catch — must be typed to the specific result type
- Return empty 409 body — client must receive the existing entity to recover

# Anti-patterns
- 409 returns `ProblemDetails` instead of existing entity — client forced to make a second GET to recover

# Check list
- [ ] `try/catch` wraps `_sender.Send()`
- [ ] Catches typed `ConflictException<Result<Create{Entity}Result>>`
- [ ] Returns `Conflict(ex.Existing.Value)`
- [ ] 409 `[ProducesResponseType]` uses `typeof(Create{Entity}Result)`
