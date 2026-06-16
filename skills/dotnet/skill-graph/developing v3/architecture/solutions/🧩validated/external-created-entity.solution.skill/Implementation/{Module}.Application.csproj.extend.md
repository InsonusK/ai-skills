---
description: Add {Entity}ByGuidSpec and Create{Entity}GuidResolver
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Own `Create{Entity}GuidResolver` implementations in `/Resolvers` — one per external-created entity type
- Host `{Entity}ByGuidSpec` in `/Specifications`
- Register each resolver in the module DI registration

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /Create{Entity}
      Create{Entity}.Handler.cs
      Create{Entity}.Validator.cs
  /Specifications
    {Entity}ByGuidSpec.cs
  /Resolvers
    Create{Entity}GuidResolver.cs
  {Module}ApplicationRegistration.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Specifications/{Entity}ByGuidSpec.cs | Specification for looking up entity by Guid |
| /Resolvers/Create{Entity}GuidResolver.cs | Per-entity IGuidResolver implementation |

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Domain
- {Module}.Interfaces

# Rules

MUST:
- One `GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Each resolver registered in module DI registration
- All specs live in `/{Module}.Application/Specifications`

MUST NOT:
- Resolver implemented in Domain — resolver uses `IReadRepository<T>`, which belongs in Application
- Specs placed in Domain

# Anti-patterns
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application

# Check list
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Resolver registered in module DI
