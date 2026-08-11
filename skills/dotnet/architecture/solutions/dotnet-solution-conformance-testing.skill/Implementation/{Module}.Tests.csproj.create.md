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
  - Risk: a step definition with its own copy of the rule's logic can pass even after the real implementation breaks.
  - Fix: call `{Module}`'s public API directly from step definitions.
- Configure `dotnet test` to run both `[Fact]`/`[Theory]` unit tests and Reqnroll-generated scenario tests in the same run.
  - Risk: if only one kind of test runs per invocation, `make cucumber-test` reports an incomplete result and CI/local runs can diverge on what "green" means.
  - Fix: configure the test project so a single `dotnet test` invocation executes both.
- Never add a second, separate test project just for Gherkin scenarios — unit tests and scenarios stay in one project per module.
  - Violation: splitting unit tests and Gherkin scenarios into separate test projects.
  - Risk: coverage and mutation-testing reports get computed against only part of the module's test suite, understating both.
  - Fix: keep both in `{Module}.Tests` so every gate sees the full picture.

# Check list
- [ ] `{Module}.Tests.csproj` references `{Module}`, Reqnroll.xUnit, and coverlet.collector.
- [ ] `dotnet test` runs both plain unit tests and Reqnroll scenarios from one command.
