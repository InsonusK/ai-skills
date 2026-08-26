---
name: plateau-statefull-service--class-query
description: Classes {Query}/{Dto} in the statefull-service plateau
whenToUse: when declaring a new read operation and its response shape for this module
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Declare a read-only query and the DTO it returns — the only data shape that crosses module boundaries for reads, never a domain entity

# Core Principles
- Query implements `IQuery<Result<TDto>>`, declared as `record` in `/{Module}.Interfaces/Queries`
- DTO is a `record` in `/{Module}.Interfaces/DTOs` with no domain entity type properties — primitives, Value Objects, or nested DTOs only

# Implementation
```csharp
//Skill: class-query
//Plateau: statefull-service
//Version: 20260824100000

public record GetTaskSummaryQuery(int TaskId) : IQuery<Result<TaskSummaryDto>>;

public record TaskSummaryDto(int Id, string Title);
```

# Rules
MUST:
- Query implements `IQuery<Result<TDto>>`, lives in `/{Module}.Interfaces/Queries`
- DTO is a `record` in `/{Module}.Interfaces/DTOs`, no domain entity properties
MUST NOT:
- Query implement `ICommand` or reach into domain entities directly
- DTO expose a domain entity type as a property

# Check list
- [ ] Query implements `IQuery<Result<TDto>>`
- [ ] DTO has no domain entity type properties

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../../solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md|{Query}.cs.create]], [[../../../../../solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs.create]]
