---
description: Add /ValueObjects folder for soft value object declarations that other modules can use in their commands and DTOs
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Expose soft value object shapes from `{Module}.Interfaces` so other modules can use them in commands and DTOs
- Keep `{Module}.Interfaces` declarations-only; validators belong in `{Module}.Application`

# Core Principles
- `{Module}.Interfaces` remains the public surface of the module
- `Soft{ValueObject}` is a plain declaration with no validation logic
- `{Module}.Interfaces` does not reference FluentValidation

# Structure

## Project Structure
```
/{Module}.Interfaces
  /ValueObjects
    Soft{ValueObject}.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /ValueObjects | Soft value object declarations |

# NuGet Packages
None.

# What Does NOT Belong Here
- Validators — belong in `{Module}.Application`
- Domain Value Object invariant enforcement — belongs in `{Module}.Domain`
- Command handler logic — belongs in `{Module}.Application`
- Business rules — belong in `{Module}.Domain`

# Allowed Dependencies
- Shared

# Rules
MUST:
- Add `/ValueObjects` folder containing `Soft{ValueObject}.cs`
- Keep `{Module}.Interfaces` declarations-only

MUST NOT:
- Reference FluentValidation
- Reference `{Module}.Domain`, `{Module}.Application`, or any infrastructure project

# Anti-patterns
- Adding validator implementations to `{Module}.Interfaces`
- Adding validation logic to `Soft{ValueObject}`

# Check list
- [ ] `/ValueObjects` folder exists
- [ ] No FluentValidation reference added
- [ ] No domain or application references added
