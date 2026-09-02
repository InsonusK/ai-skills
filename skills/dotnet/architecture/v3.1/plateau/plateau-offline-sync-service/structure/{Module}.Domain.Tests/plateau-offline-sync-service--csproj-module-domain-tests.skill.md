---
name: plateau-offline-sync-service--csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the plateau-offline-sync-service plateau — the dedicated test project for {Module}.Domain, referencing that module's Domain only
whenToUse: when adding a Gherkin scenario or unit test for an entity invariant, a domain service, or a strict Value Object
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
  - "[[../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Give `{Module}.Domain` a dedicated test project referencing that module's `Domain` only, proving entity invariants, domain-service conditions, and strict Value Object validation against the real types.
- Exists only once `{Module}.Domain` exists (VP1).

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj.create]]

# Core Principles
- Scenarios are validator-shaped: an input goes in, valid/invalid comes out — proven against the real entity method / VO constructor, asserting the `DomainException` code on failure.
- References `{Module}.Domain` only — never `{Module}.Application`, never infrastructure.
- Unit tests and Gherkin scenarios live together in this one project; runs on Microsoft.Testing.Platform.

# Structure

## Solution place
```
/tests/{Module}.Domain.Tests
```

## Project Structure
- /{Module}.Domain.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](./classes/plateau-offline-sync-service--class-module-domain-rule-steps.skill.md)
  - /Architecture/[{Module}ArchitectureTests.cs / GuardedPropertyRuleCoverageTests.cs](./classes/plateau-offline-sync-service--class-architecture-tests.skill.md) — Cecil exception-scoping + guarded-property-coverage `[Fact]`s (VP4 companion, VP1-gated; the dead-rule / code-uniqueness checks live in `{Module}.Domain.Rules.Tests`)
  - reqnroll.json
  - {Module}.Domain.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /StepDefinitions/{Rule}Steps.cs | Bindings asserting entity/VO behavior against the real types (+ `@format` rule scenarios via VP4) | [[./classes/plateau-offline-sync-service--class-module-domain-rule-steps.skill.md\|class-module-domain-rule-steps]] |
| /Architecture/*.cs | Cecil: exception-scoping + guarded-property-coverage `[Fact]`s (VP1-gated) | [[./classes/plateau-offline-sync-service--class-architecture-tests.skill.md\|class-architecture-tests]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| Microsoft.NET.Test.Sdk / xunit.v3 / xunit.runner.visualstudio / Reqnroll.xunit.v3 / coverlet.collector | test host, assertions, Gherkin, coverage |

## What Does NOT Belong Here
- Handler/orchestration scenarios — belong to `{Module}.Application.Tests`.
- A reference to `{Module}.Application` or any infrastructure project.

## Allowed Dependencies
- `{Module}.Domain` (and transitively `{Module}.Interfaces`, `Shared`) — nothing else.

# Rules
MUST:
- Reference `{Module}.Domain` only.
- Assert against the real entity method / VO constructor; assert the exact `DomainException.Code` on a failure scenario.
- Keep unit tests and scenarios in this one project; set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] `{Module}.Domain.Tests.csproj` references only `{Module}.Domain` plus the five test packages.
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Failure scenarios assert the exact `DomainException.Code`.
