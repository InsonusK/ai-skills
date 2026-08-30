---
name: plateau-statefull-service--csproj-shared-tests
description: Project Shared.Tests in the statefull-service plateau
whenToUse: when adding a unit test or Gherkin scenario for Shared, or deciding whether new test code belongs here
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
- Give `Shared` a dedicated test project, referencing `Shared` only.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create.md|Shared.Tests.csproj.create]]

# Core Principles
- Value-shaped scenarios: given one or more primitive values, prove how they compare/combine — never "is this input valid" (see [[./classes/plateau-statefull-service--class-shared-rule-steps.skill.md|class-shared-rule-steps]]). `Shared` is mostly interfaces and primitives today, so its own test suite is small.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create.md|Shared.Tests.csproj.create]]

# Structure

## Solution place
```
/src/Shared.Tests
```

## Project Structure
- /Shared.Tests
  - /Rules
    - {Rule}.feature
  - /StepDefinitions
    - [{Rule}Steps.cs](./classes/plateau-statefull-service--class-shared-rule-steps.skill.md)
  - Shared.Tests.csproj

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create.md|Shared.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one Shared primitive | |
| /StepDefinitions | Bindings that call `Shared`'s real primitives | [[./classes/plateau-statefull-service--class-shared-rule-steps.skill.md\|class-shared-rule-steps]] |

## NuGet Packages
Same as `{Module}.Domain.Tests`: `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`.

## Allowed Dependencies
- `Shared` — nothing else, mirroring `Shared.csproj`'s own zero project references.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create.md|Shared.Tests.csproj.create]]

# Rules
MUST:
- Reference `Shared` and nothing else
MUST NOT:
- Introduce a module-specific concept into a `Shared.Tests` scenario
- Add a second, separate test project just for `Shared`'s Gherkin scenarios

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create.md|Shared.Tests.csproj.create]]
