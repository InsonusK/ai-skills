---
name: plateau-shared-rules--class-i-query
description: Class IQuery in the shared-rules plateau
whenToUse: when declaring a new read-only query record
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Mark a MediatR request as a read-only operation, kept distinct from `ICommand`

# Core Principles
- `IQuery<TResponse>` does not extend `ICommand` — queries and commands remain distinct marker families

# Implementation
```csharp
//Skill: class-i-query
//Plateau: shared-rules
//Version: 20260824163000

public interface IQuery<TResponse> : IRequest<TResponse> { }
```

# Rules
MUST:
- Live in `Shared/MediatR/IQuery.cs`
MUST NOT:
- Extend `ICommand`

# Check list
- [ ] `IQuery<TResponse>` defined in `Shared/MediatR/IQuery.cs`, does not extend `ICommand`

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../../solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
