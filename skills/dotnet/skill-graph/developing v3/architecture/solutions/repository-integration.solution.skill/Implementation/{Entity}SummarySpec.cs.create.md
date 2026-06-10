---
description: Application-layer projection specification mapping entity to DTO
name: "{Entity}SummarySpec.cs"
change_kind: create
---

# Goals
- Project entity data directly to a DTO inside the query
- Avoid loading full entity when only read data is needed

# Core Principals
- Inherits `Specification<T, TResult>` — second type parameter is the DTO
- Uses `Query.Select(...)` to define projection
- Repository executes projection at the database level

# Structure

## Project Structure
```
/{Module}.Application
  /Specifications
    {Entity}SummarySpec.cs
```

# Implementation changes

```csharp
// {Module}.Application/Specifications/{Entity}SummarySpec.cs
using Ardalis.Specification;

namespace {Module}.Application.Specifications;

public class {Entity}SummarySpec : Specification<{Entity}, {Entity}SummaryDto>
{
    public {Entity}SummarySpec(int ownerId)
    {
        Query
            .Where(e => e.OwnerId == ownerId)
            .Select(e => new {Entity}SummaryDto(e.Id, e.Name, e.Status.ToString()));
    }
}
```

# Rules

MUST:
- Inherit `Specification<T, TResult>`
- Define projection via `Query.Select(...)`
- Live in `/{Module}.Application/Specifications`

MUST NOT:
- Load full entity when only DTO fields needed
- Define DTO inline in the spec — DTO belongs in Interfaces

# Check list
- [ ] Inherits `Specification<{Entity}, {Entity}SummaryDto>`
- [ ] `Query.Select(...)` defines projection
- [ ] Lives in Application Specifications folder
