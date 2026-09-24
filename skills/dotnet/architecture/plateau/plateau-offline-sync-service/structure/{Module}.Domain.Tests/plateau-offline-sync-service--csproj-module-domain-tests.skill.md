---
name: plateau-offline-sync-service--csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the plateau-offline-sync-service plateau — the dedicated test project for {Module}.Domain, referencing that module's Domain only
whenToUse: when adding a Gherkin scenario or unit test for an entity invariant, a domain service, or a strict Value Object
domain: skill
type: template
plateau: offline-sync-service
version: 20260924000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
  - "[[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]]"
  - "[[skills/dotnet/architecture/solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill|solution-cecil-architecture-tests]]"
---

# Goal
- Give `{Module}.Domain` a dedicated test project referencing that module's `Domain` only, proving entity invariants, domain-service conditions, and strict Value Object validation against the real types.
- Exists only once `{Module}.Domain` exists (VP1).

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create|{Module}.Domain.Tests.csproj]]

# Core Principles
- Scenarios are validator-shaped: an input goes in, valid/invalid comes out — proven against the real entity method / VO constructor, asserting the `DomainException` code on failure.
- References `{Module}.Domain` only — never `{Module}.Application`, never infrastructure.
- Unit tests and Gherkin scenarios live together in this one project; runs on the VSTest runner (xUnit v2).

# Structure

## Solution place
```
/tests/{Module}.Domain.Tests
```

## Project Structure
- /{Module}.Domain.Tests
  - /Rules/{Rule}.feature — entity-invariant / domain-service / strict-VO scenarios owned by this project
  - /Rules/Shared/*.feature — linked in (`<None Include>`), never copied: `{Module}.Domain.Rules.Spec`'s `@format`-tagged scenarios, re-proven through the VO constructor (VP4)
  - /StepDefinitions/[{Rule}Steps.cs](skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-module-domain-rule-steps.skill.md)
  - /Architecture/[{Module}ArchitectureTests.cs / GuardedPropertyRuleCoverageTests.cs](skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill.md) — Cecil exception-scoping + guarded-property-coverage `[Fact]`s (VP4 companion, VP1-gated; the dead-rule / code-uniqueness checks live in `{Module}.Domain.Rules.Tests`)
  - reqnroll.json
  - {Module}.Domain.Tests.csproj

`{Module}.Domain.Rules.Spec` is linked, not referenced as a project. Link one entry per spec file this layer proves — a file whose scenarios are all `@format` — so a later `@semantic`/`@domain` file is not dragged in:
```xml
<None Include="..\..\src\Modules\{ModuleName}\{ModuleName}.Domain.Rules.Spec\{Rule}.feature" Link="Rules\Shared\{Rule}.feature" />
```

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /StepDefinitions/{Rule}Steps.cs | Bindings asserting entity/VO behavior against the real types (+ `@format` rule scenarios via VP4) | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-module-domain-rule-steps.skill\|class-module-domain-rule-steps]] |
| /Architecture/*.cs | Cecil: exception-scoping + guarded-property-coverage `[Fact]`s (VP1-gated) | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill\|class-architecture-tests]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| Microsoft.NET.Test.Sdk / xunit / xunit.runner.visualstudio / Reqnroll.xUnit / coverlet.collector | test host, assertions, Gherkin, coverage |

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
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Check list
- [ ] `{Module}.Domain.Tests.csproj` references only `{Module}.Domain` plus the five test packages.
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Failure scenarios assert the exact `DomainException.Code`.
