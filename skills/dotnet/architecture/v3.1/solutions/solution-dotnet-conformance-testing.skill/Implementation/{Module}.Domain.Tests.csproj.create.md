---
description: Create the test project for {Module}.Domain — unit tests, Reqnroll feature files, and step definitions together
name: "{Module}.Domain.Tests"
element_kind: project
change_kind: create
---

# Goals
- Give `{Module}.Domain` a dedicated test project that runs plain unit tests and Gherkin scenarios together, referencing `{Module}.Domain` only.

# Core Principles
- Feature files live under `Rules/`, one file per business rule; step definitions live under `StepDefinitions/`, one class per feature file.
- Step definitions call `{Module}.Domain`'s public API directly — never `{Module}.Application` or `{Module}.Interfaces`, since `{Module}.Domain` itself cannot reach them either.

# Structure

## Project Structure
```
/{Module}.Domain.Tests
  /Rules
    {Rule}.feature
  /StepDefinitions
    {Rule}Steps.cs
  {Module}.Domain.Tests.csproj
```

## Directory and class skills

| Directory | file | Description |
| ----------------- | ----------- |
| /Rules | {Rule}.feature | Gherkin scenarios for one Domain invariant/rule |
| /StepDefinitions | {Rule}Steps.cs | Bindings that call `{Module}.Domain`'s real entity/value-object code |

# NuGet Packages

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Reqnroll.xUnit | latest stable | Run `.feature` files as xUnit tests |
| coverlet.collector | latest stable | Collect coverage during `dotnet test` |
| Microsoft.NET.Test.Sdk | latest stable | Test SDK required by xUnit/Reqnroll |

# What Does NOT Belong Here
- A scenario that needs `{Module}.Application` or `{Module}.Interfaces` to set up its `Given` step — that scenario belongs in `{Module}.Application.Tests` instead, which is allowed to reference both.
- Gherkin `.feature` files shared with a non-.NET implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this project.

# Allowed Dependencies
- `{Module}.Domain` — nothing else. `{Module}.Domain.Tests` may reference exactly what `{Module}.Domain.csproj` itself is allowed to reference (per `solution-sln-structure`: nothing), plus `{Module}.Domain` itself.

# Rules

## MUST
- Reference `{Module}.Domain` and nothing else — no `{Module}.Application`, no `{Module}.Interfaces`, no other module's project.
  - Risk: referencing a wider set than `{Module}.Domain` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production.
  - Fix: keep this project's references to exactly `{Module}.Domain`.
- Step definitions must call `{Module}.Domain`'s public types/methods directly; step definitions must not duplicate the logic they prove.
  - Risk: a step definition with its own copy of the rule's logic can pass even after the real implementation breaks.
  - Fix: call `{Module}.Domain`'s public API directly from step definitions.
- Configure `dotnet test` to run both `[Fact]`/`[Theory]` unit tests and Reqnroll-generated scenario tests in the same run.
  - Risk: if only one kind of test runs per invocation, `make unit-test` reports an incomplete result and CI/local runs can diverge on what "green" means.
  - Fix: configure the test project so a single `dotnet test` invocation executes both.
- Never add a second, separate test project just for `{Module}.Domain`'s Gherkin scenarios — unit tests and scenarios stay in this one project.
  - Risk: coverage and mutation-testing reports get computed against only part of `{Module}.Domain`'s test suite, understating both.
  - Fix: keep both in `{Module}.Domain.Tests` so every gate sees the full picture for this project.

# Check list
- [ ] `{Module}.Domain.Tests.csproj` references `{Module}.Domain`, Reqnroll.xUnit, and coverlet.collector — nothing else project-wise.
- [ ] `dotnet test` runs both plain unit tests and Reqnroll scenarios from one command.
