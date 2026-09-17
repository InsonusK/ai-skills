---
name: solution-go-http-api
description: The base inbound API — a thin stdlib net/http adapter over the domain service, plus wiring it into the composition root
whenToUse: when giving a Go web-service its first inbound entry point, or reviewing whether an HTTP handler contains business logic that belongs in the domain layer instead
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-http-api
  - stack/go
  - concern/architecture
creates:
  - "internal/api/http/"
extends:
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
built_on_plateau:
adr:
---

# Goal
- Give the module its base inbound entry point: a `net/http` adapter that decodes a request, calls the domain service, and translates the result (or a domain sentinel error) back into an HTTP response.
- Start the HTTP server from `main.go`'s `run()`, listening until a shutdown signal.

# Capabilities
- The module answers HTTP requests without any business logic living in the handler layer.
- A `/health` endpoint that doubles as a "the process finished wiring everything" signal, since it only starts serving once every adapter above it in `run()` is constructed.

# Core Principles
- The HTTP layer is a thin translator: decode, call the domain service, encode — no rule or decision lives here.
- Every domain sentinel error this layer knows about is translated to a specific HTTP status; anything else maps to `500`.

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|Domain logic solution]]
  - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{Service}]]
    - `{Service}` - the domain service this adapter calls
GO MODULES / STANDARD LIBRARY:
- `net/http` (standard library) — `http.ServeMux`'s Go 1.22+ method+pattern syntax, no third-party router
- `encoding/json` (standard library) — request/response bodies

# Template Skill Mutations
FILES:
- [[./Implementation/internal/api/http/Package.create.md|internal/api/http]] - create - the HTTP adapter package
- [[./Implementation/internal/api/http/server.go.create.md|server.go]] - create - `Server` struct wrapping the domain service, `Handler()` builds the mux
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - construct the domain service and the HTTP server, listen until shutdown
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add `HTTPListenPort`

# Workflow

## Handle a request (happy path)
1. A client sends an HTTP request matching a registered route.
2. `Server.handle{Method}` decodes the request body (if any).
3. It calls the domain service.
4. It encodes the result as the response body and writes `200`.

## Domain error translation
1. The domain service returns an error.
2. The handler maps a known sentinel error to its HTTP status (e.g. "not found" → `404`); anything else maps to `500`.
3. The handler writes a JSON `{"error": "..."}` body at that status.

# Rules

## MUST
- [[./Implementation/internal/api/http/Package.create.md#MUST|internal/api/http]]
- [[./Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `internal/api/http` imports the domain package and nothing under `internal/infrastructure`.
- [ ] `GET /health` returns `200` once `run()` has finished constructing every adapter.
- [ ] `make build && make run` starts the HTTP server and it answers `/health`.
