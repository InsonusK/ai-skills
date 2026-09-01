---
description: Create the test project for {Module}.Application — unit tests, Reqnroll feature files, and step definitions together
name: "{Module}.Application.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `{Module}.Application` a dedicated test project, referencing exactly what `{Module}.Application.csproj` itself is allowed to reference.

# Core Principles
- Step definitions are command-shaped, not validator-shaped — see [[./{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs]]: a command goes in, a `Result` comes out, proven against the real handler, never `{Module}.Domain`'s types directly.

# Structure

## Project Structure
```
/{Module}.Application.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  {Module}.Application.Tests.csproj
```

# NuGet Packages
Same as [[./{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests]]: Reqnroll.xUnit, coverlet.collector, Microsoft.NET.Test.Sdk.

# Allowed Dependencies
- `{Module}.Application`, `{Module}.Domain` — the same two projects `{Module}.Application.csproj` itself is allowed to reference (per `solution-sln-structure`). No other module's project.

# Rules

## MUST
- Reference `{Module}.Application` and `{Module}.Domain` only — no other module's project, no `{Module}.Interfaces`-only shortcut around `{Module}.Application`'s own boundary.
  - Risk: referencing more than `{Module}.Application` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production.
  - Fix: keep this project's references to exactly `{Module}.Application` and `{Module}.Domain`.
- Step definitions must call `{Module}.Application`'s real handlers/validators — never `{Module}.Domain`'s types directly, and never duplicate the logic they prove.
  - Risk: calling `{Module}.Domain` directly bypasses the orchestration `{Module}.Application` is responsible for, proving the wrong layer.
  - Fix: call the handler/validator under test the same way a real caller would.
- Never add a second, separate test project just for `{Module}.Application`'s Gherkin scenarios.
  - Risk: coverage and mutation-testing reports get computed against only part of `{Module}.Application`'s test suite.
  - Fix: keep unit tests and scenarios together in this one project.

# Check list
- [ ] `{Module}.Application.Tests.csproj` references only `{Module}.Application` and `{Module}.Domain`, plus Reqnroll.xUnit/coverlet.collector.
- [ ] Step definitions call `{Module}.Application`'s handlers/validators, not `{Module}.Domain` directly.
