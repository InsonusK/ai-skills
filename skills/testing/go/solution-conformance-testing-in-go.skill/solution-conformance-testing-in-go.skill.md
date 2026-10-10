---
name: solution-conformance-testing-in-go
description: Sets up the Go side of the Cucumber/coverage/mutation quality gate — godog for scenarios, go test -cover for coverage, gremlins for mutation testing, the scenario inventory, and the unit and mutation test kinds behind the shared make test-kind-{kind} / test-report contract
whenToUse: when setting up or reviewing the test tooling of a Go module that must prove conformance to solution-conformance-testing's gate, or wiring coverage, mutation testing, and the scenario inventory into a Go project's Makefile/CI pipeline
domain: skill
type: architecture
version: 20261010120000
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
  - "[[./adr/distribution-version.md|Distribution version]]"
  - "[[skills/testing/go/solution-conformance-testing-in-go.skill/adr/mutation-tool-choice.md|Mutation-testing tool for Go]]"
---

# Goal
- Give a Go module the concrete tooling to run the gate [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] defines: Cucumber (godog) scenarios, code coverage, mutation testing — behind the same `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract every stack in this repository exposes.
- Keep godog scenarios co-located with the package they test (per [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]]'s own convention), never centralized in one repository-root `features/` tree.

# Capabilities
- `go test -json ./...` runs godog scenarios and plain Go tests in one invocation; a normalizer collapses godog's parent/subtest duplication so a scenario is counted once.
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type and category, its status, and `@status/todo` reason — for the tag check and the living doc; no separate scenarios page is generated.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` scopes a mutation run to files changed since `<ref>`, so a pull request's gate does not pay for a full-module run.
- `make test-report` gathers the kinds' reports and badges into a stack-independent `$TEST_REPORT_DIR/` site, matching the parent solution's report contract exactly — nothing downstream needs to know this is a Go module.

# Core Principles
- Every scenario is authored per [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]] — this solution wires the `make`/report machinery around that authoring standard, it does not restate it.
- `go test -coverpkg` excludes `gen/` (generated protobuf/gRPC code) and `tools/` (this solution's own reporting tools) — neither has product logic to cover.
- `test-kind-mutation` always exits with the underlying `gremlins` exit code after writing its normalized result, per the parent solution's contract.

# Adr
- [[./adr/distribution-version.md|Distribution version]] — compare the package's public version with the version recorded in `internal/version/version.go`; local Go build metadata does not carry a release version.
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
[`examples/`](./examples/) is a runnable linkcheck module: eight co-located features cover checking, extraction, batch summaries, CLI, file storage, mapping, the result contract, and distribution metadata. The scenarios and tags match the Python reference, including two excluded `@status/todo` scenarios, one excluded `@status/broken`, all seven type and category values, and the unchanged store scenario marked `@status/validated` for owner confirmation. The store uses a mutex and the concurrency scenario uses a start barrier and waits for both writers; it has no timing assertion. Verified with Go 1.26, godog 0.16, gremlins 0.6.0:
- `make init && make test-and-report` — exit `0`; 25/25 scenarios, coverage 94.7%, mutation score 100% (18 killed, no survivors/timeouts/uncovered mutants).
- `run-example.sh` — report/check runs, caller-selected directories, inventory/tag/reason/legend assertions, and all five report pages pass with no broken links. A deliberate bad step and removed category each fail the unit kind, which still writes its inventory; the failed report records `unit` as `failed`.
- `make test-and-report TEST_RUN_PURPOSE=check` — mutation skipped without a delta base, coverage gathered but not published.
- Real delta mutation in an isolated two-commit repository changes the scheme predicate: 2 mutants killed, 16 skipped, score 100%; only the changed checker condition is mutated. `go test -race -count=5` passes both scenario packages.
- The `tests` link opens the living doc; every scenario appears with all tags and excluded reasons, plus the status legend. `result/scenarios.json` remains the tag-check and rendering input; there is no scenarios report directory.
- Mutation keeps `--integration`, `GOFLAGS=-count=1`, and `--timeout-coefficient 10`: tests run in sibling `test/` packages, so package-only mutation runs would measure little.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/tools/normalize_unittest/main.go.create.md#MUST|tools/normalize_unittest/main.go]]
- [[./Implementation/tools/normalize_scenarios/main.go.create.md#MUST|tools/normalize_scenarios/main.go]]
- [[./Implementation/tools/normalize_mutation/main.go.create.md#MUST|tools/normalize_mutation/main.go]]

# Check list
- [ ] `make test-kinds` lists `unit` and `mutation`; `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, `make test-readme-check`, and `make test-and-report` all exist and match [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]].
- [ ] `COVERPKG` excludes `gen/` and `tools/`.
- [ ] `$TEST_KIND_DIR/result/scenarios.json` is written on every `make test-kind-unit` run and the living doc includes every inventory entry with its tags, excluded reason, and status legend.
- [ ] Every `.feature` file's scenarios follow [[skills/testing/go/cucumber-testing-in-go.skill.md|cucumber-testing-in-go]]'s check list.
- [ ] `make test-kind-unit` writes godog's classic Cucumber JSON to `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#living-doc-report|the parent solution's living-doc report]].
