---
name: solution-conformance-testing-in-dotnet
description: The .NET implementation of [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] — Reqnroll for Gherkin scenarios, coverlet + ReportGenerator for coverage, Stryker.NET for mutation testing, and the make unit-test/mutation-test/test-report/test-and-report contract aggregated across every test project of a .NET solution
whenToUse: Set up or review the test tooling of a .NET solution that must prove conformance to a Cucumber/Gherkin spec, or wire coverage, mutation testing, and the scenario report into a .NET solution's `make`/CI pipeline.
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-dotnet
  - stack/dotnet
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
creates:
  - Makefile
  - scripts/unit-test.sh
  - scripts/normalize-scenarios.sh
  - scripts/messages-results.jq
  - scripts/mutation-test.sh
  - scripts/test-report.sh
  - "{TestProject}/reqnroll.json"
  - report-template/index.html
extends:
  - README.md
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a .NET solution the concrete tooling to run the gate [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] defines: Reqnroll scenarios, code coverage, mutation testing, and the scenario report — behind the same `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract every stack in this repository exposes.

# Capabilities
- `make unit-test` runs every test project of the solution in one `dotnet test` invocation and reports one aggregated result — plain unit tests and Reqnroll scenarios together.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-solution mutation run on every call.
- `make test-report` assembles a stack-independent `public/` site — badges, native reports, and the scenario report — from the normalized results only.

# Core Principles
- Every scenario is authored per [[skills/dotnet/testing/cucmber-testing-in-dotnet.skill.md|cucmber-testing-in-dotnet]] — this solution wires the `make`/report machinery around that authoring standard, it does not restate it.
- How the solution splits into test projects is not decided here; a catalog's own architecture solution decides it (e.g. [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] for the dotnet plateau catalog). This solution only requires that every test project is part of the solution `dotnet test` runs.
- `mutation-test` always exits with Stryker.NET's own exit code after writing its normalized result, per the parent solution's contract.

# Adr
- [[skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: Reqnroll (Gherkin runner) + coverlet/ReportGenerator (coverage) + Stryker.NET (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` target names, the `tmp/result/*.json` schema, and the `public/` layout this solution implements for .NET.

NUGET:
- Reqnroll.xUnit
  - Executes `.feature` files against step definitions using xUnit as the runner; its `message` formatter feeds the scenario report.
- coverlet.collector
  - Collects line/branch coverage during `dotnet test`.
- ReportGenerator (dotnet tool)
  - Converts coverlet's Cobertura output into an HTML report and a coverage badge.
- Stryker.NET (`dotnet-stryker` tool)
  - Runs mutation testing against the solution and produces a mutation score report.

# Template Skill Mutations
REPOSITORY:
- [[skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile`, the normalization scripts, one `reqnroll.json` per test project, and `report-template/index.html`

# Workflow
## Run the gate
1. `make unit-test` runs `dotnet test` across every test project, executing both the plain unit tests and the Reqnroll scenarios, and normalizes the aggregated result into `tmp/result/unit-test.json`, `tmp/result/scenarios.json` (plus `tmp/result/coverage-test.json` when `WITH_CODE_COVERAGE=true`).
2. `make mutation-test` runs `dotnet-stryker` — scoped to changed files when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across every project otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
3. `make test-report` assembles `public/` from `tmp/result/*.json` and `tmp/report/*`, ready to publish. `make test-and-report` runs all three in sequence.
4. Which target runs on which trigger, and how `public/` gets published, is owned by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make mutation-test` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. Whoever notices the survivor (via the published report or the mutation-score badge) either strengthens the assertion in the corresponding scenario in a follow-up change, or explicitly accepts it per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must|solution-conformance-testing]].

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend#MUST|Repository]]

# Check list
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` exist at the repository root, run across every test project, and support the toggles defined by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].
- [ ] `tmp/result/*.json` — including `scenarios.json` — and `tmp/report/<kind>/` aggregate every test project present and follow that same contract's schema.
- [ ] `public/` follows [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#public-site-output|solution-conformance-testing's Public site output]] contract, and `report-template/index.html` exists at the repository root (not under `.github/`).
