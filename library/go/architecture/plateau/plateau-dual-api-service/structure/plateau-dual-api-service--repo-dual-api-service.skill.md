---
name: plateau-dual-api-service--repo-dual-api-service
description: Repository-root layout of the plateau-dual-api-service plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20261012000000
tags:
  - skill/template/repo
  - plateau/plateau-dual-api-service
created_by:
  - "[[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
  - "[[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Structure

## Repository Structure
```
go.mod
Makefile
.gitignore
report-template/
  index.html
buf.yaml                         ← the buf modules: docs/integration/api
buf.lock
buf.gen.yaml                     ← own contract → gen/api and the spec beside the proto file
docs/
  integration/
    api/
      linkcheck/                 ← package tier, see plateau-dual-api-service--package-docs-integration-api.skill.md
gen/
  api/
    linkcheck/                   (generated; committed, not gitignored)
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
  testing/kinds/generated.sh      ← the generated test kind, copied unchanged from api-develop-in-go
  normalize_unittest/main.go
  normalize_scenarios/main.go
  normalize_mutation/main.go
```

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[library/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]]
- [[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[library/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md|Repository]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md|Repository]]

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/services | [[library/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[library/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-api-http.skill.md]] | Inbound HTTP adapter |
| docs/integration/api/linkcheck | [[library/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-docs-integration-api.skill.md]] | The service's own API contract and its embed package |
| internal/api/grpc | [[library/go/architecture/plateau/plateau-dual-api-service/structure/plateau-dual-api-service--package-api-grpc.skill.md]] | Inbound gRPC adapter |

# Rules

MUST:
- `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` are the only testing-related `Makefile` targets; `build`/`run`/`lint`/`proto-gen` are the only lifecycle targets. No solution redefines an existing target — each adds its own.
- `report-template/index.html` is a static asset, copied verbatim by the shared `tools/testing/test-report.sh` into `$TEST_REPORT_DIR/index.html` — never generated.
- `make test-kind-unit` writes `$TEST_KIND_DIR/result/scenarios.json` on every run, green or red; every scenario (or `Examples:` block) carries exactly one type tag.
- `make test-kind-unit` writes godog's classic Cucumber JSON to `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` via `tools/livingdoc/` (skipped, never failed, without `npm`).
- `tools/testing/` (copied verbatim from `solution-conformance-testing`) defines the caller-facing targets — `make test-kinds`, `test-kind-{kind}`, `test-report`, `test-readme-check`, `test-and-report`; `README.md` carries one badge per declared badge.
- `docs/integration/api/linkcheck/linkcheck.proto` lies directly in its contract folder (no version folder): `paths=source_relative` mirrors its path into `gen/api/linkcheck/`, which is what `go_package` declares. `buf.gen.yaml` starts every plugin as `local: ["go", "tool", …]` — the generators are `tool` lines of `go.mod`, nothing is installed with `@latest`.
- `make test-kinds` lists `generated - no badge`; `tools/testing/kinds/generated.sh` is an unchanged copy of the file of `api-develop-in-go`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[library/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[library/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] `make proto-gen` regenerates `gen/api/linkcheck/`, `docs/integration/api/linkcheck/linkcheck.swagger.json` with no manual edits needed afterward; `make test-kind-generated` passes (verified against this plateau's own `examples/`).
