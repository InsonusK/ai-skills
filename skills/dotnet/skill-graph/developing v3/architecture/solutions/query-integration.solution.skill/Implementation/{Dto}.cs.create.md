---
description: DTO response shape declaration
project_name: "{Module}.Interfaces"
name: "{Dto}.cs"
change_kind: create
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

# Rules

MUST:
- Declared as `record`
- Properties are primitives or other DTOs
- Declared in `/{Module}.Interfaces/DTOs`

MUST NOT:
- Expose domain entity types as properties
- Have public setters — `record` provides immutability

# Anti-patterns
- DTO containing a domain entity — always project to flat primitives
- DTO with mutable properties — use `record` for immutability
