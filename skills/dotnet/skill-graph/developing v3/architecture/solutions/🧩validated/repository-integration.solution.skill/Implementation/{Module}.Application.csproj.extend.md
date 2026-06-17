---
description: Enforce repository abstraction usage in Application layer handlers and host all specifications
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Ensure all command handlers use `IRepository<T>` for write-staging
- Ensure all query handlers use `IReadRepository<T>` for read-only access
- Keep Application layer decoupled from DbContext and Infrastructure
- Store all specifications for this module in Application — simple, multi-condition, projection, and idempotency

# Core Principals
- Command handlers inject `IRepository<T>` — they may read and stage changes
- Query handlers inject `IReadRepository<T>` — they only read, signaling intent
- No handler references `AppDbContext` or `App.Infrastructure` directly
- All specifications for this module live in one place — `/{Module}.Application/Specifications`
- Simple single-condition specs are reusable across features and event handlers
- Multi-condition or feature-specific specs belong to one use case
- Projection specs map entities to DTOs for read models

# Structure

## Project Structure
```
/{Module}.Application
  /Specifications
    {Entity}ByIdSpec.cs
    {Entity}ByGuidSpec.cs
    Active{Entities}Spec.cs
    Active{Entities}By{Owner}Spec.cs
    {Entity}SummarySpec.cs
    {Entity}ByEventIdSpec.cs
  /Features
    /{FeatureName}
      {Command}Handler.cs
      {Query}Handler.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Specifications | All module specifications — single-condition, multi-condition, projection, idempotency |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Ardalis.Specification` | latest stable | For creating `Specification<T>` and `Specification<T, TResult>` classes |

# Allowed Dependencies
- Shared
- `{Module}.Domain`
- `{Module}.Interfaces`
- MediatR

# Rules

MUST:
- Command handlers inject `IRepository<T>` from Shared
- Query handlers inject `IReadRepository<T>` from Shared
- All entity loading uses named specs — no inline `Where(...)` LINQ
- All specs for this module live in `/{Module}.Application/Specifications`
- Every entity loadable by `Id` has a `{Entity}ByIdSpec`
- Idempotency specs for event handlers live in `/Specifications` — not inside `/Features`

MUST NOT:
- Any Application class reference `AppDbContext`
- Query handlers inject `IRepository<T>`
- Specs placed in `{Module}.Domain` — Application is the single spec location

# Anti-patterns
- `private readonly AppDbContext _dbContext` in a handler
- `IRepository<T>` injected into a query handler
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — use named spec
- Single-condition spec duplicated across modules — reuse via shared interfaces, not copied specs

# Check list
- [ ] Command handlers use `IRepository<T>`
- [ ] Query handlers use `IReadRepository<T>`
- [ ] No DbContext references in Application
- [ ] All reads go through named specifications
- [ ] `/Specifications` folder exists in Application
- [ ] `{Entity}ByIdSpec` exists for every entity loadable by Id
- [ ] Idempotency specs live in `/Specifications`
