---
name: solution-conformance-testing-in-go
description: Sets up the Go side of the Cucumber/coverage/mutation quality gate — godog for scenarios, go test -cover for coverage, gremlins for mutation testing, the scenario report, and the unit and mutation test kinds behind the shared make test-kind-{kind} / test-report contract
whenToUse: when setting up or reviewing the test tooling of a Go module that must prove conformance to solution-conformance-testing's gate, or wiring coverage, mutation testing, and the scenario report into a Go project's Makefile/CI pipeline
domain: skill
type: architecture
version: 20261008200000
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-go
  - stack/go
  - concern/testing
  - concern/testing/bdd
  - concern/testing/mutation
  - cucumber
  - godog
creates:
  - "tools/normalize_unittest/main.go"
  - "tools/normalize_scenarios/main.go"
  - "tools/normalize_mutation/main.go"
  - "tools/testing/kinds/unit.sh"
  - "tools/testing/kinds/mutation.sh"
  - "README.md"
extends:
  - "Makefile"
  - "go.mod"
depends_on:
  - "[[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
  - "[[skills/testing/go/solution-conformance-testing-in-go.skill/adr/mutation-tool-choice.md|Mutation-testing tool for Go]]"
---

# Goal
- Give a Go module the concrete tooling to run the gate [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] defines: Cucumber (godog) scenarios, code coverage, mutation testing — behind the same `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract every stack in this repository exposes.
- Keep godog scenarios co-located with the package they test (per [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]]'s own convention), never centralized in one repository-root `features/` tree.

# Capabilities
- `go test -json ./...` runs godog scenarios and plain Go tests in one invocation; a normalizer collapses godog's parent/subtest duplication so a scenario is counted once.
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type tag, status, and `@todo` reason — and `make test-report` renders it as `$TEST_REPORT_DIR/reports/scenarios/`.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` scopes a mutation run to files changed since `<ref>`, so a pull request's gate does not pay for a full-module run.
- `make test-report` gathers the kinds' reports and badges into a stack-independent `$TEST_REPORT_DIR/` site, matching the parent solution's report contract exactly — nothing downstream needs to know this is a Go module.

# Core Principles
- Every scenario is authored per [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]] — this solution wires the `make`/report machinery around that authoring standard, it does not restate it.
- `go test -coverpkg` excludes `gen/` (generated protobuf/gRPC code) and `tools/` (this solution's own reporting tools) — neither has product logic to cover.
- `test-kind-mutation` always exits with the underlying `gremlins` exit code after writing its normalized result, per the parent solution's contract.

# Adr
- [[skills/testing/go/solution-conformance-testing-in-go.skill/adr/mutation-tool-choice.md|Mutation-testing tool for Go]]
  - Selected variant: `gremlins` (`github.com/go-gremlins/gremlins`)

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for Go.
GO MODULES / STANDARD LIBRARY:
- `github.com/cucumber/godog` — runs `.feature` files against step definitions; see [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]] for authoring rules.
- `github.com/cucumber/gherkin/go/v42` + `github.com/cucumber/messages/go/v34` — the `.feature` parser godog itself uses; `tools/normalize_scenarios` imports it to build the scenario inventory.
- `github.com/go-gremlins/gremlins` (installed as a CLI, not imported) — mutation testing.
- `go test`'s own `-json`/`-coverprofile`/`-coverpkg` flags (standard toolchain) — no third-party coverage library.

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `Makefile`'s `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` targets, `report-template/index.html`, gherkin parser in `go.mod`
- [[./Implementation/tools/normalize_unittest/main.go.create.md|tools/normalize_unittest/main.go]] - create - collapses `go test -json` output to leaf-level results, writes `$TEST_KIND_DIR/result/unit-test.json`
- [[./Implementation/tools/normalize_scenarios/main.go.create.md|tools/normalize_scenarios/main.go]] - create - parses every `.feature` file, joins `go test -json` results, writes `$TEST_KIND_DIR/result/scenarios.json`
- [[./Implementation/tools/normalize_mutation/main.go.create.md|tools/normalize_mutation/main.go]] - create - normalizes `gremlins`' report, writes `$TEST_KIND_DIR/result/mutation-test.json`

# Workflow

## Add conformance coverage for a new rule (happy path)
1. A `.feature` file is added next to the package it exercises (e.g. `internal/domain/services/features/{rule}.feature`), per [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]].
2. Step definitions in that package's `test/` folder bind the scenario to the package's real exported API.
3. `make test-kind-unit` runs `go test -json -coverpkg=$(COVERPKG) -coverprofile=... ./...`, piping JSON events through `tools/normalize_unittest` into `$TEST_KIND_DIR/result/unit-test.json`, then runs `tools/normalize_scenarios` to write `$TEST_KIND_DIR/result/scenarios.json` — also when a test failed (plus `$TEST_KIND_DIR/result/coverage-test.json` in a `report` run).
4. `make test-kind-mutation` runs `gremlins unleash` (across the whole module in a `report` run; in a `check` run scoped to files changed since `DELTA_BASE`, and skipped without one), normalizing its report into `$TEST_KIND_DIR/result/mutation-test.json` via `tools/normalize_mutation`.
5. `make test-report` runs the shared `tools/testing/test-report.sh`, gathering every kind's `report/` and `badges/` into `$TEST_REPORT_DIR/`. `make test-and-report` runs every kind, then the report.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant `gremlins` could not kill, as part of a report-only CI run.
2. Whoever notices it (via the published report or a coverage/mutation badge) strengthens the corresponding scenario's assertion in a follow-up change, or explicitly accepts it per the parent solution's own rule.

# Ground truth
[`example/`](./example/) is a minimal module (`internal/linkcheck/` with its `features/` — a `Scenario Outline` of two `Examples:` blocks and a `@todo` scenario — and its `test/` runner) carrying this solution as it is delivered. Verified on 2026-10-08 with Go 1.26, `godog` 0.16, `gremlins` 0.6.0:
- `make init`, then `make test-and-report` — exit `0`; 4/4 scenarios, coverage 90.0%, mutation score 100% (7 killed); the scenario report lists both `Examples:` blocks and the `@todo` entry with its reason, and the living doc shows each row with its type tag.
- `make test-and-report TEST_RUN_PURPOSE=check` — mutation skipped, no coverage report, only the `tests` badge.
- `make test-kind-mutation` without `--integration` — 4 of the 7 mutants live and the score is 42.9%: the measurement behind the `--integration` rule.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/tools/normalize_unittest/main.go.create.md#MUST|tools/normalize_unittest/main.go]]
- [[./Implementation/tools/normalize_scenarios/main.go.create.md#MUST|tools/normalize_scenarios/main.go]]
- [[./Implementation/tools/normalize_mutation/main.go.create.md#MUST|tools/normalize_mutation/main.go]]

# Check list
- [ ] `make test-kinds` lists `unit` and `mutation`; `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, `make test-readme-check`, and `make test-and-report` all exist and match [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]].
- [ ] `COVERPKG` excludes `gen/` and `tools/`.
- [ ] `$TEST_KIND_DIR/result/scenarios.json` is written on every `make test-kind-unit` run and `$TEST_REPORT_DIR/reports/scenarios/index.html` is rendered from it.
- [ ] Every `.feature` file's scenarios follow [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]]'s check list.
- [ ] `make test-kind-unit` writes godog's classic Cucumber JSON to `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#living-doc-report|the parent solution's living-doc report]].
