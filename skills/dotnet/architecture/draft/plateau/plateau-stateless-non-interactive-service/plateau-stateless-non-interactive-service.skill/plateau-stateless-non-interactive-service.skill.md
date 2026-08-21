---
name: stateless-non-interactive-service
description: Base composition of the system — module/App layer folder structure, composition-root wiring, global exception handling, and the Cucumber/coverage/mutation conformance-testing gate. No API surface, no persistence, no state — every other plateau composes on top of this one.
whenToUse: when scaffolding a brand-new service before any module has business logic, an API, or persistence — or when reviewing whether App.Host wiring, global exception handling, or the conformance-testing gate follow this baseline
domain: skill
type: template
version: 20260821230300
tags:
  - skill/template/plateau
  - plateau/stateless-non-interactive-service
parent_plateaus:
standalone: false
created_by:
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
---

# Goal
Give every module a fixed four-project shape (Api/Application/Domain/Interfaces), each with its own dedicated test project, wired into a single App.Host composition root with centralized pipeline registration, global unhandled-exception handling, and a Cucumber/coverage/mutation conformance gate — before any module has actual business logic, endpoints, or persistence.

# Core Principles
- Structure: every module lives under `/src/Modules/{ModuleName}` with exactly the base four projects; App.Host is the single composition root and the only project referencing every module at once.
- Pipeline: `PipelineRegistration.AddPipeline()` is the single, ordered source of truth for MediatR pipeline behaviors; `ExceptionHandlingBehavior` is always registered first, so no other behavior's exception escapes unhandled.
- Testing: every production project that has one (Domain, Application, Interfaces, Shared, BuildingBlocks) gets exactly one dedicated test project mirroring its Allowed Dependencies exactly — never one combined project per module, never a project reaching wider than its production counterpart. `{ModuleName}.Api` has no dedicated test project. See [[../../../solutions/solution-conformance-testing.skill/adr/test-project-per-production-project.md|solution-conformance-testing's ADR]].
- CI: this plateau assumes [[skills/devops/devops-github-wf-pr-validation.skill/devops-github-wf-pr-validation.skill.md|devops-github-wf-pr-validation]]'s base PR workflow is wired up, extended per [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]] with the PR-gate Cucumber/mutation jobs and the master-push report/Pages/badges workflow — neither workflow file is produced as `structure/` content here; both are practices to apply directly, referenced the same way `solution-conformance-testing` itself already references them.
- Not standalone: `standalone: false` — this plateau has no API surface and no persistence; nothing can call into it yet. It exists to be composed into every other plateau, not deployed on its own.

# Capabilities
- structure
  - Fixed module shape (Api/Application/Domain/Interfaces, each production project paired with its own `.Tests` project except Api) and non-module layers (Shared, BuildingBlocks, App.Host — each of Shared/BuildingBlocks also paired with its own `.Tests` project). No persistence layer exists yet — no `App.Infrastructure`, no `App.Queries`.
  - Single composition root (`App.Host`) with `AddModules()`/`AddPipeline()` as the only two extension points `Program.cs` calls.
- reliability
  - Every unhandled exception anywhere in the MediatR pipeline is caught, logged at `Critical`, and turned into a generic `Result.Error` — never leaks to the API layer.
- testing
  - Every production project's business/technical/architectural rules are provable as Gherkin scenarios against real production code, alongside plain unit tests, in that project's own dedicated `.Tests` project.
  - `make unit-test`/`make mutation-test`/`make test-report`/`make test-and-report` give a uniform, stack-independent contract for CI to enforce coverage and mutation score, aggregated across every test project.
- ci
  - PR-gate: [[skills/devops/devops-github-wf-pr-validation.skill/devops-github-wf-pr-validation.skill.md|devops-github-wf-pr-validation]]'s base unit-test/version-check jobs, extended by [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]'s Cucumber/mutation-delta jobs.
  - Master-push: full coverage + mutation report, published to GitHub Pages with README badges, via the same `devops-github-wf-bdd-report-publish` workflow.

# Example
A real runnable reference implementation lives in [[./example|example]]. It shows a minimal .NET solution (`StatelessService`) with one `Sample` module, the `Shared`/`BuildingBlocks` layers, `App.Host` as the composition root, and dedicated test projects for every production project except `Sample.Api`. See `example/README.md` for how to run it.

# Usecases

## Scaffold a brand-new module
1. Create `{ModuleName}.Api`, `{ModuleName}.Application`, `{ModuleName}.Domain`, `{ModuleName}.Interfaces` under `/src/Modules/{ModuleName}`, plus `{ModuleName}.Application.Tests`, `{ModuleName}.Domain.Tests`, and `{ModuleName}.Interfaces.Tests` (no `{ModuleName}.Api.Tests`).
2. Add `Register{ModuleName}Module(configuration)` and call it from `ModuleRegistration.AddModules()`.
3. The module has no endpoints or persistence yet — those arrive once a later plateau (API publication, persistence) is composed on top of this one.
4. Add the module's first Gherkin scenario to whichever test project matches the production project the rule belongs to (e.g. an entity invariant → `{ModuleName}.Domain.Tests/Rules`, a handler's orchestration → `{ModuleName}.Application.Tests/Rules`).

## Unhandled exception during request handling
1. A handler or an inner pipeline behavior throws.
2. `ExceptionHandlingBehavior`, registered first in `AddPipeline()`, catches it, logs at `Critical`, and returns `Result.Error("An unexpected error occurred. Please try again later.")`.
3. No exception detail (message, stack trace) reaches the caller. This exact contract is itself proven as a Gherkin scenario in `BuildingBlocks.Tests`.

## Run the conformance gate locally or in CI
1. `make unit-test` runs unit tests + Gherkin scenarios across all five test projects (`{ModuleName}.Domain.Tests`, `{ModuleName}.Application.Tests`, `{ModuleName}.Interfaces.Tests`, `Shared.Tests`, `BuildingBlocks.Tests`); `WITH_CODE_COVERAGE=true` also collects aggregated coverage.
2. `make mutation-test` runs Stryker.NET across the whole solution; `ONLY_DELTA=true DELTA_BASE=<ref>` scopes it to changed code, the mode the PR-gate workflow uses.
3. `make test-report` assembles `public/` from both targets' normalized results, ready to publish.
4. See [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]] for exactly which of these run on which trigger.
