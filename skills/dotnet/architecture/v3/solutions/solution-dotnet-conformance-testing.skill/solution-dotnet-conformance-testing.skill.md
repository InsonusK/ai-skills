---
name: solution-dotnet-conformance-testing
description: The .NET implementation of [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] — one test project per production project (mirroring its Allowed Dependencies exactly), Reqnroll for Gherkin scenarios, coverlet + ReportGenerator for coverage, Stryker.NET for mutation testing, and the make unit-test/mutation-test/test-report/test-and-report contract that devops-github-wf-bdd-report-publish's workflows consume
whenToUse: Set up or review the test suite of a .NET library/project that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing .NET project, or wire coverage and mutation testing into a .NET project's `make`/CI pipeline.
domain: skill
type: architecture
version: 2
tags:
  - skill/architecture/solution
  - solution/conformance-testing
  - stack/dotnet
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
  - concern/architecture

creates:
  - "{Module}.Domain.Tests.csproj"
  - "{Module}.Application.Tests.csproj"
  - "{Module}.Interfaces.Tests.csproj"
  - "Shared.Tests.csproj"
  - "BuildingBlocks.Tests.csproj"
  - "{TestProject}.StepDefinitions.{Rule}Steps.cs"
  - Makefile
  - report-template/index.html
extends:
  - README.md
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
  - "[[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|One test project per production project, not per module]]"
---

# Goal
- Give every testable .NET production project — `{Module}.Domain`, `{Module}.Application`, `{Module}.Interfaces`, `Shared`, `BuildingBlocks` — its own test project, so a reader can always answer "what does this test project reference and prove" by looking at exactly one production project.
- Give each of those Gherkin scenarios proven against real production code (Reqnroll), coverage measurement, and mutation testing, exposed behind a uniform `make unit-test`/`make mutation-test`/`make test-report`/`make test-and-report` contract — so CI can drive it without knowing anything .NET-specific.

# Capabilities
- Every production project's Allowed Dependencies rule (from `solution-sln-structure`) has a matching test project with the same, mirrored dependency: `{Module}.Domain.Tests` references only `{Module}.Domain`, `{Module}.Interfaces.Tests` references only `{Module}.Interfaces`, and so on.
- Gherkin `.feature` files execute against real production code via Reqnroll step definitions, in whichever test project owns the code being proven.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-project mutation run on every call.
- `make unit-test WITH_CODE_COVERAGE=true` and `make test-report` give `master` one aggregated coverage/mutation-score report across every test project, and the data the README badges are generated from.

# Core Principles
- One test project per production project, never one combined project per module: `{Module}.Domain.Tests`, `{Module}.Application.Tests`, and `{Module}.Interfaces.Tests` for the module layer; `Shared.Tests` and `BuildingBlocks.Tests` for the cross-cutting layer. `{Module}.Api` has no dedicated test project here — it is a thin MediatR adapter with no business logic of its own to prove (see [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|ADR]]).
- Each test project's Allowed Dependencies mirror its production counterpart's exactly: `{Module}.Application.Tests` may reference `{Module}.Application` and `{Module}.Domain` (the same two `{Module}.Application.csproj` itself is allowed to reference), `BuildingBlocks.Tests` may reference `BuildingBlocks` and `Shared`, and so on. A test project never reaches further than the production project it tests is itself allowed to reach.
- Every test project contains unit tests, Reqnroll feature files, and their step definitions together — never a separate project split out just for Gherkin scenarios.
- Step definitions call the tested project's real public API; they never re-implement the rule under test.
- `make unit-test` runs every test project in one invocation and reports one aggregated result — a caller never invokes an individual test project directly.

