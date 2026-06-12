---
description: Add ConflictException exception class
name: Shared.csproj
change_kind: extend
---

# Goals
- Own `ConflictException<T>` — the exception thrown by `GuidResolvingBehavior` that carries the existing entity result to the controller

# Core Principles
- Lives in Shared — accessible by both BuildingBlocks (thrown) and Api layer (caught)
- Generic on the result type — carries `TResult` so the controller can return the existing entity body typed correctly
- Not a domain exception — it is a pipeline coordination exception

# Structure

## Project Structure
```
/Shared
  /Exceptions
    ConflictException.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Exceptions/ConflictException.cs | Exception carrying existing entity result for 409 responses |

# Allowed Dependencies
- None

# Rules

MUST:
- `ConflictException<T>` defined in Shared — accessible by BuildingBlocks (throw) and Api (catch)

MUST NOT:
- `ConflictException<T>` defined in BuildingBlocks — Api layer must not reference BuildingBlocks directly

# Anti-patterns
- `ConflictException<T>` defined in BuildingBlocks — violates layer boundaries

# Check list
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
