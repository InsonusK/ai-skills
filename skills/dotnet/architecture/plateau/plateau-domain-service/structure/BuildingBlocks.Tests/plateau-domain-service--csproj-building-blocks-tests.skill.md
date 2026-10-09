---
name: plateau-domain-service--csproj-building-blocks-tests
description: Project BuildingBlocks.Tests in the plateau-domain-service plateau — the dedicated test project for BuildingBlocks (and transitively Shared)
whenToUse: when adding a Gherkin scenario or unit test for a MediatR pipeline behavior, or checking that BuildingBlocks.Tests keeps to BuildingBlocks' own allowed references
domain: skill
type: template
plateau: domain-service
version: 20261009220000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `BuildingBlocks` a dedicated test project referencing exactly what `BuildingBlocks.csproj` references — `BuildingBlocks` (and transitively `Shared`).
- Prove each pipeline behavior's contract as a Gherkin scenario against the real behavior class.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create|BuildingBlocks.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Pipeline scenarios supply a hand-built `RequestHandlerDelegate` and observe the returned `Result`.
- At plateau-core the covered behaviors are `ValidationBehavior` (short-circuit with `Result.Invalid`) and `ExceptionHandlingBehavior` (throw → generic `Result.Error`).

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/BuildingBlocks.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings driving the real behavior | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks.Tests/classes/plateau-domain-service--class-building-blocks-rule-steps.skill\|class-building-blocks-rule-steps]] |

## What Does NOT Belong Here
- A module-specific concept — belongs to that module's `.Tests`.
- A reference to any module, `App.Host`, or infrastructure project.

## Allowed Dependencies
- `BuildingBlocks` (and transitively `Shared`) — nothing else.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Reference `BuildingBlocks` only.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create|BuildingBlocks.Tests.csproj]]

# Check list
- [ ] `BuildingBlocks.Tests.csproj` references only `BuildingBlocks`.
- [ ] Each scenario asserts the exact `Result` the real behavior returns.
