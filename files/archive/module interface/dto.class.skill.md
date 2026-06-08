---
name: dto
description: defines how to declare Data Transfer Objects in the Interfaces project
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - dto
  - interfaces
triggers:
  - declare dto
  - response shape
  - query result type
---
# Goal
Define how to declare DTOs in `{Module}.Interfaces`. A DTO is a flat, immutable projection of data returned by a query or command. It is the only shape that crosses module boundaries — domain entities never leave the module.

# Core Principles
- DTO is a flat projection — no domain entity references, no business logic
- `record` type — immutable, structural equality
- DTO shape is driven by what the consumer needs — not by entity structure
- One DTO per use case — do not reuse the same DTO for different query shapes

# Structure
## Place in csproj
Defined in `module-interfaces.csproj.skill.md`
```
/{ModuleName}.Interfaces
  /DTOs
    TaskDto.cs
    TaskSummaryDto.cs
```

## Naming convention
```
class name:
  rule: entity name + Dto suffix, or entity name + purpose + Dto suffix
  pattern: {Entity}Dto / {Entity}{Purpose}Dto
  example: TaskDto, TaskSummaryDto, TaskWithUserDetailsDto

file name:
  rule: matches class name exactly
  pattern: {Entity}Dto.cs
  example: TaskDto.cs
```

# Contracts

## Full detail DTO
```csharp
public record TaskDto(
    int Id,
    string Title,
    string Status,
    int AssigneeId,
    uint Version
);
```

## Summary DTO — for list queries
```csharp
public record TaskSummaryDto(
    int Id,
    string Title,
    string Status
);
```

## Cross-module DTO — includes data from multiple modules
```csharp
public record TaskWithUserDetailsDto(
    int TaskId,
    string Title,
    string AssigneeName,
    string AssigneeEmail
);
```

# Rules
MUST:
- Be a `record` type
- Contain only primitive types or other DTOs — never domain entities
- Include `Version` field if DTO is returned by a query for a mutable entity that supports updates
MUST NOT:
- Reference domain entity types
- Contain methods or business logic
- Be shared between commands and queries — separate shapes for separate purposes

# Anti-patterns
- DTO exposes domain entity: `public Task Task { get; }` — project to flat fields
- Same DTO reused for list and detail — list needs summary shape, detail needs full shape
- DTO missing `Version` for mutable entity — client cannot construct If-Match header for updates

# Checklist
- [ ] `record` type
- [ ] Only primitive types or nested DTOs
- [ ] No domain entity references
- [ ] `Version` included if entity is mutable and updatable
- [ ] Separate DTOs for list vs detail shapes

# Relations
- module-interfaces.csproj.skill.md — project this DTO lives in
- query.class.skill.md — queries return these DTOs
- command.class.skill.md — command result types are also declared in same file as command
- concurrency-control.solution.skill.md — Version field in DTO enables If-Match on updates
