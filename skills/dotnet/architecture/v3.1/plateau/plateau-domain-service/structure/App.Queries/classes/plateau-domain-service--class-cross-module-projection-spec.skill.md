---
name: plateau-domain-service--class-cross-module-projection-spec
description: Class {Thing}ProjectionSpec in the plateau-domain-service plateau — an Ardalis Specification<T, TResult> in App.Queries that joins entities from more than one module and projects to a read DTO
whenToUse: when a read needs to join entities across module boundaries, or editing an existing cross-module projection spec
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Express a cross-module read as a named `Specification<T, TResult>` that joins across module entity types and projects straight to a read DTO — the only place that may see more than one module's `Domain`.

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md|{Entity}SummarySpec.cs.create]]
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]

# Core Principles
- Apply ONE plateau template per class.
- `Specification<T, TResult>` in `/App.Queries/Specifications`; `TResult` is a read DTO in `/App.Queries/DTOs`.
- Projects inside the query (`Query.Select(...)`) — never returns a tracked entity graph.
- Contains only filter / order / include / projection — no business logic, no I/O.
- Name reflects intent (`OpenOrdersWithCustomerSpec`), not field names.

# Implementation
```csharp
// Skill: plateau-domain-service--class-cross-module-projection-spec
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Specification;

namespace App.Queries.Specifications;

public sealed class OpenOrdersWithCustomerSpec : Specification<Order, OrderSummaryReadModel>
{
    public OpenOrdersWithCustomerSpec()
    {
        Query.Where(o => !o.IsClosed);
        Query.Select(o => new OrderSummaryReadModel(o.Id, o.CustomerId, o.Total));
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]

# Rules
MUST:
- Be `Specification<T, TResult>` in `/App.Queries/Specifications`, projecting to a read DTO.
- Contain only filtering / ordering / includes / projection — never a business rule, never I/O.
- Live in `App.Queries` only when the read genuinely spans modules; a single-module spec stays in that module's `Application`.
- Never apply several plateau templates per class.

# Check list
- [ ] `Specification<T, TResult>` in `/App.Queries/Specifications`; `TResult` is a read DTO.
- [ ] Projection inside the query; no tracked entity returned.
- [ ] Genuinely cross-module; no business logic.

# Unittest TestCases
- [ ] WHEN the spec runs THEN it returns the projected read DTOs matching the filter.
