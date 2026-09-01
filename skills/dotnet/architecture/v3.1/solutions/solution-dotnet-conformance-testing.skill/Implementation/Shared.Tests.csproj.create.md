---
description: Create the test project for Shared — unit tests, Reqnroll feature files, and step definitions together
name: "Shared.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `Shared` a dedicated test project, referencing `Shared` only.

# Core Principles
- Step definitions are value-shaped — see [[./Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs]]: given one or more primitive values, prove how they compare/combine, never "is this input valid" (that belongs to a module's own Domain/Application). `Shared` is mostly interfaces and primitives today, so its own test suite is small — it grows as `Shared` gains real behavior.

# Structure

## Project Structure
```
/Shared.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  Shared.Tests.csproj
```

# NuGet Packages
Same as [[./{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests]]: Reqnroll.xUnit, coverlet.collector, Microsoft.NET.Test.Sdk.

# Allowed Dependencies
- `Shared` — nothing else, mirroring `Shared.csproj`'s own zero project references.

# Rules

## MUST
- Reference `Shared` and nothing else.
  - Risk: referencing any other project here contradicts `Shared`'s own MUST rule of zero project references, and a test could pass by relying on something `Shared` itself is never allowed to depend on.
  - Fix: keep this project scoped to `Shared` only.
- Never add a second, separate test project just for `Shared`'s Gherkin scenarios.
  - Risk: coverage and mutation-testing reports get computed against only part of `Shared`'s test suite.
  - Fix: keep unit tests and scenarios together in this one project.

# Check list
- [ ] `Shared.Tests.csproj` references only `Shared`, plus Reqnroll.xUnit/coverlet.collector.
