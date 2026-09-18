---
name: plateau-persistent-service--package-api-http
description: internal/api/http package of the plateau-persistent-service plateau
whenToUse: when adding or editing a route/handler in internal/api/http, or deciding whether new HTTP-facing code belongs here
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/package
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
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
  server.go        ← Server, see plateau-persistent-service--file-api-http-server.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server`: `GET /health`, `POST /v1/links/check`, `GET /v1/links/recent` | [[skills/go/architecture/plateau/plateau-persistent-service/structure/plateau-persistent-service--file-api-http-server.skill.md]] |

# Allowed Dependencies
- `internal/domain/services`
- Standard library (`net/http`, `encoding/json`, `log/slog`)

# Rules
MUST:
- Never call anything under `internal/infrastructure` directly from this package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/Package.create.md#MUST|internal/api/http]]

# Check list
- [ ] No import of any `internal/infrastructure/*` package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/Package.create.md|internal/api/http]]
