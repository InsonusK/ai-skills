---
name: csproj-module-application-tests
description: Project {Module}.Application.Tests in the v1 plateau
whenToUse: when adding a unit test or Gherkin scenario for {Module}.Application, or deciding whether new test code belongs here
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give `{Module}.Application` a dedicated test project, referencing exactly what `{Module}.Application.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Core Principles
- Command-shaped scenarios: a command goes in, a `Result` comes out, proven against the real handler — never `{Module}.Domain`'s types directly (see [[./classes/plateau-v1--class-rule-steps.skill.md|class-rule-steps]]).
- Once `solution-domain-rules` is applied, `{Module}.Domain.Rules.Spec`'s `@semantic`/`@domain`-tagged scenarios are linked in here and proven again through the real `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check` — see [[./classes/plateau-v1--class-application-rule-steps.skill.md|class-application-rule-steps]].

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend.md|{Module}.Application.Tests.csproj.extend]]

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
    - [{Rule}Steps.cs](./classes/plateau-v1--class-rule-steps.skill.md)
    - [{Rule}DtoSteps.cs](./classes/plateau-v1--class-application-rule-steps.skill.md) — `@semantic`/`@domain` scenarios linked in from `{Module}.Domain.Rules.Spec`
  - {Module}.Application.Tests.csproj

`{Module}.Application.Tests.csproj` links `{Module}.Domain.Rules.Spec`'s `@semantic`/`@domain`-tagged scenarios in the same way `{Module}.Domain.Rules.Tests` links the whole directory — see that project's own csproj skill for the `<None Include>` pattern.

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend.md|{Module}.Application.Tests.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one Application orchestration | |
| /StepDefinitions | Bindings that call `{Module}.Application`'s real handlers/validators | [[./classes/plateau-v1--class-rule-steps.skill.md\|class-rule-steps]], [[./classes/plateau-v1--class-application-rule-steps.skill.md\|class-application-rule-steps]] |

## NuGet Packages
Same as `{Module}.Domain.Tests`: `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`. Plus `Moq` — needed to mock a `@domain`-tagged scenario's loading step (e.g. `IReadRepository<T>`) without a real persistence implementation.

## Allowed Dependencies
- `{Module}.Application`, `{Module}.Domain` — the same two projects `{Module}.Application.csproj` itself is allowed to reference.

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Application` and `{Module}.Domain` only — no other module's project
- Step definitions call `{Module}.Application`'s real handlers/validators, never `{Module}.Domain`'s types directly
- Every `@semantic`/`@domain`-tagged scenario in a linked `{Module}.Domain.Rules.Spec` file has a matching step class proving the DtoValidator/`{Feature}Check` adapter
- A `@domain` scenario's mock cover only the loading step — the comparison itself still runs for real, inside the rule
MUST NOT:
- Add a second, separate test project just for `{Module}.Application`'s Gherkin scenarios
- Mock or stub the rule's own `Check()`/`IsValid()`

__Applied solutions:__
- [[../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create.md|{Module}.Application.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend.md|{Module}.Application.Tests.csproj.extend]]
