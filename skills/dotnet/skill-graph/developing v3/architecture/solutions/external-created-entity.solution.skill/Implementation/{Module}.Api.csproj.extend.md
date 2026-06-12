---
description: Add ConflictException catch to collection controller POST
name: "{Module}.Api.csproj"
change_kind: extend
---

# Goals
- Extend collection controller POST actions to catch `ConflictException<Result<T>>` and return 409 with existing entity body

# Structure

## Project Structure
```
/{Module}.Api
  /Controllers
    /{Entity}
      {Entity}Controller.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Controllers/{Entity}/{Entity}Controller.cs | Collection controller with ConflictException catch on POST |

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Interfaces

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
- [ ] Controller POST catches `ConflictException<Result<Create{Entity}Result>>`
- [ ] 409 `[ProducesResponseType]` uses `typeof(Create{Entity}Result)`
- [ ] `Conflict(ex.Existing.Value)` returns existing entity body
