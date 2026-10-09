---
version: 20261009220000
description: Create the catalog test project for {Module}.Domain and define its reference boundary
name: "{Module}.Domain.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-domain-tests-csproj

---

# Goals
- Give `{Module}.Domain` a dedicated test project, referencing `{Module}.Domain` only.

# Core Principles
- Step definitions call `{Module}.Domain`'s public API directly — never `{Module}.Application` or `{Module}.Interfaces`, since `{Module}.Domain` itself cannot reach them either.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# What Does NOT Belong Here
- A scenario that needs `{Module}.Application` or `{Module}.Interfaces` to set up its `Given` step — that scenario belongs in `{Module}.Application.Tests` instead, which is allowed to reference both.
- Gherkin `.feature` files shared with a non-.NET implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this project.

# Allowed Dependencies
- `{Module}.Domain` — nothing else. `{Module}.Domain.Tests` may reference exactly what `{Module}.Domain.csproj` itself is allowed to reference (per `solution-sln-structure`: nothing), plus `{Module}.Domain` itself.

# Rules

## MUST
- Reference `{Module}.Domain` and nothing else — no `{Module}.Application`, no `{Module}.Interfaces`, no other module's project.
  - Risk: referencing a wider set than `{Module}.Domain` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production.
  - Fix: keep this project's references to exactly `{Module}.Domain`.

# Check list
- [ ] `{Module}.Domain.Tests.csproj` references `{Module}.Domain` — nothing else project-wise.
