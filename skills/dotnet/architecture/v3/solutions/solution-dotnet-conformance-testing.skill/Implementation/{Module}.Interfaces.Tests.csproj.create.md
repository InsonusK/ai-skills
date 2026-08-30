---
description: Create the test project for {Module}.Interfaces — unit tests, Reqnroll feature files, and step definitions together
name: "{Module}.Interfaces.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `{Module}.Interfaces` a dedicated test project, referencing `{Module}.Interfaces` only.

# Core Principles
- Step definitions are shape-shaped, not validator-shaped — see [[./{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs]]: `{Module}.Interfaces` is declarations-only, so scenarios prove equality/serialization round-trip, never "is this input valid".

# Structure

## Project Structure
```
/{Module}.Interfaces.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  {Module}.Interfaces.Tests.csproj
```

# NuGet Packages
Same as [[./{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests]]: Reqnroll.xUnit, coverlet.collector, Microsoft.NET.Test.Sdk.

# Allowed Dependencies
- `{Module}.Interfaces` — nothing else. `{Module}.Interfaces.Tests` may reference exactly what `{Module}.Interfaces.csproj` itself is allowed to reference (per `solution-sln-structure`: nothing), plus `{Module}.Interfaces` itself.

# Rules

## MUST
- Reference `{Module}.Interfaces` and nothing else.
  - Risk: referencing `{Module}.Application` or `{Module}.Domain` here would let a contract test depend on implementation details it is meant to be isolated from.
  - Fix: keep this project scoped to `{Module}.Interfaces`'s own declarations.

## MUST NOT
- Add a second, separate test project just for `{Module}.Interfaces`'s Gherkin scenarios.
  - Risk: coverage and mutation-testing reports get computed against only part of `{Module}.Interfaces`'s test suite.
  - Fix: keep unit tests and scenarios together in this one project.

# Check list
- [ ] `{Module}.Interfaces.Tests.csproj` references only `{Module}.Interfaces`, plus Reqnroll.xUnit/coverlet.collector.
