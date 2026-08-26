---
name: csproj-building-blocks-tests
description: Project BuildingBlocks.Tests in the statefull-service plateau
whenToUse: when adding a unit test or Gherkin scenario for BuildingBlocks, or deciding whether new test code belongs here
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/csproj
  - plateau/statefull-service
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `BuildingBlocks` a dedicated test project, referencing exactly what `BuildingBlocks.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

# Core Principles
- Technical-contract-shaped scenarios: proving a pipeline behavior's observable contract (e.g. `ExceptionHandlingBehavior`'s catch-log-return-generic-error contract) as its own `.feature` file, not just plain xUnit (see [[./classes/plateau-statefull-service--class-building-blocks-rule-steps.skill.md|class-building-blocks-rule-steps]]).

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

# Structure

## Solution place
```
/src/BuildingBlocks.Tests
```

## Project Structure
- /BuildingBlocks.Tests
  - /Rules
    - {Rule}.feature
  - /StepDefinitions
    - [{Rule}Steps.cs](./classes/plateau-statefull-service--class-building-blocks-rule-steps.skill.md)
  - BuildingBlocks.Tests.csproj

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one pipeline behavior's contract | |
| /StepDefinitions | Bindings that exercise `BuildingBlocks`'s real behaviors | [[./classes/plateau-statefull-service--class-building-blocks-rule-steps.skill.md\|class-building-blocks-rule-steps]] |

## NuGet Packages
Same as `{Module}.Domain.Tests`: `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`.

## Allowed Dependencies
- `BuildingBlocks`, `Shared` — the same two projects `BuildingBlocks.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]

# Rules
MUST:
- Reference `BuildingBlocks` and `Shared` only
- Give a technical/architectural behavior its own `.feature` file, the same way a business rule would get one
MUST NOT:
- Add a second, separate test project just for `BuildingBlocks`'s Gherkin scenarios

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create.md|BuildingBlocks.Tests.csproj.create]]
