---
description: Generic Ardalis Specification that loads any entity by its integer Id
project_name: BuildingBlocks
name: EntityByIdSpec.cs
element_kind: class
change_kind: create
---

# Goals
- Provide a single generic specification that `ConcurrencyBehavior` can instantiate at runtime for any entity type
- Avoid hardcoding per-entity `ByIdSpec` types inside the pipeline behavior

# Core Principles
- Uses reflection over `Expression` trees to build the `Id == value` predicate
- No EF Core dependency — relies only on `Ardalis.Specification` from Shared and `System.Linq.Expressions`
- Constructor accepts the integer `id` value

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Generic by-Id specification | `EntityByIdSpec<T>` | `EntityByIdSpec<T>` | `EntityByIdSpec.cs` | `EntityByIdSpec.cs` |

# Implementation changes

```csharp
// BuildingBlocks/Specifications/EntityByIdSpec.cs
using System.Linq.Expressions;
using Ardalis.Specification;

namespace BuildingBlocks.Specifications;

public class EntityByIdSpec<T> : Specification<T>
    where T : class
{
    public EntityByIdSpec(int id)
    {
        var param = Expression.Parameter(typeof(T), "e");
        var idProperty = typeof(T).GetProperty("Id")!;

        var lambda = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(
                Expression.Property(param, idProperty),
                Expression.Constant(id)),
            param);

        Query.Where(lambda);
    }
}
```

> **Note:** This specification assumes every versioned entity has an `Id` property of type `int`. If an entity uses a different key type, extend the constructor overload or use a dedicated spec.

# Rules

MUST:
- Live in `BuildingBlocks/Specifications`
- Be generic over any class with an `Id` property
- Build the predicate using `Expression` trees — no EF Core dependency

MUST NOT:
- Reference EF Core directly
- Be used as a replacement for per-entity specs in handlers — it is intended only for the dynamic `ConcurrencyBehavior`

# Anti-patterns
- Using `EntityByIdSpec<T>` in handlers instead of domain-named specs like `TaskByIdSpec` — handlers should use intent-revealing specs

# Check list
- [ ] `EntityByIdSpec<T>` defined in `BuildingBlocks/Specifications/EntityByIdSpec.cs`
- [ ] Constructor accepts `int id`
- [ ] No EF Core reference

# Unittest TestCases
- [ ] WHEN applied THEN Provide a single generic specification that ConcurrencyBehavior can instantiate at runtime for any entity type
- [ ] WHEN applied THEN Avoid hardcoding per-entity ByIdSpec types inside the pipeline behavior
- [ ] WHEN applied THEN Uses reflection over Expression trees to build the Id == value predicate
- [ ] WHEN applied THEN No EF Core dependency — relies only on Ardalis.Specification from Shared and System.Linq.Expressions
- [ ] WHEN verified THEN EntityByIdSpec<T> defined in BuildingBlocks/Specifications/EntityByIdSpec.cs
- [ ] WHEN verified THEN Constructor accepts the integer id value
- [ ] WHEN naming 'Generic by-Id specification' THEN pattern matches convention
