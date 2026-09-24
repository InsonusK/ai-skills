---
name: plateau-domain-service--csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the plateau-domain-service plateau — the dedicated test project for {Module}.Domain, referencing that module's Domain only
whenToUse: when adding a Gherkin scenario or unit test for an entity invariant, a domain service, or a strict Value Object
domain: skill
type: template
plateau: domain-service
version: 20260924000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
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
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain.Tests/classes/plateau-domain-service--class-module-domain-rule-steps.skill.md)
  - reqnroll.json
  - {Module}.Domain.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /StepDefinitions/{Rule}Steps.cs | Bindings asserting entity/VO behavior against the real types | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain.Tests/classes/plateau-domain-service--class-module-domain-rule-steps.skill\|class-module-domain-rule-steps]] |

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
