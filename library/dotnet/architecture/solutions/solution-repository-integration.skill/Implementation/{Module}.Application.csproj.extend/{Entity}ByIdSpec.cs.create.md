---
description: Simple single-condition specification to load an entity by Id
project_name: "{Module}.Application"
name: "{Entity}ByIdSpec.cs"
element_kind: class
change_kind: create
tags:
  - solution/repository-integration
  - element/entity-byidspec-cs
---

# Goals
- Provide a reusable named query for loading a single entity by its internal Id

# Core Principles
- Inherits `Specification<T>` from `Ardalis.Specification`
- Constructor receives filter parameter and calls `Query.Where(...)`

# Structure

## Project Structure
```
/{Module}.Application
  /Specifications
    {Entity}ByIdSpec.cs
```

# Implementation changes

```csharp
// {Module}.Application/Specifications/{Entity}ByIdSpec.cs
using Ardalis.Specification;

namespace {Module}.Application.Specifications;

public class {Entity}ByIdSpec : Specification<{Entity}>
{
    public {Entity}ByIdSpec(int id)
    {
        Query.Where(e => e.Id == id);
    }
}
```

# Rule changes

## MUST
- Inherit `Specification<T>`
- Be named `{Entity}ByIdSpec`
- All entity loading in handlers uses a named spec — no inline `Where(...)` LINQ
- All specifications for a module live in `/{Module}.Application/Specifications`
- Cross-module JOIN specs live in `/App.Queries/Specifications`
- Every entity loaded by Id has a `{Entity}ByIdSpec` in Application
- Projection specs use `Specification<T, TResult>` — entity filter specs use `Specification<T>`
- Spec name reflects query intent — not field names or implementation detail
- Never call the database or reference `DbContext` from a spec
- Never put business logic in a spec — filtering, ordering, and projection only
- Never write inline `Where(...)` LINQ in a handler — always delegate to a named spec
- Never place a spec in `{Module}.Domain` — all specs belong in `Application`
- Never generic spec names used across multiple entities (`GetByIdSpec`) — name per entity
- Never single-module specs live in App.Queries — they belong in the module's Application
# Check list
- [ ] Inherits `Specification<{Entity}>`
- [ ] Constructor applies `Query.Where(e => e.Id == id)`
- [ ] Lives in Application Specifications folder

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a reusable named query for loading a single entity by its internal Id
- [ ] WHEN applied THEN Inherits Specification<T> from Ardalis.Specification
- [ ] WHEN applied THEN Constructor receives filter parameter and calls Query.Where(...)
- [ ] WHEN applied THEN Never touches the database — describes what to fetch
- [ ] WHEN verified THEN Inherits Specification<{Entity}>
- [ ] WHEN verified THEN Constructor applies Query.Where(e => e.Id == id)
- [ ] WHEN verified THEN Lives in Application Specifications folder
