---
name: test-project-per-production-project
description: Whether .NET conformance testing gets one combined test project per module, or one test project per production project inside the module
problem: A module has four production projects (Domain, Application, Interfaces, Api), each with its own Allowed Dependencies rule from solution-sln-structure. A single combined {Module}.Tests project cannot state a clear Allowed Dependencies list of its own, because it is unclear which of the four production projects it is actually meant to reference.
decision: One test project per production project — {Module}.Domain.Tests, {Module}.Application.Tests, {Module}.Interfaces.Tests, plus Shared.Tests and BuildingBlocks.Tests for the cross-cutting layer — each mirroring its production counterpart's own Allowed Dependencies exactly. {Module}.Api has no dedicated test project.
tags:
  - solution/conformance-testing
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

An earlier draft of this solution created a single `{Module}.Tests` project per module, combining unit tests and Gherkin scenarios for the whole module in one place. Once module Application, Domain, and Interfaces from solution-sln-structure exist as separate production projects — each with its own Allowed Dependencies rule (Domain: none; Application: own Interfaces + own Domain + other modules' Interfaces; Interfaces: none) — a single combined test project has no way to state a matching Allowed Dependencies list of its own. It either has to reference all of them (turning every test into an integration-style test that can pass by reaching code its production project could never legally reach) or the reference stays a vague placeholder — which is exactly what happened: the previous `{Module}.Tests.csproj.create.md`'s Allowed Dependencies read only `{Module}`, without saying which project that name actually resolved to.

# Selected variant

**Selected variant:** [[#One test project per production project (selected)]]

# Searched variants

## One test project per production project (selected)

### Description

Give each production project its own dedicated test project, with the exact same Allowed Dependencies the production project itself has: `{Module}.Domain.Tests` → `{Module}.Domain` only; `{Module}.Application.Tests` → `{Module}.Application` + `{Module}.Domain`; `{Module}.Interfaces.Tests` → `{Module}.Interfaces` only; `Shared.Tests` → `Shared` only; `BuildingBlocks.Tests` → `BuildingBlocks` + `Shared`. `{Module}.Api` gets no dedicated test project — it is a thin MediatR adapter with no business logic of its own to prove at this layer.

### Benefits

- Every test project answers "what does this actually test" unambiguously — the answer is always exactly one production project.
- A test project's own Allowed Dependencies mirrors its production counterpart, so a test that reaches further than its production project is allowed to reach is itself a structural violation, not just a code-review nit.
- Each production project's own architectural boundary (e.g. Domain has zero project references) is provable in isolation, by a test project with the same zero-reference constraint.

### Costs

- Five test projects to create and wire into the `Makefile`/CI instead of one — more project-file boilerplate per module.
- A scenario that genuinely spans two production projects (e.g. a handler in Application calling a Domain method) has to live in the outer project's test suite (`{Module}.Application.Tests`), which needs to be understood as intentional rather than arbitrary.

## One combined {Module}.Tests project per module

### Description

Keep a single `{Module}.Tests` project referencing "the module" as a whole, holding unit tests and Gherkin scenarios for every layer together.

### Benefits

- One project to create and wire per module, not five.
- No question of which test belongs in which project — everything about the module lives together.

### Costs

- No single, precise Allowed Dependencies statement is possible — the project would need to reference Domain, Application, and Interfaces all at once, which is wider than any single one of them is itself allowed to reference.
- A test can pass by exercising a code path that would be a genuine architectural violation if the production project under test tried to reach it directly (e.g. a "Domain" test that, because the project also references Application, accidentally exercises Application code).
- Coverage and mutation-testing reports are computed against the whole module at once, hiding which specific production project actually has weak coverage.
