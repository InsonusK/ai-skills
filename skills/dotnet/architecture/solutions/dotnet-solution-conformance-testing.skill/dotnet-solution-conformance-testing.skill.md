---
name: dotnet-solution-conformance-testing
description: Sets up the .NET side of the Cucumber/coverage/mutation quality gate — Reqnroll for Gherkin scenarios, coverlet + ReportGenerator for coverage, Stryker.NET for mutation testing, and the PR-gate/trunk-gate CI split with README badges
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - dotnet
  - testing
  - bdd
  - cucumber
  - mutation-testing
triggers:
  - Set up or review the test suite of a .NET library/project that must prove conformance to a Cucumber/Gherkin spec
  - Add Gherkin scenarios and step definitions to an existing .NET project
  - Wire coverage and mutation testing into a .NET project's CI pipeline
creates:
  - "{Module}.Tests.csproj"
  - "{Module}.Tests.StepDefinitions.{Rule}Steps.cs"
extends:
  - .github/workflows/ci.yml
  - README.md
depends_on:
  - "[[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]"
adr:
  - "[[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a .NET project the concrete tooling to run the three-layer gate defined by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Make the PR-gate/trunk-gate CI split (delta-scoped mutation testing on PR, full mutation testing on merge to `master`) concrete for a .NET/GitHub Actions setup.

# Capabilities
- Gherkin `.feature` files execute against the project's real production code via Reqnroll step definitions.
- CI fails a PR when a changed line's mutant survives, without paying for a full-repository mutation run on every PR.
- `master` always has an up-to-date coverage and mutation-score report, and the README badges reflect it.

# Core Principles
- One `{Module}.Tests` project per module under test, containing unit tests, Reqnroll feature files, and their step definitions together.
- Step definitions call the module's real public API; they never re-implement the rule under test.
- Coverage and mutation testing both run against `{Module}.Tests`, not against a separate, parallel "spec-only" project.

# Adr
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: Reqnroll (Gherkin runner) + coverlet/ReportGenerator (coverage) + Stryker.NET (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]
  - Defines the PR-gate/trunk-gate split and the badge requirement this solution implements concretely for .NET.

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
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the PR-gate/trunk-gate CI jobs and README badges

PROJECT:
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create|{Module}.Tests.csproj]] - create - test project holding unit tests, Reqnroll features, and step definitions
  - [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - step definitions binding a `.feature` file to the module's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `Rules/{Rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `{Rule}Steps.cs` is created with `[Given]`/`[When]`/`[Then]` bindings that call the module's real validator.
3. `dotnet test` runs both the plain unit tests and the Reqnroll scenarios in `{Module}.Tests`.
4. `dotnet-stryker` runs against the changed files (PR gate) or the whole module (trunk gate) and reports the mutation score.
5. `reportgenerator` turns the coverlet output into a coverage report and badge data.

## Surviving mutant found (failure path)
1. `dotnet-stryker` reports a mutant that survived in changed code.
2. The PR gate fails the check.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#must).

# Rules

## MUST
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create#MUST|{Module}.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]

## MUST NOT
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create#MUST NOT|{Rule}Steps.cs]]

# Anti-patterns
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/{Module}.Tests.csproj.create/{Rule}Steps.cs.create|See {Rule}Steps.cs.create.md]] — a step definition re-implementing the rule instead of calling the module's real validator.
- [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/Implementation/Repository.extend|See Repository.extend.md]] — running full-repository `dotnet-stryker` on every PR instead of scoping it to the diff.

# Check list
- [ ] `{Module}.Tests.csproj` references Reqnroll.xUnit, coverlet.collector, and runs alongside plain unit tests.
- [ ] Every `.feature` scenario has a matching step definition that calls production code.
- [ ] PR-gate CI job runs `dotnet-stryker` scoped to the diff; trunk-gate CI job runs it for the whole module.
- [ ] README shows a coverage badge and a mutation-score badge sourced from the latest trunk-gate run.
