---
name: dotnet-solution-conformance-testing
description: Sets up the .NET side of the Cucumber/coverage/mutation quality gate — Reqnroll for Gherkin scenarios, coverlet + ReportGenerator for coverage, Stryker.NET for mutation testing, and the make cucumber-test/mutation-test/result-page contract that devops-github-wf-bdd-report-publish's workflows consume
whenToUse: Set up or review the test suite of a .NET library/project that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing .NET project, or wire coverage and mutation testing into a .NET project's `make`/CI pipeline.
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - stack/dotnet
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
  - concern/architecture

creates:
  - "{Module}.Tests.csproj"
  - "{Module}.Tests.StepDefinitions.{Rule}Steps.cs"
  - Makefile
extends:
  - README.md
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]"
adr:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a .NET project the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Expose that tooling behind the `make cucumber-test`/`make mutation-test`/`make result-page` contract so [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) can wire CI without knowing anything .NET-specific.

# Capabilities
- Gherkin `.feature` files execute against the project's real production code via Reqnroll step definitions.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-project mutation run on every call.
- `make cucumber-test WITH_CODE_COVERAGE=true` and `make result-page` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.

# Core Principles
- One `{Module}.Tests` project per module under test, containing unit tests, Reqnroll feature files, and their step definitions together.
- Step definitions call the module's real public API; they never re-implement the rule under test.
- Coverage and mutation testing both run against `{Module}.Tests`, not against a separate, parallel "spec-only" project.

# Adr
- [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: Reqnroll (Gherkin runner) + coverlet/ReportGenerator (coverage) + Stryker.NET (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for .NET.
- [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]
  - Owns the actual CI workflows (PR-gate and master-push) that call this solution's `Makefile`; this solution does not define any `.github/workflows/*.yml` file itself.

NUGET:
- Reqnroll.xUnit
  - Executes `.feature` files against step definitions using xUnit as the runner.
- coverlet.collector
  - Collects line/branch coverage during `dotnet test`.
- ReportGenerator (dotnet tool)
  - Converts coverlet's Cobertura output into an HTML report and a coverage badge.
- Stryker.NET (`dotnet-stryker` tool)
  - Runs mutation testing against the module and produces a mutation score report.

# Template Skill Mutations
REPOSITORY:
- [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make cucumber-test`/`mutation-test`/`result-page` contract

PROJECT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create|{Module}.Tests.csproj]] - create - test project holding unit tests, Reqnroll features, and step definitions
  - [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - step definitions binding a `.feature` file to the module's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `Rules/{Rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `{Rule}Steps.cs` is created with `[Given]`/`[When]`/`[Then]` bindings that call the module's real validator.
3. `make cucumber-test` runs `dotnet test`, executing both the plain unit tests and the Reqnroll scenarios in `{Module}.Tests`, and normalizes the result into `tmp/result/cucumber-test.json` (plus `tmp/result/coverage-test.json` when `WITH_CODE_COVERAGE=true`).
4. `make mutation-test` runs `dotnet-stryker` — scoped to changed files when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across the whole module otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
5. `make result-page` assembles `public/` from `tmp/result/*.json` and `tmp/report/*`, ready to publish.
6. Which of these `make` targets run on which trigger, and how `public/` gets published to GitHub Pages, is owned by [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) — not by this solution.

## Surviving mutant found (failure path)
1. `make mutation-test` reports a mutant that survived in changed code.
2. The CI job calling it (per [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md)) fails.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create#MUST|{Module}.Tests.csproj]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]

# Check list
- [ ] `{Module}.Tests.csproj` references Reqnroll.xUnit, coverlet.collector, and runs alongside plain unit tests.
- [ ] Every `.feature` scenario has a matching step definition that calls production code.
- [ ] `make cucumber-test`, `make mutation-test`, and `make result-page` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `tmp/result/*.json` and `tmp/report/<kind>/` follow that same contract's schema.
