---
description: The domain's outbound-ports package — created empty, one file added per port by whichever solution needs it
name: internal/domain/interfaces
element_kind: package
change_kind: create
tags:
  - solution/go-domain-ports
  - element/internal-domain-interfaces
---

# Goals
- Give the domain a package to declare what it needs from the outside world, without knowing which infrastructure will provide it.

# Core Principles
- Declares interfaces (and the value types/sentinel errors they use) only — never an implementation.

# Structure

## Repository place
```
internal/
  domain/
    interfaces/
```

## Package Structure
```
internal/domain/interfaces/
  (empty at creation — one file per port, added by whichever solution needs it)
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| (none yet) | — | — |

# What Does NOT Belong Here
- Any concrete type from an infrastructure library (a Redis client, a gRPC connection, an HTTP client) — belongs to the adapter package that implements the port.
- Business logic — belongs to `internal/domain/services`.

# Allowed Dependencies
- Standard library only (`context`, `errors`).

# Rules

## MUST
- Every interface added to this package must be named for the domain's need, never for the technology that will implement it.
  - Risk: a technology-named port (e.g. `RedisChatStore`) leaks an infrastructure decision into the domain layer, so swapping the technology means renaming a domain-level type every caller already imports.
  - Fix: name the interface after its business purpose (e.g. `ChatStore`); the adapter's own package name (`redisstore`) carries the technology.
- Never let this package import anything under `internal/infrastructure` or `internal/api`.
  - Risk: an import in either direction turns the dependency inversion this package exists for into a cycle or a fiction.
  - Fix: keep imports to the standard library only.

# Check list
- [ ] The package compiles with a doc comment and no exported symbol until a port-needing solution adds one.
- [ ] No import outside the standard library.
