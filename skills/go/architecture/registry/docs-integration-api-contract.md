---
name: registry-docs-integration-api-contract
description: Conflict Detection result for the `docs-integration-api-contract` element
tags:
  - concern/architecture
  - stack/go
  - element/docs-integration-api-contract
---

# Element
`docs-integration-api-contract` — the folder `docs/integration/api/{contract-name}/`.

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] (`.create`)
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`)

`solution-external-integration` and `solution-persistent-db` add a field or an operation to the contract through their own `server.go.extend.md` deltas; neither has a delta file for this element, so they are not grouped here.

# Classification
`FMN`. **F**: no Constraint between `HttpApi` (common baseline) and VP1 `GrpcApi`. **M**: the delta deletes the hand-written `openapi.json`, adds `{file}.proto` and the `{file}.swagger.json` generated from it, and changes the one `//go:embed` line of `embed.go`. **N**: one dependent delta over the base's create — the package name, the exported `Spec`, and every importer stay as they are.

# Ordering
`source: constraint` — trivial: `solution-grpc-api` `depends_on` `solution-go-http-api`, and replaces what that solution created.

# Resolution
Canonical — no resolver needed. The rule that carries the risk is `solution-grpc-api`'s own: the proto file takes its version, paths, and fields from the `openapi.json` it replaces. Verified: `plateau-http-service`'s `http_api.feature` passes on `plateau-dual-api-service` with one changed cell — the docs row reads `"basePath":"/prefix"` (OpenAPI 2, generated) where it read `"url":"/prefix"` (OpenAPI 3, hand-written).

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-http-service` | 1 | `solution-go-http-api` creates the folder with `openapi.json` and `embed.go` | the operations scenario; the binary serving `/openapi.json` under a base path |
| `plateau-dual-api-service` | 2 | First real: `solution-grpc-api` replaces the spec by the proto file and its generated spec | `make proto-gen`, `make test-kind-generated`, the unchanged HTTP feature |
| `plateau-integrated-service` … `gw009-001` | 2 | Unchanged set; the contract gains `flagged`/`reason` and, from `plateau-persistent-service`, `RecentChecks` | `make test-kind-generated` and the HTTP feature in each |
