---
name: csproj-module-interfaces-tests
description: Project {Module}.Interfaces.Tests in the v1 plateau
whenToUse: when adding a unit test or Gherkin scenario for {Module}.Interfaces, or deciding whether new test code belongs here
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
---

# Goal
- Give `{Module}.Interfaces` a dedicated test project, referencing `{Module}.Interfaces` only.

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

# Core Principles
- Shape-shaped scenarios: `{Module}.Interfaces` is declarations-only, so scenarios prove equality/serialization round-trip, never "is this input valid" (see [[./classes/plateau-v1--class-rule-steps.skill.md|class-rule-steps]]).

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Interfaces.Tests
```

## Project Structure
- /{Module}.Interfaces.Tests
  - /Rules
    - {Rule}.feature
  - /StepDefinitions
    - [{Rule}Steps.cs](./classes/plateau-v1--class-rule-steps.skill.md)
  - {Module}.Interfaces.Tests.csproj

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one contract's shape | |
| /StepDefinitions | Bindings that prove equality/serialization | [[./classes/plateau-v1--class-rule-steps.skill.md\|class-rule-steps]] |

## NuGet Packages
Same as `{Module}.Domain.Tests`: `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`.

## Allowed Dependencies
- `{Module}.Interfaces` — nothing else, mirroring `{Module}.Interfaces.csproj`'s own zero project references.

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Interfaces` and nothing else
- Cover equality and serialization round-trip, not validity — validity belongs to `{Module}.Application.Tests`/`{Module}.Domain.Tests`
MUST NOT:
- Add a second, separate test project just for `{Module}.Interfaces`'s Gherkin scenarios

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]
