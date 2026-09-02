---
name: plateau-core--csproj-module-application-tests
description: Project {Module}.Application.Tests in the plateau-core plateau — the dedicated test project for {Module}.Application, referencing the same projects {Module}.Application itself may reference
whenToUse: when adding a Gherkin scenario or unit test for a handler or validator, or checking {Module}.Application.Tests keeps to the same reference boundary as {Module}.Application
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Application` a dedicated test project referencing exactly what `{Module}.Application.csproj` may reference. At plateau-core that is `{Module}.Application` (and transitively `{Module}.Interfaces`, `Shared`); `{Module}.Domain` is added to the reference set only once VP1 creates it.
- Prove each handler's orchestration and each validator's rules against the real classes.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Core Principles
- Scenarios are command-shaped: a command goes in, a `Result` comes out, asserted against the real handler — the step never re-implements the orchestration.
- A failure scenario asserts the exact error code/message, not just `IsSuccess == false`.
- Validator scenarios run the real `AbstractValidator<T>` and assert the failing rule.
- Unit tests and scenarios live together in this one project.

# Structure

## Solution place
```
/tests/{Module}.Application.Tests
```

## Project Structure
- /{Module}.Application.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](./classes/plateau-core--class-module-application-rule-steps.skill.md)
  - reqnroll.json
  - {Module}.Application.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules/{Rule}.feature | Gherkin scenarios for one handler / validator | |
| /StepDefinitions/{Rule}Steps.cs | Bindings driving the real handler / validator | [[./classes/plateau-core--class-module-application-rule-steps.skill.md\|class-module-application-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk / xunit.v3 / xunit.runner.visualstudio / Reqnroll.xunit.v3 / coverlet.collector | central | test host, assertions, Gherkin, coverage |

## What Does NOT Belong Here
- Contract-shape assertions — belong to `{Module}.Interfaces.Tests`.
- A reference to another module's project, or to `App.Host` / infrastructure.

## Allowed Dependencies
- `{Module}.Application` (and, from VP1 on, `{Module}.Domain`); transitively `{Module}.Interfaces`, `Shared`. No other module's project.

# Rules
MUST:
- Reference `{Module}.Application` (and `{Module}.Domain` once it exists) only — no other module's project, no `{Module}.Interfaces`-only shortcut around `{Module}.Application`'s boundary.
- Call the real handler / validator; assert the exact `Result` or failing rule; keep unit tests and scenarios together; set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Never re-implement a handler's load/dispatch/return logic inside a step.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] `{Module}.Application.Tests.csproj` references only `{Module}.Application` (+ `{Module}.Domain` from VP1) plus the five test packages.
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Every scenario calls the real handler/validator and asserts its actual result.
