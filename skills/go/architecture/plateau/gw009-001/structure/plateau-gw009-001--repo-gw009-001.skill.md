---
name: plateau-gw009-001--repo-gw009-001
description: Repository-root layout of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-gw009-001
version: 20261012000000
tags:
  - skill/template/repo
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
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
buf.yaml                         ← the buf modules: docs/integration/api and docs/integration/client
buf.lock
buf.gen.yaml                     ← own contract → gen/api and the spec beside the proto file
buf.gen.client.yaml              ← consumed contracts → gen/client (go_package by managed mode)
docs/
  integration/
    api/
      linkcheck/                 ← package tier, see plateau-gw009-001--package-docs-integration-api.skill.md
    client/
      reputation/                ← package tier, see plateau-gw009-001--package-docs-integration-client.skill.md
gen/
  api/
    linkcheck/                   (generated; committed, not gitignored)
  client/
    reputation/                  (generated; committed, not gitignored)
cmd/
  {service}/
    main.go                       ← file tier, see plateau-gw009-001--file-cmd-service-main.skill.md
  migrate/                        ← package tier, see plateau-gw009-001--package-cmd-migrate.skill.md (Job mode)
internal/
  config/
    config.go                     ← file tier
  version/
    version.go                    ← file tier
  logging/
    logger.go                     ← file tier
  domain/
    interfaces/                   ← package tier, see plateau-gw009-001--package-domain-interfaces.skill.md
    services/                     ← package tier, see plateau-gw009-001--package-domain-services.skill.md
  api/
    http/                         ← package tier, see plateau-gw009-001--package-api-http.skill.md
    grpc/                         ← package tier, see plateau-gw009-001--package-api-grpc.skill.md
    tasks/                        ← package tier, see plateau-gw009-001--package-api-tasks.skill.md
  taskbox/                        ← pre-release copy of the taskbox-go library (+ pgstore/, features/, test/); no structure skill — replaced by the module dependency at taskbox-go v0.1.0
  infrastructure/
    reputationclient/             ← package tier, see plateau-gw009-001--package-infrastructure-reputationclient.skill.md
    reputationcache/              ← package tier, see plateau-gw009-001--package-infrastructure-reputationcache.skill.md
    linkstore/                    ← package tier, see plateau-gw009-001--package-infrastructure-linkstore.skill.md (+ migrations/)
tools/
  testing/kinds/generated.sh      ← the generated test kind, copied unchanged from api-develop-in-go
  normalize_unittest/main.go
  normalize_scenarios/main.go
  normalize_mutation/main.go
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]]
- [[skills/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md|Repository]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md|Repository]]

`solution-cached-db` and `solution-persistent-db` add no `Repository` content of their own — `internal/infrastructure/reputationcache` and `internal/infrastructure/linkstore` are ordinary new packages under the already-established `internal/infrastructure/` tree, listed in the table below via their own `Package.create.md` files, not via a `Repository.extend.md`.

- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

`solution-go-db-migrations` adds `cmd/migrate` and `linkstore/migrations/` inside the established layout, with no `Repository` file of its own.

## Directory and package skills
| Directory \| file | template link | Description |
| ----------------- | -------------- | ------------ |
| internal/domain/interfaces | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-domain-interfaces.skill.md]] | Outbound ports |
| internal/domain/services | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-domain-services.skill.md]] | Business logic |
| internal/api/http | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-http.skill.md]] | Inbound HTTP adapter |
| docs/integration/api/linkcheck | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-docs-integration-api.skill.md]] | The service's own API contract and its embed package |
| docs/integration/client/reputation | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-docs-integration-client.skill.md]] | The consumed reputation contract |
| internal/api/grpc | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-grpc.skill.md]] | Inbound gRPC adapter |
| internal/infrastructure/reputationclient | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-infrastructure-reputationclient.skill.md]] | Outbound gRPC-client adapter |
| internal/infrastructure/reputationcache | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-infrastructure-reputationcache.skill.md]] | Outbound Redis-backed cache adapter |
| internal/infrastructure/linkstore | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-infrastructure-linkstore.skill.md]] | Outbound PostgreSQL-backed history adapter |
| cmd/migrate | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-cmd-migrate.skill.md]] | One-shot migration job (Job mode) |
| internal/api/tasks | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-tasks.skill.md]] | Inbound TaskBox handler adapter |

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
- A new `internal/infrastructure/*` adapter package (like `linkstore`) never requires a `Repository.extend.md` of its own — it is ordinary content inside the `internal/infrastructure/` tree `solution-go-repository-structure` already established.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md#MUST|Repository]]
- `make test-kind-unit` needs `TEST_DATABASE_DSN` (a throwaway PostgreSQL): the TaskBox conformance runner fails without it, never skips.
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] `make proto-gen` regenerates `gen/api/linkcheck/`, `docs/integration/api/linkcheck/linkcheck.swagger.json`, and `gen/client/reputation/` with no manual edits needed afterward, including the `RecentChecks` RPC; `make test-kind-generated` passes (verified against this plateau's own `examples/`).
- [ ] `TEST_DATABASE_DSN=postgres://postgres:postgres@localhost:5432/taskbox_test make test-kind-unit` passes (48 tests; 30 TaskBox scenarios on PostgreSQL).
