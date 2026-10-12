---
name: plateau-dual-api-service--package-api-http
description: internal/api/http package of the plateau-dual-api-service plateau
whenToUse: when adding or editing a route/handler in internal/api/http, or deciding whether new HTTP-facing code belongs here
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20261012000000
tags:
  - skill/template/package
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Goal
Expose `internal/domain/services`' capabilities over plain HTTP/JSON.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/Package.create.md|internal/api/http]]

# Core Principles
No business rule or decision lives in this package — it decodes, calls the domain service, and encodes.

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
  server.go        ← Server, see plateau-dual-api-service--file-api-http-server.skill.md
  docs.go         ← the spec and Swagger UI routes, see plateau-dual-api-service--file-api-http-docs.skill.md
  protojson.go    ← proto-JSON in and out, see plateau-dual-api-service--file-api-http-protojson.skill.md
  features/
    http_api.feature   ← @type/api/http: the adapter's specification
  test/                ← godog runner, World, request steps, the operations step
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server`, `Options`, `contractRoutes()`: `GET /health`, `POST /v1/links/check` | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--file-api-http-server.skill.md]] |
| docs.go | `GET /openapi.json`, `GET /swagger/`, `GET /swagger` — only when docs are enabled | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--file-api-http-docs.skill.md]] |
| protojson.go | `readMessage`, `writeMessage`, `writeError`: proto-JSON and the canonical status table | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--file-api-http-protojson.skill.md]] |
| features/http_api.feature | The served operations against the contract; each route's status and body; an unknown field, an invalid and an unparseable request; the docs routes on, under `/prefix`, and off | — |
| test/ | Drives `Server.Handler()` through `net/http/httptest` over the real domain service | — |

# Allowed Dependencies
- `internal/domain/services`
- Standard library (`net/http`, `encoding/json`, `log/slog`)
- `docs/integration/api/linkcheck` (the embedded contract)
- `github.com/swaggest/swgui` (Swagger UI)
- `gen/api/linkcheck`, `google.golang.org/protobuf/encoding/protojson`, `google.golang.org/grpc/codes` and `status` (the contract's messages and codes)

# Rules
MUST:
- Never call anything under `internal/infrastructure` directly from this package.
- Every route of the contract is an entry of `contractRoutes()` and an operation of the contract; `/health` and the docs routes are outside the list.
- `features/http_api.feature` sends every request through `Server.Handler()` — never calls a handler function directly.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/Package.create.md#MUST|internal/api/http]]

# Check list
- [ ] No import of any `internal/infrastructure/*` package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/Package.create.md|internal/api/http]]
