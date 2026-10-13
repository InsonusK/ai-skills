---
name: plateau-integrated-service--repo-integrated-service
description: Repository-root layout of the plateau-integrated-service plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-integrated-service
version: 20261012000000
tags:
  - skill/template/repo
  - plateau/plateau-integrated-service
created_by:
  - "[[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
  - "[[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[library/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
  - "[[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Structure

## Repository Structure
```
go.mod
Makefile
.gitignore
report-template/
  index.html
buf.yaml                         ← the buf modules: docs/integration/api and docs/integration/client
buf.lock
buf.gen.yaml                     ← own contract → gen/api and the spec beside the proto file
buf.gen.client.yaml              ← consumed contracts → gen/client (go_package by managed mode)
docs/
  integration/
    api/
      linkcheck/                 ← package tier, see plateau-integrated-service--package-docs-integration-api.skill.md
    client/
      reputation/                ← package tier, see plateau-integrated-service--package-docs-integration-client.skill.md
gen/
  api/
    linkcheck/                   (generated; committed, not gitignored)
  client/
    reputation/                  (generated; committed, not gitignored)
cmd/
  {service}/
    main.go                       ← file tier, see plateau-integrated-service--file-cmd-{service}-main.skill.md
internal/
  config/
    config.go                     ← file tier
  version/
    version.go                    ← file tier
  logging/
    logger.go                     ← file tier
  domain/
    interfaces/                   ← package tier, see plateau-integrated-service--package-domain-interfaces.skill.md
    services/                     ← package tier, see plateau-integrated-service--package-domain-services.skill.md
  api/
    http/                         ← package tier, see plateau-integrated-service--package-api-http.skill.md
    grpc/                         ← package tier, see plateau-integrated-service--package-api-grpc.skill.md
  infrastructure/
    reputationclient/             ← package tier, see plateau-integrated-service--package-infrastructure-reputationclient.skill.md
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
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md|Repository]]

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/interfaces | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-domain-interfaces.skill.md]] | Outbound ports |
| internal/domain/services | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-api-http.skill.md]] | Inbound HTTP adapter |
| docs/integration/api/linkcheck | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-docs-integration-api.skill.md]] | The service's own API contract and its embed package |
| docs/integration/client/reputation | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-docs-integration-client.skill.md]] | The consumed reputation contract |
| internal/api/grpc | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-api-grpc.skill.md]] | Inbound gRPC adapter |
| internal/infrastructure/reputationclient | [[library/go/architecture/plateau/plateau-integrated-service/structure/plateau-integrated-service--package-infrastructure-reputationclient.skill.md]] | Outbound gRPC-client adapter |

# Rules

MUST:
- `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` are the only testing-related `Makefile` targets; `build`/`run`/`lint`/`proto-gen` are the only lifecycle targets. No solution redefines an existing target — each adds its own.
- `report-template/index.html` is a static asset, copied verbatim by the shared `tools/testing/test-report.sh` into `$TEST_REPORT_DIR/index.html` — never generated.
- `make test-kind-unit` writes `$TEST_KIND_DIR/result/scenarios.json` on every run, green or red; every scenario (or `Examples:` block) carries exactly one type tag.
- `make test-kind-unit` writes godog's classic Cucumber JSON to `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` via `tools/livingdoc/` (skipped, never failed, without `npm`).
- `tools/testing/` (copied verbatim from `solution-conformance-testing`) defines the caller-facing targets — `make test-kinds`, `test-kind-{kind}`, `test-report`, `test-readme-check`, `test-and-report`; `README.md` carries one badge per declared badge.
- `docs/integration/api/linkcheck/linkcheck.proto` lies directly in its contract folder (no version folder): `paths=source_relative` mirrors its path into `gen/api/linkcheck/`, which is what `go_package` declares. `buf.gen.yaml` starts every plugin as `local: ["go", "tool", …]` — the generators are `tool` lines of `go.mod`, nothing is installed with `@latest`.
- `make test-kinds` lists `generated - no badge`; `tools/testing/kinds/generated.sh` is an unchanged copy of the file of `api-develop-in-go`.
- `docs/integration/client/reputation/reputation.proto` (the external service's own contract, the provider's copy unchanged) is generated into its own `gen/client/reputation/` through its own `buf.gen.client.yaml` — never merged with `gen/api/`. `buf.gen.yaml` and `buf.gen.client.yaml` each name their module under `inputs`, and `proto-gen` runs both as two lines of one target.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[library/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[library/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] `make proto-gen` regenerates `gen/api/linkcheck/`, `docs/integration/api/linkcheck/linkcheck.swagger.json`, and `gen/client/reputation/` with no manual edits needed afterward; `make test-kind-generated` passes (verified against this plateau's own `examples/`).
