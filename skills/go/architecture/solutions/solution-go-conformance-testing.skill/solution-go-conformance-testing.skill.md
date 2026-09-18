---
name: solution-go-conformance-testing
description: Sets up the Go side of the Cucumber/coverage/mutation quality gate — godog for scenarios, go test -cover for coverage, gremlins for mutation testing, and the make unit-test/mutation-test/test-report/test-and-report contract
whenToUse: when setting up or reviewing the test tooling of a Go module that must prove conformance to solution-conformance-testing's gate, or wiring coverage and mutation testing into a Go project's Makefile/CI pipeline
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-conformance-testing
  - stack/go
  - concern/testing
  - concern/testing/bdd
  - concern/testing/mutation
  - cucumber
  - godog
  - concern/architecture
creates:
  - "tools/normalize_unittest/main.go"
  - "tools/normalize_mutation/main.go"
  - "tools/test_report/main.go"
extends:
  - "Makefile"
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
  - "[[skills/go/architecture/solutions/solution-go-conformance-testing.skill/adr/mutation-tool-choice.md|Mutation-testing tool for Go]]"
---

# Goal
- Give a Go module the concrete tooling to run the gate [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] defines: Cucumber (godog) scenarios, code coverage, mutation testing — behind the same `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract every stack in this repository exposes.
- Keep godog scenarios co-located with the package they test (per [[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]]'s own convention), never centralized in one repository-root `features/` tree.

# Capabilities
- `go test -json ./...` runs godog scenarios and plain Go tests in one invocation; a normalizer collapses godog's parent/subtest duplication so a scenario is counted once.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` scopes a mutation run to files changed since `<ref>`, so a pull request's gate does not pay for a full-module run.
- `make test-report` assembles a stack-independent `public/` site from the normalized results, matching the parent solution's report contract exactly — nothing downstream needs to know this is a Go module.

# Core Principles
- Every scenario is authored per [[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]] — this solution wires the `make`/report machinery around that authoring standard, it does not restate it.
- `go test -coverpkg` excludes `gen/` (generated protobuf/gRPC code) and `tools/` (this solution's own reporting tools) — neither has product logic to cover.
- `mutation-test` always exits with the underlying `gremlins` exit code after writing its normalized result, per the parent solution's contract.

# Adr
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/adr/mutation-tool-choice.md|Mutation-testing tool for Go]]
  - Selected variant: `gremlins` (`github.com/go-gremlins/gremlins`)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for Go.
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]
  - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|Repository]] - the `Makefile` this solution extends with the testing targets
GO MODULES / STANDARD LIBRARY:
- `github.com/cucumber/godog` — runs `.feature` files against step definitions; see [[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]] for authoring rules.
- `github.com/go-gremlins/gremlins` (installed as a CLI, not imported) — mutation testing.
- `go test`'s own `-json`/`-coverprofile`/`-coverpkg` flags (standard toolchain) — no third-party coverage library.

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `Makefile`'s `unit-test`/`mutation-test`/`test-report`/`test-and-report` targets, `report-template/index.html`
- [[./Implementation/tools/normalize_unittest/main.go.create.md|tools/normalize_unittest/main.go]] - create - collapses `go test -json` output to leaf-level results, writes `tmp/result/unit-test.json`
- [[./Implementation/tools/normalize_mutation/main.go.create.md|tools/normalize_mutation/main.go]] - create - normalizes `gremlins`' report, writes `tmp/result/mutation-test.json`
- [[./Implementation/tools/test_report/main.go.create.md|tools/test_report/main.go]] - create - assembles `public/` from `tmp/result/*.json`

# Workflow

## Add conformance coverage for a new rule (happy path)
1. A `.feature` file is added next to the package it exercises (e.g. `internal/domain/services/features/{rule}.feature`), per [[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]].
2. Step definitions in that package's `test/` folder bind the scenario to the package's real exported API.
3. `make unit-test` runs `go test -json -coverpkg=$(COVERPKG) -coverprofile=... ./...`, piping JSON events through `tools/normalize_unittest` into `tmp/result/unit-test.json` (plus `tmp/result/coverage-test.json` when `WITH_CODE_COVERAGE=true`).
4. `make mutation-test` runs `gremlins unleash` (scoped to changed files with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across the whole module otherwise), normalizing its report into `tmp/result/mutation-test.json` via `tools/normalize_mutation`.
5. `make test-report` runs `tools/test_report`, assembling `public/` from `tmp/result/*.json`. `make test-and-report` chains all three.

## Surviving mutant found (report path)
1. `make mutation-test` reports a mutant `gremlins` could not kill, as part of a report-only CI run.
2. Whoever notices it (via the published report or a coverage/mutation badge) strengthens the corresponding scenario's assertion in a follow-up change, or explicitly accepts it per the parent solution's own rule.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/tools/normalize_unittest/main.go.create.md#MUST|tools/normalize_unittest/main.go]]
- [[./Implementation/tools/normalize_mutation/main.go.create.md#MUST|tools/normalize_mutation/main.go]]
- [[./Implementation/tools/test_report/main.go.create.md#MUST|tools/test_report/main.go]]

# Check list
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` all exist and match [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]].
- [ ] `COVERPKG` excludes `gen/` and `tools/`.
- [ ] Every `.feature` file's scenarios follow [[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]]'s check list.
