---
description: Add single-module query handler in feature folder alongside command handlers
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/query-integration
  - element/module-application-csproj
---

# Goals
- Own single-module query handler implementations in `/Queries` alongside command handlers
- No validator alongside query handlers — queries are read-only

# Core Principles
- Query handlers located under `/Queries` — one folder per feature, separate from command handlers
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler` — same convention as commands
- `.Validator.cs` file is optional alongside query handlers — transport correctness only when needed
- Single-module handlers only — cross-module JOIN handlers live in App.Queries

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs     ← command handler
      CreateTask.Validator.cs   ← command validator
  /Queries
    /GetTask
      GetTask.Handler.cs        ← query handler
      GetTask.Validator.cs      ← optional transport validator
    /GetTasks
      GetTasks.Handler.cs       ← query handler
  /Specifications
    TaskByIdSpec.cs             ← repository-integration
    TaskSummarySpec.cs          ← repository-integration
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Queries/{FeatureName} | One folder per query feature; contains handler and optional transport validator |
| /Specifications | Named reusable specifications for entity loading and projection |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Handler implements `IRequestHandler<TQuery, Result<T>>` |
| `Ardalis.Result` | latest stable | Return typed results |
| `Ardalis.Specification` | latest stable | Named specs for loading and projection |

# Allowed Dependencies
- Shared — for `IQuery<T>` and `IReadRepository<T>`
- `{Module}.Domain` — for entity types used in specs
- `{Module}.Interfaces` — for query and DTO types

# Rules

## MUST
- Single-module query handlers live in `/{Module}.Application/Queries/{FeatureName}`
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
- Query handlers registered via `AddMediatR` assembly scan in module registration — same scan as command handlers
- Query handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Never query validators contain business rules — transport correctness only
- Never cross-module JOIN handlers live here — belongs in App.Queries
- Never reference `DbContext` directly

## SHOULD
- Avoid query handler placed outside `/Queries` — keep query handlers under `/Queries`
- Avoid `.Validator.cs` next to query handler — queries are read-only, no validation needed

# Check list
- [ ] Single-module handlers in `/{Module}.Application/Queries/{FeatureName}`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Optional `.Validator.cs` paired with query handler checks transport correctness only
- [ ] Query handlers inject `IReadRepository<T>`
