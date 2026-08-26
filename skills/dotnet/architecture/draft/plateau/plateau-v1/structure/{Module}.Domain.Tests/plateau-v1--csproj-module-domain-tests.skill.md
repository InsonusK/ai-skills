---
name: csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the v1 plateau
whenToUse: when adding a unit test or Gherkin scenario for {Module}.Domain, or deciding whether new test code belongs here
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
  - "[[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Give `{Module}.Domain` a dedicated test project that runs plain unit tests and Gherkin scenarios together, referencing `{Module}.Domain` (and, at this plateau, `{Module}.Domain.Rules`).
- Prove, via Mono.Cecil over compiled IL, that the rule mechanism itself is wired correctly across `{Module}.Domain`/`{Module}.Domain.Rules` — dead rules, exception scoping, code uniqueness, guarded-property coverage.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
- [[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]

# Core Principles
- Feature files live under `/Rules`, one file per business rule; step definitions live under `/StepDefinitions`, one class per feature file.
- Step definitions call `{Module}.Domain`'s public API directly — never `{Module}.Application` or `{Module}.Interfaces`, since `{Module}.Domain` itself cannot reach them either.
- Validator-shaped scenarios: input → valid/invalid + error code (see [[./classes/plateau-v1--class-domain-invariant-rule-steps.skill.md|class-domain-invariant-rule-steps]]).
- Once `solution-domain-rules` is applied, `{Module}.Domain.Rules.Spec`'s `@format`-tagged scenarios are linked in here too and proven again through the VO constructor / Entity method — see [[./classes/plateau-v1--class-domain-rule-steps.skill.md|class-domain-rule-steps]].
- `/Architecture` holds the four Mono.Cecil structural checks — build-time guarantees plain unit/BDD tests cannot give by construction, over compiled IL rather than by executing it — see [[./classes/plateau-v1--class-module-architecture-tests.skill.md|class-module-architecture-tests]] and [[./classes/plateau-v1--class-guarded-property-rule-coverage-tests.skill.md|class-guarded-property-rule-coverage-tests]].

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]
- [[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]

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
    - [{Rule}Steps.cs](./classes/plateau-v1--class-domain-invariant-rule-steps.skill.md)
    - [{Rule}VoSteps.cs](./classes/plateau-v1--class-domain-rule-steps.skill.md) — `@format` scenarios linked in from `{Module}.Domain.Rules.Spec`
  - /Architecture
    - [{Module}ArchitectureTests.cs](./classes/plateau-v1--class-module-architecture-tests.skill.md)
    - [GuardedPropertyRuleCoverageTests.cs](./classes/plateau-v1--class-guarded-property-rule-coverage-tests.skill.md)
    - {Check}.feature (documentary only, see [[./classes/plateau-v1--class-check-feature.skill.md|class-check-feature]])
  - {Module}.Domain.Tests.csproj

`{Module}.Domain.Tests.csproj` links `{Module}.Domain.Rules.Spec`'s `@format`-tagged scenarios in the same way `{Module}.Domain.Rules.Tests` links the whole directory — see that project's own csproj skill for the `<None Include>` pattern.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]
- [[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Gherkin scenarios for one Domain invariant/rule | |
| /StepDefinitions | Bindings that call `{Module}.Domain`'s real entity/value-object code | [[./classes/plateau-v1--class-domain-invariant-rule-steps.skill.md\|class-domain-invariant-rule-steps]], [[./classes/plateau-v1--class-domain-rule-steps.skill.md\|class-domain-rule-steps]] |
| /Architecture | Mono.Cecil structural checks over compiled `{Module}.Domain`/`{Module}.Domain.Rules` IL | [[./classes/plateau-v1--class-module-architecture-tests.skill.md\|class-module-architecture-tests]], [[./classes/plateau-v1--class-guarded-property-rule-coverage-tests.skill.md\|class-guarded-property-rule-coverage-tests]], [[./classes/plateau-v1--class-check-feature.skill.md\|class-check-feature]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Reqnroll.xUnit` | latest stable | Run `.feature` files as xUnit tests |
| `coverlet.collector` | latest stable | Collect coverage during `dotnet test` |
| `Microsoft.NET.Test.Sdk` | latest stable | Test SDK required by xUnit/Reqnroll |
| `Mono.Cecil` | latest stable | Read compiled IL for the four Architecture checks |

## Allowed Dependencies
- `{Module}.Domain` — mirroring `{Module}.Domain.csproj`'s own zero-project-reference baseline
- `{Module}.Domain.Rules` — new at this plateau, needed both to prove the `@format`-redirected VO/Entity adapter and to load the assembly the dead-rule/code-uniqueness Cecil checks scan

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]

# Rules
MUST:
- Reference `{Module}.Domain` (and, at this plateau, `{Module}.Domain.Rules`) — nothing else
- Step definitions call `{Module}.Domain`'s public API directly, never duplicate the logic they prove
- Configure `dotnet test` to run both `[Fact]`/`[Theory]` unit tests and Reqnroll-generated scenario tests in the same run
- Every `@format`-tagged scenario in a linked `{Module}.Domain.Rules.Spec` file has a matching step class proving the VO/Entity adapter
- Every architecture check load its target assembly via `typeof(KnownType).Assembly.Location`, never a hardcoded path
MUST NOT:
- Add a second, separate test project just for `{Module}.Domain`'s Gherkin scenarios
- Reference `{Module}.Application` or any other module's project, even to prove an architecture check

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]
- [[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj.extend]]
