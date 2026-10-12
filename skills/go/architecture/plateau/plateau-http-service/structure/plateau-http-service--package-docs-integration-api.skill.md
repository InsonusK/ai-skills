---
name: plateau-http-service--package-docs-integration-api
description: docs/integration/api/linkcheck — the service's own API contract of the plateau-http-service plateau
whenToUse: when adding or changing a method, a field, or a route of the service's own API, bumping its version, or deciding where a contract file belongs
domain: skill
type: template
plateau: plateau-http-service
version: 20261012000000
tags:
  - skill/template/package
  - plateau/plateau-http-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
---

# Goal
The service's own published HTTP contract, written by hand, and the package that embeds it into the binary.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/docs/integration/api/{contract-name}/Package.create.md|docs/integration/api/{contract-name}]]

# Core Principles
The folder is the contract: a consumer copies it, the binary embeds it, a reviewer reads it. Its rules are those of `api-develop` and `api-develop-in-go`.

# Structure
## Repository place
```
docs/
  integration/
    api/
      linkcheck/
```
## Package Structure
```
docs/integration/api/linkcheck/
  openapi.json     ← hand-written, OpenAPI 3; info.version 1.0.0
  embed.go         ← package linkcheck, exports Spec
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| openapi.json | The contract: `POST /v1/links/check`, its request, response, and `Status` error schema | — |
| embed.go | `//go:embed openapi.json` into `Spec []byte` | — |

# Allowed Dependencies
- Standard library (`embed`)

# Rules
MUST:
- `openapi.json` is edited, and its `info.version` bumped, in the change that adds, renames, or reshapes a route or a request or response struct of `internal/api/http`.
- `openapi.json` holds no `servers` entry — the prefix of one deployment is set in the served copy.
- No `.proto` file lives here: it arrives with `solution-grpc-api`, which deletes `openapi.json` in the same change.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/docs/integration/api/{contract-name}/Package.create.md#MUST|docs/integration/api/{contract-name}]]

# Check list
- [ ] `info.version` is `major.minor.patch`, its major the `/v1/` of the paths.
- [ ] The operations scenario of `internal/api/http/features/http_api.feature` passes.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/docs/integration/api/{contract-name}/Package.create.md|docs/integration/api/{contract-name}]]
