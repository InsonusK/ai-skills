---
description: Add the DomainException type used by every guarded entity method
name: "Shared.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-behaviour
  - element/shared-csproj
---

# Goals
- Give every module one exception type for an invariant violation, so `solution-mediator-exception-handler` has a single type to recognise.

# Structure

## Project Structure
```
/Shared
  /Exceptions
    DomainException.cs
```

# Allowed Dependencies
- None (BCL only).

# Rules

## MUST
- Place `DomainException` in `Shared/Exceptions`, not in any module.
  - Risk: a per-module exception type forces the exception handler to know every module's type.
  - Fix: one shared type, thrown by every module's entities.

# Check list
- [ ] `Shared/Exceptions/DomainException.cs` exists.
