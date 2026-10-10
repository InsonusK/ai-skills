---
version: 20261009220001
description: Create the catalog test project for BuildingBlocks and define its reference boundary
name: "BuildingBlocks.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/buildingblocks-tests

---

# Goals
- Give `BuildingBlocks` its catalog-selected test project with its production dependency boundary mirrored.

# Core Principles
- Step definitions are technical-contract-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: proving a pipeline behavior's observable contract (e.g. `ExceptionHandlingBehavior`'s catch-log-return-generic-error contract) as its own `.feature` file, not just plain xUnit, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s "technical and architectural functions are described through Cucumber/Gherkin scenarios too" principle.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- Reference `BuildingBlocks` and mirror the tested production project's [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md#allowed-dependencies), including only contributions of solutions actually applied to it.

# Rules

## MUST
- Mirror the tested production project's [Allowed Dependencies](#allowed-dependencies), plus that production project itself.
  - Risk: an independently maintained list either permits an illegal dependency or forbids a legitimate production dependency.
  - Fix: derive the boundary from the production skill after its applied solution contributions are assembled.
- Give a technical/architectural behavior (e.g. a pipeline behavior's ordering or error-handling contract) its own `.feature` file, the same way a business rule would get one.
  - Risk: leaving technical behavior to bare `[Fact]` tests only makes it invisible in the readable report [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] exists to produce.
  - Fix: write a `.feature` file describing the technical contract, with step definitions calling the real `BuildingBlocks` class.

# Check list
- [ ] Project references stay within the tested production project and its applied Allowed Dependencies.
- [ ] Every pipeline behavior's technical contract (e.g. `ExceptionHandlingBehavior`) has its own `.feature` file, not just a plain `[Fact]` test.
