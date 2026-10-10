---
version: 20261009220001
description: Create the catalog test project for {Module}.Domain and define its reference boundary
name: "{Module}.Domain.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-domain-tests-csproj

---

# Goals
- Give `{Module}.Domain` its catalog-selected test project with its production dependency boundary mirrored.

# Core Principles
- Step definitions call `{Module}.Domain`'s public API directly — never `{Module}.Application` or `{Module}.Interfaces`, so the scenario proves the Domain entry point rather than another layer.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# What Does NOT Belong Here
- A scenario that invokes an Application handler instead of a Domain entry point — that scenario belongs in `{Module}.Application.Tests`. Contract types permitted by Domain's Allowed Dependencies may still supply inputs.
- Gherkin `.feature` files shared with a non-.NET implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this project.

# Allowed Dependencies
- Reference `{Module}.Domain` and mirror the tested production project's [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md#allowed-dependencies), including only contributions of solutions actually applied to it.

# Rules

## MUST
- Mirror the tested production project's [Allowed Dependencies](#allowed-dependencies), plus that production project itself.
  - Risk: an independently maintained list either permits an illegal dependency or forbids a legitimate production dependency.
  - Fix: derive the boundary from the production skill after its applied solution contributions are assembled.

# Check list
- [ ] Project references stay within the tested production project and its applied Allowed Dependencies.
