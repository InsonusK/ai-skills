---
name: solution-conformance-testing-in-dotnet
description: The .NET implementation of [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] — Reqnroll for Gherkin scenarios, coverlet + ReportGenerator for coverage, Stryker.NET for mutation testing, and the make test-kind-unit/test-kind-mutation/test-report/test-and-report contract aggregated across every test project of a .NET solution
whenToUse: Set up or review the test tooling of a .NET solution that must prove conformance to a Cucumber/Gherkin spec, or wire coverage, mutation testing, and the scenario report into a .NET solution's `make`/CI pipeline.
domain: skill
type: architecture
version: 20261007000000
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
  - stryker-config.json
  - report-template/index.html
extends:
  - README.md
depends_on:
  - "[[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
  - "[[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/testing-tool-choice|Testing tool choice]]"
  - "[[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|xUnit v2 on VSTest until Stryker.NET supports xunit.v3]]"
---

# Goal
- Give a .NET solution the concrete tooling to run the gate [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] defines: Reqnroll scenarios, code coverage, mutation testing, and the scenario report — behind the same `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract every stack in this repository exposes.

# Capabilities
- `make test-kind-unit` runs every test project of the solution in one `dotnet test` invocation and reports one aggregated result — plain unit tests and Reqnroll scenarios together.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-solution mutation run on every call.
- `make test-report` assembles a stack-independent `$TEST_REPORT_DIR/` site — badges, native reports, and the scenario report — from the normalized results only.

# Core Principles
- Every scenario is authored per [[skills/testing/dotnet/cucumber-testing-in-dotnet.skill.md|cucumber-testing-in-dotnet]] — this solution wires the `make`/report machinery around that authoring standard, it does not restate it.
- How the solution splits into test projects is not decided here; a catalog's own architecture solution decides it (e.g. `solution-dotnet-conformance-testing` for the dotnet plateau catalog). This solution only requires that every test project is part of the solution `dotnet test` runs.
- `test-kind-mutation` always exits with Stryker.NET's own exit code after writing its normalized result, per the parent solution's contract.
- Test projects run xUnit v2 on the VSTest runner, never xunit.v3 on Microsoft.Testing.Platform: Stryker.NET 4.16 reports a false 0% score there. Re-check with `recheck/stryker-xunit-v3.sh` before any move — see [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|xUnit v2 on VSTest until Stryker.NET supports xunit.v3]].

# Adr
- [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: Reqnroll (Gherkin runner) + coverlet/ReportGenerator (coverage) + Stryker.NET (mutation testing)
- [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|xUnit v2 on VSTest until Stryker.NET supports xunit.v3]]
  - Selected variant: xUnit v2 + Reqnroll.xUnit on VSTest until `recheck/stryker-xunit-v3.sh` reports `SUPPORTED`

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` target names, the `$TEST_KIND_DIR/result/*.json` schema, and the `$TEST_REPORT_DIR/` layout this solution implements for .NET.

NUGET:
- xunit (2.9.x), xunit.runner.visualstudio (3.x), Reqnroll.xUnit, Microsoft.NET.Test.Sdk
  - Execute `.feature` files and plain tests on the VSTest runner; Reqnroll's `message` formatter feeds the scenario report. Not xunit.v3 — see [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|xUnit v2 on VSTest until Stryker.NET supports xunit.v3]].
- coverlet.collector
  - Collects line/branch coverage during `dotnet test`.
- ReportGenerator (dotnet tool)
  - Converts coverlet's Cobertura output into an HTML report and a coverage badge.
- Stryker.NET (`dotnet-stryker` tool)
  - Runs mutation testing against the solution and produces a mutation score report.

# Template Skill Mutations
REPOSITORY:
- [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile`, the normalization scripts, one `reqnroll.json` per test project, and `report-template/index.html`

# Workflow
## Run the gate
1. `make test-kind-unit` runs `dotnet test` across every test project, executing both the plain unit tests and the Reqnroll scenarios, and normalizes the aggregated result into `$TEST_KIND_DIR/result/unit-test.json`, `$TEST_KIND_DIR/result/scenarios.json` (plus `$TEST_KIND_DIR/result/coverage-test.json` in a `report` run). It also copies every project's Cucumber Messages file to `report/tests/cucumber/` and renders `report/tests/livingdoc/` with the shared `tools/livingdoc/`.
2. `make test-kind-mutation` runs `dotnet-stryker` — across every project in a `report` run; in a `check` run scoped to code changed since `DELTA_BASE`, and skipped without one — and normalizes the result into `$TEST_KIND_DIR/result/mutation-test.json`.
3. `make test-report` builds `$TEST_REPORT_DIR/` from every kind's `result/*.json` and `report/*/`, ready to publish. `make test-and-report` runs every kind, then the report.
4. Which target runs on which trigger, and how `$TEST_REPORT_DIR/` gets published, is owned by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. Whoever notices the survivor (via the published report or the mutation-score badge) either strengthens the assertion in the corresponding scenario in a follow-up change, or explicitly accepts it per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must|solution-conformance-testing]].

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend#MUST|Repository]]

# Check list
- [ ] `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, and `make test-and-report` exist at the repository root, run across every test project, and support the toggles defined by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].
- [ ] `$TEST_KIND_DIR/result/*.json` — including `scenarios.json` — and `$TEST_KIND_DIR/report/<kind>/` aggregate every test project present and follow that same contract's schema.
- [ ] No test project references `xunit.v3`/`Reqnroll.xunit.v3`, and no `global.json` opts `dotnet test` into Microsoft.Testing.Platform.
- [ ] `stryker-config.json` sets `test-case-filter` to `Category!=todo`.
- [ ] `$TEST_REPORT_DIR/` follows [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-output|solution-conformance-testing's Public site output]] contract, and `report-template/index.html` exists at the repository root (not under `.github/`).
