---
uid: a36897d9-c533-48c3-9866-3317a4fd217e
name: dto-class
description: DTO response shape declaration
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration.solution.skill]]"
---

# Goal
- Define the response shape returned by a query handler — a flat, read-only projection of domain data
- Be the only data shape that crosses module and layer boundaries for read operations

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

# Core Principals
- Declared as `record` — immutable, structural equality
- Properties are primitives or other DTOs — never domain entity types
- Declared in Interfaces alongside the query that returns it
- One DTO per distinct response shape — `TaskDto` for full detail, `TaskSummaryDto` for list items

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Full entity detail | `{Entity}Dto` | `TaskDto` | `{Entity}Dto.cs` | `TaskDto.cs` |
| List item / summary | `{Entity}SummaryDto` | `TaskSummaryDto` | `{Entity}SummaryDto.cs` | `TaskSummaryDto.cs` |
| Cross-module projection | `{Entity}With{Related}Dto` | `TaskWithUserDetailsDto` | `{Entity}With{Related}Dto.cs` | `TaskWithUserDetailsDto.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

# Implementation
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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

# Rules
MUST:
	- Declared as `record`
	- Properties are primitives or other DTOs
	- Declared in `/{Module}.Interfaces/DTOs`
MUST NOT:
	- Expose domain entity types as properties
	- Have public setters — `record` provides immutability

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

# Anti-patterns
- DTO containing a domain entity — always project to flat primitives
- DTO with mutable properties — use `record` for immutability

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]

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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
