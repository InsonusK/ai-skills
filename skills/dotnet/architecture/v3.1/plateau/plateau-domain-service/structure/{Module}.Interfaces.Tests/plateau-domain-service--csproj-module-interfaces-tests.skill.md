---
name: plateau-domain-service--csproj-module-interfaces-tests
description: Project {Module}.Interfaces.Tests in the plateau-domain-service plateau — the dedicated test project for {Module}.Interfaces, referencing that module's Interfaces only
whenToUse: when adding a Gherkin scenario or unit test that pins a public contract's shape (a command's marker, a DTO's fields), or checking {Module}.Interfaces.Tests keeps to the Interfaces-only boundary
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Interfaces` a dedicated test project referencing that module's `Interfaces` only.
- Pin the shape of the module's public contracts — a command implements the right marker, a response DTO carries the expected fields as `Soft{ValueObject}`/primitives.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

# Core Principles
- Scenarios are contract-shaped: construct the declared type and assert it is assignable to the right marker / carries the right members.
- No handler, no validator, no domain type is referenced — only the module's `Interfaces`.
- Unit tests and scenarios live together in this one project.

# Structure

## Solution place
```
/tests/{Module}.Interfaces.Tests
```

## Project Structure
- /{Module}.Interfaces.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](./classes/plateau-domain-service--class-module-interfaces-rule-steps.skill.md)
  - reqnroll.json
  - {Module}.Interfaces.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules/{Rule}.feature | Gherkin scenarios pinning contract shapes | |
| /StepDefinitions/{Rule}Steps.cs | Bindings constructing and inspecting the real contract types | [[./classes/plateau-domain-service--class-module-interfaces-rule-steps.skill.md\|class-module-interfaces-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk / xunit.v3 / xunit.runner.visualstudio / Reqnroll.xunit.v3 / coverlet.collector | central | test host, assertions, Gherkin, coverage |

## What Does NOT Belong Here
- Handler/validator behavior — belongs to `{Module}.Application.Tests`.
- A reference to `{Module}.Application`, `{Module}.Domain`, or another module.

## Allowed Dependencies
- `{Module}.Interfaces` (and transitively `Shared`) — nothing else.

# Rules
MUST:
- Reference `{Module}.Interfaces` only.
- Assert against the real declared contract type; keep unit tests and scenarios in this one project; set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Never reach into `{Module}.Application` or `{Module}.Domain` for a shortcut.

__Applied solutions:__
- [[../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create.md|{Module}.Interfaces.Tests.csproj.create]]

# Check list
- [ ] `{Module}.Interfaces.Tests.csproj` references only `{Module}.Interfaces` plus the five test packages.
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Every scenario constructs and inspects a real contract type.
