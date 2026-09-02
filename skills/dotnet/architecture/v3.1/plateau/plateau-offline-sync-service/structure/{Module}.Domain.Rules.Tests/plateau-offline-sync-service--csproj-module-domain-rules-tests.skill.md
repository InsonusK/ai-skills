---
name: plateau-offline-sync-service--csproj-module-domain-rules-tests
description: Project {Module}.Domain.Rules.Tests in the plateau-offline-sync-service plateau — the dedicated test project for {Module}.Domain.Rules, isolating its mutation-testing surface
whenToUse: when adding a scenario proving a Rule's own Check() / IsValid() / IRuleBuilder extension, or checking the Domain.Rules test isolation
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Give `{Module}.Domain.Rules` a dedicated test project referencing it only, so the rule mechanism's mutation-testing surface is isolated from the broader entity/VO surface of `{Module}.Domain.Tests`.
- Prove every scenario in the rule's `.feature` file directly against `IsValid()` / `Check()` / the `IRuleBuilder` extension — the one place the rule's own correctness is proven in isolation.

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

# Core Principles
- References `{Module}.Domain.Rules` (+ `Mono.Cecil` and the production assemblies the Cecil dead-rule check inspects).
- Links in both its own `Rules/*.feature` (rule-only edge cases) and, physically, `{Module}.Domain.Rules.Spec`'s shared `.feature` files, generating its own Reqnroll fixture bound to its own step definitions.
- Proves every scenario regardless of `@format`/`@semantic`/`@domain` tag — the other layers only re-prove their applicable subset.

# Structure

## Solution place
```
/tests/{Module}.Domain.Rules.Tests
```

## Project Structure
- /{Module}.Domain.Rules.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}RuleSteps.cs](./classes/plateau-offline-sync-service--class-rule-steps.skill.md)
  - /Architecture/[{Module}RuleArchitectureTests.cs](../{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill.md) — Cecil dead-rule + code-uniqueness `[Fact]`s (always run with VP4)
  - reqnroll.json
  - {Module}.Domain.Rules.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /StepDefinitions/{Rule}RuleSteps.cs | Bindings proving the rule's own `IsValid()`/`Check()` | [[./classes/plateau-offline-sync-service--class-rule-steps.skill.md\|class-rule-steps]] |
| /Architecture/{Module}RuleArchitectureTests.cs | Cecil: dead-rule detection + rejection-code uniqueness/format | [[../{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill.md\|class-architecture-tests]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| Microsoft.NET.Test.Sdk / xunit.v3 / xunit.runner.visualstudio / Reqnroll.xunit.v3 / coverlet.collector | test host, assertions, Gherkin, coverage |

## What Does NOT Belong Here
- The VO/entity fail-fast adapter proof — that is `{Module}.Domain.Tests`.
- The DTO-validator collect-all adapter proof — that is `{Module}.Application.Tests`.
- A reference to any project other than `{Module}.Domain.Rules`.

## Allowed Dependencies
- `{Module}.Domain.Rules` (and transitively `{Module}.Interfaces`, `Shared`); plus `{Module}.Domain` / `{Module}.Application` for the Cecil dead-rule scan when they exist. NuGet: the five test packages + `Mono.Cecil`.

# Rules
MUST:
- Reference `{Module}.Domain.Rules` (+ `Mono.Cecil` for the Architecture checks, + the production assemblies the dead-rule check inspects).
- Host **only** the two rules-only Cecil checks (`{Module}RuleArchitectureTests`) in `/Architecture` — exception-scoping and guarded-property-coverage belong in `{Module}.Domain.Tests`.
- Prove every scenario in the rule's `.feature` directly against `IsValid()`/`Check()`; assert the exact error code on a failure scenario.
- Keep unit tests and scenarios together; set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Never duplicate scenario text — link the physical `.feature` file from `{Module}.Domain.Rules.Spec`.

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Check list
- [ ] `{Module}.Domain.Rules.Tests.csproj` references only `{Module}.Domain.Rules` plus the five test packages.
- [ ] Proves every `.feature` scenario against `IsValid()`/`Check()`.
- [ ] No scenario text duplicated across the three rule-proving test projects.
