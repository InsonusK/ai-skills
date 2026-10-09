---
version: 20261009220000
description: Create the catalog test project for BuildingBlocks and define its reference boundary
name: "BuildingBlocks.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/buildingblocks-tests

---

# Goals
- Give `BuildingBlocks` a dedicated test project, referencing exactly what `BuildingBlocks.csproj` itself is allowed to reference.

# Core Principles
- Step definitions are technical-contract-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: proving a pipeline behavior's observable contract (e.g. `ExceptionHandlingBehavior`'s catch-log-return-generic-error contract) as its own `.feature` file, not just plain xUnit, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s "technical and architectural functions are described through Cucumber/Gherkin scenarios too" principle.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- `BuildingBlocks`, `Shared` — the same two projects `BuildingBlocks.csproj` itself is allowed to reference (per `solution-sln-structure`).

# Rules

## MUST
- Reference `BuildingBlocks` and `Shared` only.
  - Risk: referencing more than `BuildingBlocks` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production (e.g. reaching a module project directly).
  - Fix: keep this project's references to exactly `BuildingBlocks` and `Shared`.
- Give a technical/architectural behavior (e.g. a pipeline behavior's ordering or error-handling contract) its own `.feature` file, the same way a business rule would get one.
  - Risk: leaving technical behavior to bare `[Fact]` tests only makes it invisible in the readable report [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] exists to produce.
  - Fix: write a `.feature` file describing the technical contract, with step definitions calling the real `BuildingBlocks` class.

# Check list
- [ ] `BuildingBlocks.Tests.csproj` references only `BuildingBlocks` and `Shared`.
- [ ] Every pipeline behavior's technical contract (e.g. `ExceptionHandlingBehavior`) has its own `.feature` file, not just a plain `[Fact]` test.
