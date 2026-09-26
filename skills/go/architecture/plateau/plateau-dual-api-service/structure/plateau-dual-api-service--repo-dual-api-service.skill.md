---
name: plateau-dual-api-service--repo-dual-api-service
description: Repository-root layout of the plateau-dual-api-service plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260924000000
tags:
  - skill/template/repo
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Structure

## Repository Structure
```
go.mod
Makefile
.gitignore
report-template/
  index.html
proto/
  linkcheck/
    linkcheck.proto                ← this module's own exposed gRPC contract
buf/
  buf.gen.yaml
gen/
  api/                             (generated; committed, not gitignored)
cmd/
  {service}/
    main.go                       ← file tier, see plateau-dual-api-service--file-cmd-{service}-main.skill.md
internal/
  config/
    config.go                     ← file tier
  version/
    version.go                    ← file tier
  logging/
    logger.go                     ← file tier
  domain/
    services/                     ← package tier, see plateau-dual-api-service--package-domain-services.skill.md
  api/
    http/                         ← package tier, see plateau-dual-api-service--package-api-http.skill.md
    grpc/                         ← package tier, see plateau-dual-api-service--package-api-grpc.skill.md
tools/
  normalize_unittest/main.go
  normalize_scenarios/main.go
  normalize_mutation/main.go
  test_report/main.go
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]]
- [[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/go/test/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md|Repository]]

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/services | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-api-http.skill.md]] | Inbound HTTP adapter |
| internal/api/grpc | [[skills/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-api-grpc.skill.md]] | Inbound gRPC adapter |

# Rules

MUST:
- `unit-test`/`mutation-test`/`test-report`/`test-and-report` are the only testing-related `Makefile` targets; `build`/`run`/`lint`/`proto-gen` are the only lifecycle targets. No solution redefines an existing target — each adds its own.
- `report-template/index.html` is a static asset, copied verbatim by `tools/test_report` into `public/index.html` — never generated.
- `make unit-test` writes `tmp/result/scenarios.json` on every run, green or red; every scenario (or `Examples:` block) carries exactly one type tag.
- `proto/linkcheck/linkcheck.proto` stays flat (no version subdirectory) — its path must match `go_package`'s flat `gen/api` exactly, or `buf generate` emits code at a different Go import path than every hand-written file expects (verified — this is the actual, observed failure mode, not a hypothetical). `buf/buf.gen.yaml` uses `local:` plugins (`protoc-gen-go`, `protoc-gen-go-grpc`, both `go install`ed), not `buf.build` remote plugins.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/go/test/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] `make proto-gen` regenerates `gen/api` from `proto/linkcheck/linkcheck.proto` with no manual edits needed afterward (verified against this plateau's own `example/`).
