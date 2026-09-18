---
name: plateau-dual-api-service--package-api-grpc
description: internal/api/grpc package of the plateau-dual-api-service plateau
whenToUse: when adding or editing an RPC handler in internal/api/grpc, or deciding whether new gRPC-facing code belongs here
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/package
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Goal
Expose `internal/domain/services`' capabilities over gRPC, sharing the exact domain-service instance `internal/api/http` uses.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/Package.create.md|internal/api/grpc]]

# Core Principles
No business rule or decision lives in this package — it decodes, calls the domain service, and encodes.

# Structure
## Repository place
```
internal/
  api/
    grpc/
```
## Package Structure
```
internal/api/grpc/
  server.go        ← Server, see plateau-dual-api-service--file-api-grpc-server.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server`: implements the generated `LinkCheckServiceServer`, one RPC (`Check`) | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--file-api-grpc-server.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| google.golang.org/grpc | >= 1.83 | hosts the generated service |
| google.golang.org/protobuf | >= 1.36 | generated message types |

# Allowed Dependencies
- `internal/domain/services`
- `gen/api` (this module's own generated stubs)
- `google.golang.org/grpc`, `google.golang.org/grpc/codes`, `google.golang.org/grpc/status`

# Rules
MUST:
- Never call anything under `internal/infrastructure` directly from this package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/Package.create.md#MUST|internal/api/grpc]]

# Check list
- [ ] No import of any `internal/infrastructure/*` package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/Package.create.md|internal/api/grpc]]
