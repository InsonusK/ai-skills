---
name: plateau-gw009-001--repo-gw009-001
description: Repository-root layout of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when adding, removing, or relocating a top-level package under this plateau, or deciding where new repository-root content (build tooling, config files) belongs
domain: skill
type: template
plateau: plateau-gw009-001
version: 20261006000000
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
proto/
  linkcheck/
    linkcheck.proto                ← this module's own exposed gRPC contract (extended: RecentChecks RPC)
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
- `proto/linkcheck/linkcheck.proto` stays flat (no version subdirectory) — its path must match `go_package`'s flat `gen/api` exactly, or `buf generate` emits code at a different Go import path than every hand-written file expects (verified — this is the actual, observed failure mode, not a hypothetical). `buf/buf.gen.yaml` uses `local:` plugins (`protoc-gen-go`, `protoc-gen-go-grpc`, both `go install`ed), not `buf.build` remote plugins.
- `proto/reputation/reputation.proto` (the external service's own contract) is generated into its own `gen/reputation` via its own `buf/reputation.gen.yaml` — never merged with `gen/api`. Its `buf generate` call was added as a second line inside the existing `proto-gen` target, not a second target declaration.
- A new `internal/infrastructure/*` adapter package (like `linkstore`) never requires a `Repository.extend.md` of its own — it is ordinary content inside the `internal/infrastructure/` tree `solution-go-repository-structure` already established.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] - [[skills/testing/go/solution-conformance-testing-in-go.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md#MUST|Repository]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md#MUST|Repository]]
- `make test-kind-unit` needs `TEST_DATABASE_DSN` (a throwaway PostgreSQL): the TaskBox conformance runner fails without it, never skips.
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] `make proto-gen` regenerates both `gen/api` and `gen/reputation` with no manual edits needed afterward, including the new `RecentChecks` RPC (verified against this plateau's own `examples/`).
- [ ] `TEST_DATABASE_DSN=postgres://postgres:postgres@localhost:5432/taskbox_test make test-kind-unit` passes (48 tests; 30 TaskBox scenarios on PostgreSQL).
