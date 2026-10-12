---
description: The service's own published HTTP contract, hand-written, and the package that embeds it
name: "docs/integration/api/{contract-name}"
element_kind: package
change_kind: create
tags:
  - solution/go-http-api
  - element/docs-integration-api-contract
---

# Goals
- Give the service's HTTP API one published description, in the place `api-develop` names, that the binary serves unchanged.

# Core Principles
- The folder is the contract: a consumer copies it, the binary embeds it, a reviewer reads it.

# Structure

## Repository place
```
docs/
  integration/
    api/
      {contract-name}/
```

## Package Structure
```
docs/integration/api/{contract-name}/
  openapi.json     ← hand-written, OpenAPI 3
  embed.go         ← package {contract-package}, exports Spec
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| openapi.json | One path item per route of `contractRoutes()`; `info.version` a full `major.minor.patch` whose major is the routes' `/v{major}/`; every error response the `Status` schema | — |
| embed.go | The template of [[skills/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md|api-develop-in-go]] with `{spec-file}` = `openapi.json` | — |

# What Does NOT Belong Here
- A `servers` entry or any other value of one deployment.
- A `.proto` file: it arrives with [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]], which deletes `openapi.json` in the same change.

# Allowed Dependencies
- Standard library (`embed`)

# Rules

## MUST
- Edit `openapi.json` and bump its `info.version` in the same change that adds, renames, or reshapes a route or a request or response struct.
  - Risk: the operations scenario compares methods and paths only; a field renamed in a struct and not in the spec passes every test.
  - Fix: struct and schema in one commit; minor for an addition, major — a new `/v{n}/` beside the old one — for an incompatible change.
- Name `{contract-name}` in kebab-case after the capability, and `{contract-package}` as the same name without hyphens.
  - Risk: a Go package cannot be named with a hyphen, and a name derived differently in two services cannot be guessed.
  - Fix: `link-check` → package `linkcheck`.

# Check list
- [ ] `openapi.json` has no `servers` key.
- [ ] `info.version` is `major.minor.patch` and its major equals the `/v{major}/` of the paths.
- [ ] Searching `embed.go` for `{` returns nothing.
