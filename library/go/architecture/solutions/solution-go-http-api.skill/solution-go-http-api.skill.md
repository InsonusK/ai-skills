---
name: solution-go-http-api
description: The base inbound API — a thin stdlib net/http adapter over the domain service with its hand-written OpenAPI contract, the spec and Swagger UI served from the binary, and the wiring into the composition root
whenToUse: when giving a Go web-service its first inbound entry point, adding or changing an HTTP route of a service with no proto contract, or reviewing whether an HTTP handler contains business logic that belongs in the domain layer instead
domain: skill
type: architecture
version: 20261012000000
tags:
  - skill/architecture/solution
  - solution/go-http-api
  - stack/go
  - concern/architecture
creates:
  - "internal/api/http/"
  - "docs/integration/api/{contract-name}/"
extends:
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
depends_on:
  - "[[library/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
built_on_plateau:
adr:
  - adr/hand-written-openapi-at-baseline.md
  - adr/grpc-wire-form-at-baseline.md
---

# Goal
- `internal/api/http/`: a `net/http` adapter that decodes a request, calls the domain service, and translates the result or a domain sentinel error into an HTTP response.
- `docs/integration/api/{contract-name}/openapi.json`, written by hand, and `embed.go` beside it.
- `GET /openapi.json`, `GET /swagger/`, and `GET /swagger` registered only when `HTTP_API_DOCS_ENABLED=true`, every URL they emit carrying `HTTP_BASE_PATH`.
- `internal/api/http/features/http_api.feature`, holding the served routes against the contract and the wire form of every route.
- The HTTP server started from `main.go`'s `run()`, listening until a shutdown signal.

# Capabilities
- The module answers HTTP requests without any business logic living in the handler layer.
- A consumer reads the contract from the repository, and — where the deployment switches it on — from the running service and its Swagger UI.
- A `/health` endpoint that doubles as a "the process finished wiring everything" signal, since it only starts serving once every adapter above it in `run()` is constructed.

# Core Principles
- The HTTP layer is a thin translator: decode, call the domain service, encode — no rule or decision lives here.
- Every domain sentinel error this layer knows about is translated to a grpc status code, and the HTTP status follows from the code; anything else is `INTERNAL`, 500.
- The contract rules are those of [[library/common-workflow/develop/api-develop.skill/api-develop.skill.md|api-develop]] and [[library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md|api-develop-in-go]]; this solution states which files carry them.
- The JSON this adapter writes by hand is the JSON `protojson` writes once [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] is composed — decision recorded in [[./adr/grpc-wire-form-at-baseline.md|grpc-wire-form-at-baseline]].
- The spec is hand-written while the service has no proto file — decision recorded in [[./adr/hand-written-openapi-at-baseline.md|hand-written-openapi-at-baseline]].

# Requirements
SOLUTION:
- [[library/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|Domain logic solution]]
  - [[library/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{Service}]]
    - `{Service}` - the domain service this adapter calls
GO MODULES / STANDARD LIBRARY:
- `net/http` (standard library) — `http.ServeMux`'s Go 1.22+ method+pattern syntax, no third-party router
- `encoding/json` (standard library) — request/response bodies
- `github.com/swaggest/swgui` — Swagger UI with its assets embedded, package `v5emb`

# Template Skill Mutations
FILES:
- [[./Implementation/internal/api/http/Package.create.md|internal/api/http]] - create - the HTTP adapter package and its feature
- [[./Implementation/internal/api/http/server.go.create.md|server.go]] - create - `Server`, `Options`, the contract route list, `Handler()`
- [[./Implementation/internal/api/http/docs.go.create.md|docs.go]] - create - the spec and Swagger UI routes
- [[./Implementation/internal/api/http/errors.go.create.md|errors.go]] - create - the error body and the status mapping
- [[./Implementation/docs/integration/api/{contract-name}/Package.create.md|docs/integration/api/{contract-name}]] - create - the hand-written contract and its embed package
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - construct the domain service and the HTTP server, listen until shutdown
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add `HTTPListenPort`, `HTTPAPIDocsEnabled`, `HTTPBasePath`

# Workflow

## Handle a request (happy path)
1. A client sends an HTTP request matching a route of `contractRoutes()`.
2. `Server.handle{Method}` decodes the request body (if any).
3. It calls the domain service.
4. It encodes the result as the response body and writes 201, 202, or 200 by what the method did.

## Domain error translation
1. The domain service returns an error.
2. The handler maps a known sentinel error to its grpc code (e.g. "not found" → `NOT_FOUND`); anything else maps to `INTERNAL`.
3. `writeError` writes `{"code": …, "message": "…", "details": []}` at the HTTP status the canonical table gives the code.

## Read the contract from a running service
1. The deployment sets `HTTP_API_DOCS_ENABLED=true`, and `HTTP_BASE_PATH` to the ingress path.
2. `GET {base path}/swagger/` reaches the service as `/swagger/`; the page it answers names `{base path}/openapi.json` and its assets under `{base path}/swagger/`.
3. `GET /openapi.json` answers the embedded spec with `servers[0].url` set to the base path.

# Rules

## MUST
- [[./Implementation/internal/api/http/Package.create.md#MUST|internal/api/http]]
- [[./Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[./Implementation/internal/api/http/docs.go.create.md#MUST|docs.go]]
- [[./Implementation/internal/api/http/errors.go.create.md#MUST|errors.go]]
- [[./Implementation/docs/integration/api/{contract-name}/Package.create.md#MUST|docs/integration/api/{contract-name}]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `internal/api/http` imports the domain package and nothing under `internal/infrastructure`.
- [ ] `GET /health` returns `200` once `run()` has finished constructing every adapter.
- [ ] `make build && make run` starts the HTTP server and it answers `/health`.
- [ ] Every route of `contractRoutes()` is an operation of `openapi.json` and the reverse; the operations scenario passes.
- [ ] With no variable set, `/openapi.json` and `/swagger/` answer 404.
- [ ] Started with `HTTP_API_DOCS_ENABLED=true HTTP_BASE_PATH=/prefix`, the binary answers `/openapi.json`, `/swagger/`, and a Swagger UI asset, each naming `/prefix`.
