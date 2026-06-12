---
description: Add query record conventions in /Queries and DTO shapes in /DTOs
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Own all Query record declarations and DTO response shapes for this module
- Be the contract surface other modules and the API layer use to request data from this module

# Core Principles
- Queries are declarations only — records with input properties, no methods, no logic
- DTOs are read-only response shapes — records, no domain entity references
- Both declared in Interfaces so other modules can dispatch queries without depending on Application or Domain
- Cross-module queries are also declared here — implemented in App.Queries, declared in the owning module's Interfaces

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Queries
    Get{Entity}Query.cs
    Get{Entities}Query.cs
    Get{Entity}With{Related}Query.cs
  /DTOs
    {Entity}Dto.cs
    {Entity}SummaryDto.cs
    {Entity}With{Related}Dto.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Queries | Read intent contract declarations for this module |
| /DTOs | Response shape declarations consumed by query handlers and API |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Queries implement `IQuery<T>` which extends `IRequest<T>` |
| `Ardalis.Result` | latest stable | Query return types use `Result<T>` |

# Allowed Dependencies
- Shared — for `IQuery<T>` marker

# Rules

MUST:
- All queries for this module declared in `/{Module}.Interfaces/Queries`
- All DTOs for this module declared in `/{Module}.Interfaces/DTOs`
- Cross-module query contracts declared in the owning module's Interfaces — implemented in App.Queries
- Queries implement `IQuery<Result<T>>`
- DTOs declared as `record`

MUST NOT:
- Queries contain any logic or methods
- DTOs expose domain entity types — projection shapes only
- DTOs have public setters — declared as `record` for immutability

# Anti-patterns
- Query declared in Application — Interfaces is the public contract surface
- DTO with domain entity property — breaks layer isolation

# Check list
- [ ] `/{Module}.Interfaces/Queries` folder exists
- [ ] `/{Module}.Interfaces/DTOs` folder exists
- [ ] All query records implement `IQuery<Result<T>>`
- [ ] All DTOs are `record` types
- [ ] No domain entity types referenced in DTOs
