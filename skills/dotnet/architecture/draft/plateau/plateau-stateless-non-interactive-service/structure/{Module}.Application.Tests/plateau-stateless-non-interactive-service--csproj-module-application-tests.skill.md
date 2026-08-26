---
name: csproj-module-application-tests
description: Project {Module}.Application.Tests in the stateless-non-interactive-service plateau
whenToUse: when adding a unit test or Gherkin scenario for {Module}.Application, or deciding whether new test code belongs here
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260822120000
tags:
  - skill/template/csproj
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Application` a dedicated test project, referencing exactly what `{Module}.Application.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Core Principles
- Command-shaped scenarios: a command goes in, a `Result` comes out, proven against the real handler — never `{Module}.Domain`'s types directly (see [[./classes/plateau-stateless-non-interactive-service--class-rule-steps.skill.md|class-rule-steps]]).

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Application.Tests
```

## Project Structure
- /{Module}.Application.Tests
  - /Rules
    - {Rule}.feature
  - /StepDefinitions
    - [{Rule}Steps.cs](./classes/plateau-stateless-non-interactive-service--class-rule-steps.skill.md)
  - {Module}.Application.Tests.csproj

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one Application orchestration | |
| /StepDefinitions | Bindings that call `{Module}.Application`'s real handlers | [[./classes/plateau-stateless-non-interactive-service--class-rule-steps.skill.md\|class-rule-steps]] |

## NuGet Packages
Same as `{Module}.Domain.Tests`: `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`.

## Allowed Dependencies
- `{Module}.Application`, `{Module}.Domain` — the same two projects `{Module}.Application.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Application` and `{Module}.Domain` only — no other module's project
- Step definitions call `{Module}.Application`'s real handlers/validators, never `{Module}.Domain`'s types directly
MUST NOT:
- Add a second, separate test project just for `{Module}.Application`'s Gherkin scenarios

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]
