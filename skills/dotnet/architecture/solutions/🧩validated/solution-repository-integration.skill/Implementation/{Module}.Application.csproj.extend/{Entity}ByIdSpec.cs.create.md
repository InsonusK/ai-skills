---
description: Simple single-condition specification to load an entity by Id
project_name: "{Module}.Application"
name: "{Entity}ByIdSpec.cs"
element_kind: class
change_kind: create
---

# Goals
- Provide a reusable named query for loading a single entity by its internal Id

# Core Principles
- Inherits `Specification<T>` from `Ardalis.Specification`
- Constructor receives filter parameter and calls `Query.Where(...)`
- Never touches the database — describes what to fetch

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
## MUST NOT
- Contain business logic — filtering only
- Spec call the database or reference DbContext
- Spec contain business logic — filtering, ordering, and projection only
- Handler contain inline `Where(...)` LINQ — always delegate to a named spec
- Specs placed in `{Module}.Domain` — all specs belong in Application
- Generic spec names used across multiple entities (`GetByIdSpec`) — name per entity
- Single-module specs live in App.Queries — they belong in the module's Application
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
