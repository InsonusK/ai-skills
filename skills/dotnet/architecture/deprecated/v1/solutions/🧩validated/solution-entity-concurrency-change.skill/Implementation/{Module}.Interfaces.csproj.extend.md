---
description: Require update and patch commands to implement IHasVersions
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/module-interfaces-csproj
---

# Goals
- Extend all update and patch commands with `IHasVersions` to carry client-supplied version information

# Core Principles
- `Versions` property typed as `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>`
- Populated by the API controller from the decoded `If-Match` header — never hardcoded
- Create and delete commands do NOT implement `IHasVersions` — only update and patch

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    {Command}.cs    ← extended with IHasVersions
```

# Allowed Dependencies
- Shared

# Rules

## MUST
- All update and patch commands implement `IHasVersions`

## MUST NOT
- Create commands implement `IHasVersions` — new entities have no version
- Delete commands implement `IHasVersions` — deletion does not require version check in this architecture

# Anti-patterns
- `Versions` constructed in application code instead of passed from controller

# Check list
- [ ] All update commands implement `IHasVersions`
- [ ] No create or delete command implements `IHasVersions`
