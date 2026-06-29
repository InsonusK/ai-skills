---
name: class-dto
description: DTO response shape declaration
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Define the response shape returned by a query handler — a flat, read-only projection of domain data
- Be the only data shape that crosses module and layer boundaries for read operations

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Declared as `record` — immutable, structural equality
- Properties are primitives, other DTOs, or `Soft{ValueObject}` types — never domain entity types
- Declared in Interfaces alongside the query that returns it
- Has a matching `{Dto}Validator` in `{Module}.Application/Validators` when consumed by other modules
- One DTO per distinct response shape — `TaskDto` for full detail, `TaskSummaryDto` for list items

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Full entity detail | `{Entity}Dto` | `TaskDto` | `{Entity}Dto.cs` | `TaskDto.cs` |
| List item / summary | `{Entity}SummaryDto` | `TaskSummaryDto` | `{Entity}SummaryDto.cs` | `TaskSummaryDto.cs` |
| Cross-module projection | `{Entity}With{Related}Dto` | `TaskWithUserDetailsDto` | `{Entity}With{Related}Dto.cs` | `TaskWithUserDetailsDto.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-dto
//Plateau: default
//Version: 20260628
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Rules
MUST:
	- Declared as `record`
	- Properties are primitives, other DTOs, or `Soft{ValueObject}` types
	- Declared in `/{Module}.Interfaces/DTOs`
	- Have a matching `{Dto}Validator` in `{Module}.Application/Validators` when the DTO is public and consumed by other modules
MUST NOT:
	- Expose domain entity types as properties
	- Have public setters — `record` provides immutability

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- DTO containing a domain entity — always project to flat primitives or Soft VOs
- DTO with mutable properties — use `record` for immutability
- DTO validator placed in `{Module}.Interfaces` or `{Module}.Domain`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
