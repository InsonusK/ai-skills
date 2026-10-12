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
- Hold the adapter against its published contract by one feature.

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
  docs.go
  errors.go
  features/
    http_api.feature
  test/
    runner_test.go
    world_test.go
    request_steps_test.go
    contract_steps_test.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server` and `Options`; the contract route list; `Handler()` builds the `http.ServeMux` | [[./server.go.create.md]] |
| docs.go | The spec and Swagger UI routes, registered by `Handler()` when docs are enabled | [[./docs.go.create.md]] |
| errors.go | The `google.rpc.Status` error body and the code-to-status mapping | [[./errors.go.create.md]] |
| features/http_api.feature | `@type/api/http` — the adapter's specification | — |
| test/ | The godog runner, the `World` that builds the `Server` over the real domain service, the request steps, and the operations step of [[skills/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md|api-develop-in-go]] | — |

# What Does NOT Belong Here
- Business logic or validation beyond decoding — belongs to `internal/domain/services`.

# Allowed Dependencies
- `internal/domain/services`
- `internal/domain/interfaces` (for sentinel errors, once it exists)
- `docs/integration/api/{contract-name}` (the embedded contract)
- `github.com/swaggest/swgui`
- Standard library (`net/http`, `encoding/json`, `log/slog`)

# Rules

## MUST
- Never call anything under `internal/infrastructure` directly from this package.
  - Risk: an HTTP handler that reaches past the domain service into infrastructure duplicates whatever the domain service already orchestrates and drifts from it.
  - Fix: route every request through the domain service; let the domain service depend on infrastructure through its own ports.
- `features/http_api.feature` must hold the scenarios listed under [Unittest TestCases](#unittest-testcases), driving `Server.Handler()` through `net/http/httptest` over the real domain service with its outbound ports stubbed.
  - Violation: the adapter covered only by the domain feature; a scenario that calls a handler function directly.
  - Risk: the status codes, the error body, and the docs routes are what a client depends on and nothing else specifies them; a call past `Handler()` skips the route patterns.
  - Fix: the steps build the `Server` with `New` and send every request through `Handler().ServeHTTP`.

# Check list
- [ ] No import of any `internal/infrastructure/*` package.
- [ ] The feature carries `@type/api/http` on its `Feature:` line and every scenario or `Examples:` block one `@category/…` tag.

# Unittest TestCases
- [ ] WHEN the operations of the table are compared THEN `ContractRoutes()` and the embedded spec both have exactly those operations
- [ ] WHEN each route is called with a valid request THEN it answers its success status and the body's fields
- [ ] WHEN a request carries a field the contract does not know THEN it succeeds
- [ ] WHEN a request is invalid for the domain THEN it answers 400 with `code` 3
- [ ] WHEN a request body cannot be parsed THEN it answers 400 with `code` 3
- [ ] WHEN `GET /health` is called THEN it answers 200 with the binary's version
- [ ] WHEN docs are on with no base path THEN `/openapi.json` and `/swagger/` answer 200
- [ ] WHEN docs are on under `/prefix` THEN the spec's `servers[0].url` is `/prefix`, the page names `/prefix/openapi.json`, and `/swagger` redirects to `/prefix/swagger/`
- [ ] WHEN docs are off THEN `/openapi.json` and `/swagger/` answer 404
