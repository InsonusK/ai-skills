---
version: 20261009220001
description: Create the catalog test project for Shared and define its reference boundary
name: "Shared.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/shared-tests

---

# Goals
- Give `Shared` its catalog-selected test project with its production dependency boundary mirrored.

# Core Principles
- Step definitions are value-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: given one or more primitive values, prove how they compare/combine, never "is this input valid" (that belongs to a module's own Domain/Application). `Shared` is mostly interfaces and primitives today, so its own test suite is small — it grows as `Shared` gains real behavior.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- Reference `Shared` and mirror the tested production project's [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md#allowed-dependencies), including only contributions of solutions actually applied to it.

# Rules

## MUST
- Mirror the tested production project's [Allowed Dependencies](#allowed-dependencies), plus that production project itself.
  - Risk: an independently maintained list either permits an illegal dependency or forbids a legitimate production dependency.
  - Fix: derive the boundary from the production skill after its applied solution contributions are assembled.

# Check list
- [ ] Project references stay within the tested production project and its applied Allowed Dependencies.
