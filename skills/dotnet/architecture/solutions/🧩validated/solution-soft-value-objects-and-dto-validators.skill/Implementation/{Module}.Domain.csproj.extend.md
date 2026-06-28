---
description: Add {Module}.Interfaces reference so Domain Value Objects can inherit from Soft{ValueObject}
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Enable Domain Value Objects to inherit from their `Soft{ValueObject}` base types
- Keep strict invariant enforcement in Domain while sharing the shape from Interfaces

# Core Principles
- Domain remains the authority on invariant enforcement
- Domain depends on its own `{Module}.Interfaces` only for `Soft{ValueObject}` shapes
- Domain does not consume FluentValidation directly

# Structure

## Project Structure
No new folders; existing `/ValueObjects` now contains VOs that inherit from Soft types.

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /ValueObjects/{ValueObject}.cs | Domain VO inheriting from `Soft{ValueObject}` |

# NuGet Packages
None.

# What Does NOT Belong Here
- FluentValidation usage — belongs in `{Module}.Interfaces`
- Validator implementations — belong in `{Module}.Interfaces`

# Allowed Dependencies
- Shared
- `{Module}.Interfaces` (for `Soft{ValueObject}` base types)
- `Microsoft.EntityFrameworkCore` (for `IEntityTypeConfiguration` only)

# Rules
MUST:
- Reference `{Module}.Interfaces`
- Every `{ValueObject}` in `/ValueObjects` inherit from `Soft{ValueObject}`
- Domain VO constructor enforces invariants and throws `DomainException` on invalid values

MUST NOT:
- Use FluentValidation types directly
- Reference other modules' Interfaces or Domains

# Anti-patterns
- Domain VO not inheriting from `Soft{ValueObject}`
- Putting validator code in Domain

# Check list
- [ ] `{Module}.Domain.csproj` references `{Module}.Interfaces.csproj`
- [ ] Every Domain VO inherits from `Soft{ValueObject}`
- [ ] Domain VO throws `DomainException` for invalid values
