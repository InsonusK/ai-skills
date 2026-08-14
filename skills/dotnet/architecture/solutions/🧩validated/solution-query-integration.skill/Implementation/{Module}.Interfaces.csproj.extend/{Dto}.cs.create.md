---
description: DTO response shape declaration
project_name: "{Module}.Interfaces"
name: "{Dto}.cs"
element_kind: class
change_kind: create
tags:
  - solution/query-integration
  - element/dto-cs
---

# Goals
- Define the response shape returned by a query handler — a flat, read-only projection of domain data
- Be the only data shape that crosses module and layer boundaries for read operations

# Core Principles
- Declared as `record` — immutable, structural equality
- Properties are primitives or other DTOs — never domain entity types
- Declared in Interfaces alongside the query that returns it
- One DTO per distinct response shape — `TaskDto` for full detail, `TaskSummaryDto` for list items

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Full entity detail | `{Entity}Dto` | `TaskDto` | `{Entity}Dto.cs` | `TaskDto.cs` |
| List item / summary | `{Entity}SummaryDto` | `TaskSummaryDto` | `{Entity}SummaryDto.cs` | `TaskSummaryDto.cs` |
| Cross-module projection | `{Entity}With{Related}Dto` | `TaskWithUserDetailsDto` | `{Entity}With{Related}Dto.cs` | `TaskWithUserDetailsDto.cs` |

# Implementation changes

Full detail DTO:

```csharp
// {Module}.Interfaces/DTOs/TaskDto.cs
namespace {Module}.Interfaces.DTOs;

public record TaskDto(
    int Id,
    string Title,
    string Status,
    int AssigneeId);
```

Summary DTO:

```csharp
// {Module}.Interfaces/DTOs/TaskSummaryDto.cs
namespace {Module}.Interfaces.DTOs;

public record TaskSummaryDto(
    int Id,
    string Title,
    string Status);
```

Cross-module DTO:

```csharp
// {Module}.Interfaces/DTOs/TaskWithUserDetailsDto.cs
namespace {Module}.Interfaces.DTOs;

public record TaskWithUserDetailsDto(
    int Id,
    string Title,
    string AssigneeName,
    string AssigneeEmail);
```
# Rule changes

## MUST
- Declared as `record`
- Properties are primitives or other DTOs
- Declared in `/{Module}.Interfaces/DTOs`
- Every DTO has a matching `{Dto}Validator` in `{Module}.Application/Validators` from `solution-soft-value-objects-and-dto-validators.skill`
- Query transport validators use `SetValidator` with `IValidator<Soft{ValueObject}>` or `IValidator<{Dto}>` for cross-module properties

## SHOULD
- Use projection spec when DTO maps directly from entity fields — avoids loading full entity
- Use in-handler mapping when DTO requires computed fields, conditional logic, or nested structure

## MUST NOT
- Expose domain entity types as properties
- Have public setters — `record` provides immutability
- Query validator duplicates rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator` from `solution-soft-value-objects-and-dto-validators.skill`

# Anti-patterns
- DTO containing a domain entity — always project to flat primitives
- DTO with mutable properties — use `record` for immutability

# Unittest TestCases
- [ ] WHEN applied THEN Define the response shape returned by a query handler — a flat, read-only projection of domain data
- [ ] WHEN applied THEN Be the only data shape that crosses module and layer boundaries for read operations
- [ ] WHEN applied THEN Declared as record — immutable, structural equality
- [ ] WHEN applied THEN Properties are primitives or other DTOs — never domain entity types
- [ ] WHEN applied THEN Declared in Interfaces alongside the query that returns it
- [ ] WHEN applied THEN One DTO per distinct response shape — TaskDto for full detail, TaskSummaryDto for list items
- [ ] WHEN naming 'Full entity detail' THEN pattern matches convention
- [ ] WHEN naming 'List item / summary' THEN pattern matches convention
- [ ] WHEN naming 'Cross-module projection' THEN pattern matches convention
