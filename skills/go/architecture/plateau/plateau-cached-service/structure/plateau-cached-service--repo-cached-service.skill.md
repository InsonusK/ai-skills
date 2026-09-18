---
name: plateau-cached-service--repo-cached-service
description: Repository-root layout of the plateau-cached-service plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/repo
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
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
  reputation/
    reputation.proto                ← the EXTERNAL reputation service's own contract, vendored
buf/
  buf.gen.yaml
  reputation.gen.yaml
gen/
  api/                             (generated; committed, not gitignored)
  reputation/                      (generated; committed, not gitignored)
cmd/
  {service}/
    main.go                       ← file tier, see plateau-cached-service--file-cmd-{service}-main.skill.md
internal/
  config/
    config.go                     ← file tier
  version/
    version.go                    ← file tier
  logging/
    logger.go                     ← file tier
  domain/
    interfaces/                   ← package tier, see plateau-cached-service--package-domain-interfaces.skill.md
    services/                     ← package tier, see plateau-cached-service--package-domain-services.skill.md
  api/
    http/                         ← package tier, see plateau-cached-service--package-api-http.skill.md
    grpc/                         ← package tier, see plateau-cached-service--package-api-grpc.skill.md
  infrastructure/
    reputationclient/             ← package tier, see plateau-cached-service--package-infrastructure-reputationclient.skill.md
    reputationcache/              ← package tier, see plateau-cached-service--package-infrastructure-reputationcache.skill.md
tools/
  normalize_unittest/main.go
  normalize_mutation/main.go
  test_report/main.go
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]]
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] - [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/Implementation/Repository.extend.md|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md|Repository]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md|Repository]]

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/interfaces | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-domain-interfaces.skill.md]] | Outbound ports |
| internal/domain/services | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-api-http.skill.md]] | Inbound HTTP adapter |
| internal/api/grpc | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-api-grpc.skill.md]] | Inbound gRPC adapter |
| internal/infrastructure/reputationclient | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-infrastructure-reputationclient.skill.md]] | Outbound gRPC-client adapter |
| internal/infrastructure/reputationcache | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--package-infrastructure-reputationcache.skill.md]] | Outbound Redis-backed cache adapter |

# Rules

MUST:
- `unit-test`/`mutation-test`/`test-report`/`test-and-report` are the only testing-related `Makefile` targets; `build`/`run`/`lint`/`proto-gen` are the only lifecycle targets. No solution redefines an existing target — each adds its own.
- `report-template/index.html` is a static asset, copied verbatim by `tools/test_report` into `public/index.html` — never generated.
- `proto/linkcheck/linkcheck.proto` stays flat (no version subdirectory) — its path must match `go_package`'s flat `gen/api` exactly, or `buf generate` emits code at a different Go import path than every hand-written file expects (verified — this is the actual, observed failure mode, not a hypothetical). `buf/buf.gen.yaml` uses `local:` plugins (`protoc-gen-go`, `protoc-gen-go-grpc`, both `go install`ed), not `buf.build` remote plugins.
- `proto/reputation/reputation.proto` (the external service's own contract) is generated into its own `gen/reputation` via its own `buf/reputation.gen.yaml` — never merged with `gen/api`. Its `buf generate` call was added as a second line inside the existing `proto-gen` target, not a second target declaration.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] - [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] `make proto-gen` regenerates both `gen/api` and `gen/reputation` with no manual edits needed afterward (verified against this plateau's own `example/`).
