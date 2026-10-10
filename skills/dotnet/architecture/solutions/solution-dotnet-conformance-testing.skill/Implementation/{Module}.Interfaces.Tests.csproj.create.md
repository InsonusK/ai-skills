---
version: 20261009220001
description: Create the catalog test project for {Module}.Interfaces and define its reference boundary
name: "{Module}.Interfaces.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-interfaces-tests

---

# Goals
- Give `{Module}.Interfaces` its catalog-selected test project with its production dependency boundary mirrored.

# Core Principles
- Step definitions are shape-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: `{Module}.Interfaces` is declarations-only, so scenarios prove equality/serialization round-trip, never "is this input valid".

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- Reference `{Module}.Interfaces` and mirror the tested production project's [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md#allowed-dependencies), including only contributions of solutions actually applied to it.

# Rules

## MUST
- Mirror the tested production project's [Allowed Dependencies](#allowed-dependencies), plus that production project itself.
  - Risk: an independently maintained list either permits an illegal dependency or forbids a legitimate production dependency.
  - Fix: derive the boundary from the production skill after its applied solution contributions are assembled.

# Check list
- [ ] Project references stay within the tested production project and its applied Allowed Dependencies.
