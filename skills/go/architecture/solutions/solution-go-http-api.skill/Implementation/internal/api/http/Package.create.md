---
description: The inbound HTTP adapter package
name: internal/api/http
element_kind: package
change_kind: create
tags:
  - solution/go-http-api
  - element/internal-api-http
---

# Goals
- Expose the domain service's capabilities over plain HTTP/JSON.

# Core Principles
- No business rule or decision lives in this package — it decodes, calls the domain service, and encodes.

# Structure

## Repository place
```
internal/
  api/
    http/
```

## Package Structure
```
internal/api/http/
  server.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server` struct wrapping the domain service; `Handler()` builds the `http.ServeMux` | [[./server.go.create.md]] |

# What Does NOT Belong Here
- Business logic or validation beyond decoding — belongs to `internal/domain/services`.

# Allowed Dependencies
- `internal/domain/services`
- `internal/domain/interfaces` (for sentinel errors, once it exists)
- Standard library (`net/http`, `encoding/json`, `log/slog`)

# Rules

## MUST
- Never call anything under `internal/infrastructure` directly from this package.
  - Risk: an HTTP handler that reaches past the domain service into infrastructure duplicates whatever the domain service already orchestrates and drifts from it.
  - Fix: route every request through the domain service; let the domain service depend on infrastructure through its own ports.

# Check list
- [ ] No import of any `internal/infrastructure/*` package.
