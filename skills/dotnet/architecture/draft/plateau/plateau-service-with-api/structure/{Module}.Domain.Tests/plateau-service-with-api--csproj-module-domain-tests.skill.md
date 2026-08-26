---
name: csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the service-with-api plateau
whenToUse: when adding a unit test or Gherkin scenario for {Module}.Domain, or deciding whether new test code belongs here
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/csproj
  - plateau/service-with-api
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Domain` a dedicated test project that runs plain unit tests and Gherkin scenarios together, referencing `{Module}.Domain` only.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]

# Core Principles
- Feature files live under `/Rules`, one file per business rule; step definitions live under `/StepDefinitions`, one class per feature file.
- Step definitions call `{Module}.Domain`'s public API directly — never `{Module}.Application` or `{Module}.Interfaces`, since `{Module}.Domain` itself cannot reach them either.
- Validator-shaped scenarios: input → valid/invalid + error code (see [[./classes/plateau-service-with-api--class-domain-invariant-rule-steps.skill.md|class-domain-invariant-rule-steps]]).

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Tests
```

## Project Structure
- /{Module}.Domain.Tests
  - /Rules
    - {Rule}.feature
  - /StepDefinitions
    - [{Rule}Steps.cs](./classes/plateau-service-with-api--class-domain-invariant-rule-steps.skill.md)
  - {Module}.Domain.Tests.csproj

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one Domain invariant/rule | |
| /StepDefinitions | Bindings that call `{Module}.Domain`'s real entity/value-object code | [[./classes/plateau-service-with-api--class-domain-invariant-rule-steps.skill.md\|class-domain-invariant-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Reqnroll.xUnit` | latest stable | Run `.feature` files as xUnit tests |
| `coverlet.collector` | latest stable | Collect coverage during `dotnet test` |
| `Microsoft.NET.Test.Sdk` | latest stable | Test SDK required by xUnit/Reqnroll |

## Allowed Dependencies
- `{Module}.Domain` — nothing else, mirroring `{Module}.Domain.csproj`'s own zero project references.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Domain` and nothing else
- Step definitions call `{Module}.Domain`'s public API directly, never duplicate the logic they prove
- Configure `dotnet test` to run both `[Fact]`/`[Theory]` unit tests and Reqnroll-generated scenario tests in the same run
MUST NOT:
- Add a second, separate test project just for `{Module}.Domain`'s Gherkin scenarios

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
