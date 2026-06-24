---
description: Add ConflictResult mapping for idempotent create endpoints
name: "{Module}.Api.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Provide the API-layer mapping from `ConflictResult<T>` to HTTP 409 with the existing entity result body
- Keep the idempotent create endpoint consistent with the rest of the result-based API

# Core Principles
- API layer is still a thin adapter — no business logic, no domain rules
- `ConflictResult<Create{Entity}Result>` is detected by type and mapped to a 409 response
- Successful creation returns 201 Created with the entity result and a `Location` header pointing to the GET endpoint
- Validation and error statuses continue to use `ProblemDetails` via the existing `ResultExtensions` from http-api-publication-solution.skill
- `Create{Entity}Result` contains only the entity Id, so the 409 body is `{ id: ... }`

# Structure

## Project Structure
```
/{Module}.Api
  /Extensions
    ConflictResultExtensions.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Extensions/ConflictResultExtensions.cs | Maps `ConflictResult<T>` to HTTP 409 with existing entity result body |

# Allowed Dependencies
- Shared
- Ardalis.Result
- Microsoft.AspNetCore.Mvc

# Rules

MUST:
- `ConflictResultExtensions` defined in `{Module}.Api/Extensions/ConflictResultExtensions.cs`
- Map `ConflictResult<Create{Entity}Result>` to HTTP 409 with the result body
- Return 201 Created with entity result and `Location` header on successful creation
- `Create{Entity}Result` contains only the entity Id

MUST NOT:
- Put business logic or domain rules in the extension method
- Return ProblemDetails for `ConflictResult<Create{Entity}Result>` — the idempotent create contract returns the existing result

# Anti-patterns
- Mapping `ConflictResult<T>` to a ProblemDetails body — breaks the idempotent create contract
- Inline conflict mapping in every controller action instead of a shared extension

# Check list
- [ ] `ConflictResultExtensions` defined in `{Module}.Api/Extensions/ConflictResultExtensions.cs`
- [ ] `ConflictResult<Create{Entity}Result>` mapped to 409 with the result body
