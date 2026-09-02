---
name: plateau-domain-service--csproj-building-blocks-tests
description: Project BuildingBlocks.Tests in the plateau-domain-service plateau — the dedicated test project for BuildingBlocks (and transitively Shared)
whenToUse: when adding a Gherkin scenario or unit test for a MediatR pipeline behavior, or checking that BuildingBlocks.Tests keeps to BuildingBlocks' own allowed references
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `BuildingBlocks` a dedicated test project referencing exactly what `BuildingBlocks.csproj` references — `BuildingBlocks` (and transitively `Shared`).
- Prove each pipeline behavior's contract as a Gherkin scenario against the real behavior class.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

# Core Principles
- Scenarios drive the real behavior through a hand-built `RequestHandlerDelegate` and assert on the returned `Result` — the behavior is never re-implemented in the step.
- At plateau-core the covered behaviors are `ValidationBehavior` (short-circuit with `Result.Invalid`) and `ExceptionHandlingBehavior` (throw → generic `Result.Error`).
- Unit tests and scenarios live together in this one project.

# Structure

## Solution place
```
/tests/BuildingBlocks.Tests
```

## Project Structure
- /BuildingBlocks.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](./classes/plateau-domain-service--class-building-blocks-rule-steps.skill.md)
  - reqnroll.json
  - BuildingBlocks.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules/{Rule}.feature | Gherkin scenarios for one pipeline behavior | |
| /StepDefinitions/{Rule}Steps.cs | Bindings driving the real behavior | [[./classes/plateau-domain-service--class-building-blocks-rule-steps.skill.md\|class-building-blocks-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk / xunit.v3 / xunit.runner.visualstudio / Reqnroll.xunit.v3 / coverlet.collector | central | test host, assertions, Gherkin, coverage |

## What Does NOT Belong Here
- A module-specific concept — belongs to that module's `.Tests`.
- A reference to any module, `App.Host`, or infrastructure project.

## Allowed Dependencies
- `BuildingBlocks` (and transitively `Shared`) — nothing else.

# Rules
MUST:
- Reference `BuildingBlocks` only.
- Call the real behavior class through a hand-built next-delegate; assert on the actual `Result` status and message, never a hand-computed expected value.
- Keep unit tests and scenarios in this one project; set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

# Check list
- [ ] `BuildingBlocks.Tests.csproj` references only `BuildingBlocks` plus the five test packages.
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Each scenario asserts the exact `Result` the real behavior returns.
