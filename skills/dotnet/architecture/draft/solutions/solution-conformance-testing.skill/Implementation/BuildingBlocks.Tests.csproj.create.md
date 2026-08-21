---
description: Create the test project for BuildingBlocks — unit tests, Reqnroll feature files, and step definitions together
name: "BuildingBlocks.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `BuildingBlocks` a dedicated test project, referencing exactly what `BuildingBlocks.csproj` itself is allowed to reference.

# Core Principles
- Step definitions are technical-contract-shaped, not validator-shaped — see [[./BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs]]: proving a pipeline behavior's observable contract (e.g. `ExceptionHandlingBehavior`'s catch-log-return-generic-error contract) as its own `.feature` file, not just plain xUnit, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s "technical and architectural functions are described through Cucumber/Gherkin scenarios too" principle.

# Structure

## Project Structure
```
/BuildingBlocks.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  BuildingBlocks.Tests.csproj
```

# NuGet Packages
Same as [[../{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests]]: Reqnroll.xUnit, coverlet.collector, Microsoft.NET.Test.Sdk.

# Allowed Dependencies
- `BuildingBlocks`, `Shared` — the same two projects `BuildingBlocks.csproj` itself is allowed to reference (per `solution-sln-structure`).

# Rules

## MUST
- Reference `BuildingBlocks` and `Shared` only.
  - Risk: referencing more than `BuildingBlocks` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production (e.g. reaching a module project directly).
  - Fix: keep this project's references to exactly `BuildingBlocks` and `Shared`.
- Give a technical/architectural behavior (e.g. a pipeline behavior's ordering or error-handling contract) its own `.feature` file, the same way a business rule would get one.
  - Risk: leaving technical behavior to bare `[Fact]` tests only makes it invisible in the readable report [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] exists to produce.
  - Fix: write a `.feature` file describing the technical contract, with step definitions calling the real `BuildingBlocks` class.

## MUST NOT
- Add a second, separate test project just for `BuildingBlocks`'s Gherkin scenarios.
  - Risk: coverage and mutation-testing reports get computed against only part of `BuildingBlocks`'s test suite.
  - Fix: keep unit tests and scenarios together in this one project.

# Check list
- [ ] `BuildingBlocks.Tests.csproj` references only `BuildingBlocks` and `Shared`, plus Reqnroll.xUnit/coverlet.collector.
- [ ] Every pipeline behavior's technical contract (e.g. `ExceptionHandlingBehavior`) has its own `.feature` file, not just a plain `[Fact]` test.
