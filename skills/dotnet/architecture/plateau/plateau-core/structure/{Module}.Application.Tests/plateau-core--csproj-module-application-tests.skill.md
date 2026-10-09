---
name: plateau-core--csproj-module-application-tests
description: Project {Module}.Application.Tests in the plateau-core plateau — the dedicated test project for {Module}.Application, referencing the same projects {Module}.Application itself may reference
whenToUse: when adding a Gherkin scenario or unit test for a handler or validator, or checking {Module}.Application.Tests keeps to the same reference boundary as {Module}.Application
domain: skill
type: template
plateau: core
version: 20261009220000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Application` a dedicated test project referencing exactly what `{Module}.Application.csproj` may reference. At plateau-core that is `{Module}.Application` (and transitively `{Module}.Interfaces`, `Shared`); `{Module}.Domain` is added to the reference set only once VP1 creates it.
- Prove each handler's orchestration and each validator's rules against the real classes.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create|{Module}.Application.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are command-shaped: a command goes in, a `Result` comes out, asserted against the real handler.
- Validator scenarios run the real `AbstractValidator<T>` and assert the failing rule.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Application.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings driving the real handler / validator | [[skills/dotnet/architecture/plateau/plateau-core/structure/{Module}.Application.Tests/classes/plateau-core--class-module-application-rule-steps.skill\|class-module-application-rule-steps]] |

## What Does NOT Belong Here
- Contract-shape assertions — belong to `{Module}.Interfaces.Tests`.
- A reference to another module's project, or to `App.Host` / infrastructure.

## Allowed Dependencies
- `{Module}.Application` (and, from VP1 on, `{Module}.Domain`); transitively `{Module}.Interfaces`, `Shared`. No other module's project.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Reference `{Module}.Application` (and `{Module}.Domain` once it exists) only — no other module's project, no `{Module}.Interfaces`-only shortcut around `{Module}.Application`'s boundary.
- Handler scenarios prove command orchestration; validator scenarios prove the input boundary.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Check list
- [ ] `{Module}.Application.Tests.csproj` references only `{Module}.Application` (+ `{Module}.Domain` from VP1).
- [ ] Every scenario calls the real handler/validator and asserts its actual result.