# Adr
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: Reqnroll (Gherkin runner) + coverlet/ReportGenerator (coverage) + Stryker.NET (mutation testing)
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|One test project per production project, not per module]]
  - Selected variant: one test project per production project, mirroring its Allowed Dependencies

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - This solution's `Makefile` and scripts implement its `make unit-test`/`mutation-test`/`test-report`/`test-and-report` target names and `tmp/result/*.json` schema exactly — aggregated across all five test projects.
- [[skills/dotnet/architecture/v3/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - Defines the production projects (`{Module}.Domain`, `{Module}.Application`, `{Module}.Interfaces`, `Shared`, `BuildingBlocks`) and their Allowed Dependencies, which each test project here mirrors.
- [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]
  - This solution's `Makefile` is what its CI workflows call; this solution does not itself create any `.github/workflows/*.yml` file.

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
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract across all five test projects

PROJECT:
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create|{Module}.Domain.Tests.csproj]] - create - tests `{Module}.Domain` only
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - validator-shaped step definitions: input → valid/invalid + error code
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create|{Module}.Application.Tests.csproj]] - create - tests `{Module}.Application` (and, transitively, `{Module}.Domain`)
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - command-shaped step definitions: command → handler → `Result`
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create|{Module}.Interfaces.Tests.csproj]] - create - tests `{Module}.Interfaces` only
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - shape-shaped step definitions: equality/serialization round-trip
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]] - create - tests `Shared` only
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - value-shaped step definitions: primitive comparison/combination
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create|BuildingBlocks.Tests.csproj]] - create - tests `BuildingBlocks` (and, transitively, `Shared`)
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - technical-contract-shaped step definitions: pipeline behavior's observable contract

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. Decide which production project owns the rule (e.g. an entity invariant lives in `{Module}.Domain`, a transport check lives in `{Module}.Application`).
2. A `.feature` file describing the rule (e.g. `Rules/{Rule}.feature`) is added or extended, inside that project's own test project (e.g. `{Module}.Domain.Tests/Rules/{Rule}.feature`), with `Given/When/Then` scenarios.
3. `{Rule}Steps.cs` is created in that same test project with `[Given]`/`[When]`/`[Then]` bindings that call the real production code.
4. `make unit-test` runs `dotnet test` across every test project, executing both the plain unit tests and the Reqnroll scenarios, and normalizes the aggregated result into `tmp/result/unit-test.json` (plus `tmp/result/coverage-test.json` when `WITH_CODE_COVERAGE=true`).
5. `make mutation-test` runs `dotnet-stryker` — scoped to changed files when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across every project otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
6. `make test-report` assembles `public/` from `tmp/result/*.json` and `tmp/report/*`, ready to publish.
7. This solution does not decide which `make` targets run on which trigger, or how `public/` gets published to GitHub Pages — see [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) for that.

## Surviving mutant found (failure path)
1. `make mutation-test` reports a mutant that survived in changed code.
2. The CI job calling it (per [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md)) fails.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must|solution-conformance-testing]].

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create#MUST|{Module}.Domain.Tests.csproj]]
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create#MUST|{Module}.Application.Tests.csproj]]
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create#MUST|{Module}.Interfaces.Tests.csproj]]
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create#MUST|Shared.Tests.csproj]]
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create#MUST|BuildingBlocks.Tests.csproj]]
  - [[skills/dotnet/architecture/v3/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- Give every test project exactly the same Allowed Dependencies as the one production project it tests — never wider.
  - Risk: a test project that references more than its production counterpart is allowed to (e.g. `{Module}.Domain.Tests` referencing `{Module}.Application`) can pass by exercising code its own production project could never legally reach, hiding a real dependency violation.
  - Fix: mirror each production project's own Allowed Dependencies list exactly when scoping its test project.

# Check list
- [ ] Every production project (`{Module}.Domain`, `{Module}.Application`, `{Module}.Interfaces`, `Shared`, `BuildingBlocks`) has exactly one test project.
- [ ] Each test project's references match its production counterpart's Allowed Dependencies exactly — no wider, no narrower.
- [ ] `{Module}.Api` has no dedicated test project.
- [ ] Every `.feature` scenario has a matching step definition, in the same test project as the code it proves, that calls production code.
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` exist at the repository root, run across every test project, and support the toggles defined by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].
- [ ] `tmp/result/*.json` and `tmp/report/<kind>/` aggregate all five test projects and follow that same contract's schema.
- [ ] `public/` follows [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#public-site-output|solution-conformance-testing's Public site output]] contract, and `report-template/index.html` exists at the repository root (not under `.github/`).
