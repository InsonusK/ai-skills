---
description: Simple single-condition specification to load an entity by Id
project_name: "{Module}.Application"
name: "{Entity}ByIdSpec.cs"
element_kind: class
change_kind: create
---

# Goals
- Provide a reusable named query for loading a single entity by its internal Id

# Core Principals
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

# Rules

MUST:
- Inherit `Specification<T>`
- Live in `/{Module}.Application/Specifications`
- Be named `{Entity}ByIdSpec`

MUST NOT:
- Call the database or reference DbContext
- Contain business logic — filtering only

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
