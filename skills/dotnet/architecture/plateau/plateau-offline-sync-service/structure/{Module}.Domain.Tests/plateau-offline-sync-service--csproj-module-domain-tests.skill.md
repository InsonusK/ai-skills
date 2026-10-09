---
name: plateau-offline-sync-service--csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the plateau-offline-sync-service plateau — the dedicated test project for {Module}.Domain, referencing that module's Domain only
whenToUse: when adding a Gherkin scenario or unit test for an entity invariant, a domain service, or a strict Value Object
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220000
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
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are validator-shaped: an input goes in, valid/invalid comes out — proven against the real entity method / VO constructor, asserting the `DomainException` code on failure.
- References `{Module}.Domain` only — never `{Module}.Application`, never infrastructure.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Domain.Tests
```

## Project Structure
With VP4, `/Architecture` hosts exception-scoping and guarded-property-coverage checks; the rule-only Cecil checks belong in `Domain.Rules.Tests`. Local features prove entity/domain-service/strict-VO behavior; linked `format/` features prove the VO adapter.

Apply the linked [testing conventions](#testing-conventions).

`{Module}.Domain.Rules.Spec` is linked, not referenced as a project. Link its `format/` folder only, so `semantic/`/`domain/` files — which this project has no step definitions for — are never dragged in. `ReqnrollFeatureFiles`, never `None` — Reqnroll generates no test for a `None` item (see [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/adr/spec-folders-per-classification|ADR]]):
```xml
<ReqnrollFeatureFiles Include="..\..\src\Modules\{ModuleName}\{ModuleName}.Domain.Rules.Spec\format\**\*.feature" Link="features\Shared\%(RecursiveDir)%(Filename)%(Extension)" />
```

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings asserting entity/VO behavior against the real types (+ `@format` rule scenarios via VP4) | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-module-domain-rule-steps.skill\|class-module-domain-rule-steps]] |
| /Architecture/*.cs | Cecil: exception-scoping + guarded-property-coverage `[Fact]`s (VP1-gated) | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill\|class-architecture-tests]] |

## What Does NOT Belong Here
- Handler/orchestration scenarios — belong to `{Module}.Application.Tests`.
- A reference to `{Module}.Application` or any infrastructure project.

## Allowed Dependencies
- `{Module}.Domain` (and transitively `{Module}.Interfaces`, `Shared`) — nothing else.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Reference `{Module}.Domain` only.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Check list
- [ ] `{Module}.Domain.Tests.csproj` references only `{Module}.Domain`.

Feature files follow [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]] for mandatory feature type and scenario/Examples category tags; architecture classification tags and the existing documentary exceptions remain separate.
