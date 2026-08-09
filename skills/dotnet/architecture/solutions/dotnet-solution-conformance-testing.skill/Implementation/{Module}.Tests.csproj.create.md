---
description: Create the test project holding unit tests, Reqnroll feature files, and step definitions for a module
name: "{Module}.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `{Module}` a single test project that runs plain unit tests and Gherkin scenarios together.

# Core Principles
- Feature files live under `Rules/`, one file per business rule; step definitions live under `StepDefinitions/`, one class per feature file.
- Step definitions call `{Module}`'s public API directly.

# Structure

## Project Structure
```
/{Module}.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  {Module}.Tests.csproj
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /Rules | {Rule}.feature | Gherkin scenarios for one business rule |
| /StepDefinitions | {Rule}Steps.cs | Bindings that call `{Module}`'s real validator |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Reqnroll.xUnit | latest stable | Run `.feature` files as xUnit tests |
| coverlet.collector | latest stable | Collect coverage during `dotnet test` |
| Microsoft.NET.Test.Sdk | latest stable | Test SDK required by xUnit/Reqnroll |

# What Does NOT Belong Here
- Gherkin `.feature` files shared with a non-.NET implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this project.

# Allowed Dependencies
- [[{Module}]]

# Rules

## MUST
- Reference `{Module}` directly; step definitions must call its public types/methods, not duplicate their logic.
- Configure `dotnet test` to run both `[Fact]`/`[Theory]` unit tests and Reqnroll-generated scenario tests in the same run.

## MUST NOT
- Add a second, separate test project just for Gherkin scenarios — unit tests and scenarios stay in one project per module.

# Anti-patterns
- **Split unit tests and Gherkin scenarios into separate test projects**
  - Consequence: coverage and mutation-testing reports get computed against only part of the module's test suite, understating both.
  - Instead: keep both in `{Module}.Tests` so every gate sees the full picture.

# Check list
- [ ] `{Module}.Tests.csproj` references `{Module}`, Reqnroll.xUnit, and coverlet.collector.
- [ ] `dotnet test` runs both plain unit tests and Reqnroll scenarios from one command.
