---
name: plateau-core--csproj-building-blocks-tests
description: Project BuildingBlocks.Tests in the plateau-core plateau — tests its production counterpart within the mirrored Allowed Dependencies
whenToUse: when adding a Gherkin scenario or unit test for a MediatR pipeline behavior, or checking that BuildingBlocks.Tests keeps to BuildingBlocks' own allowed references
domain: skill
type: template
plateau: core
version: 20261009220001
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `BuildingBlocks` its catalog-selected test project with its production dependency boundary mirrored.
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
| {Rule}Steps.cs | Bindings driving the real behavior | [[skills/dotnet/architecture/plateau/plateau-core/structure/BuildingBlocks.Tests/classes/plateau-core--class-building-blocks-rule-steps.skill\|class-building-blocks-rule-steps]] |

## What Does NOT Belong Here
- A module-specific concept — belongs to that module's `.Tests`.
- A reference to any module, `App.Host`, or infrastructure project.

## Allowed Dependencies
- Reference `BuildingBlocks` and mirror its production project's assembled [Allowed Dependencies](skills/dotnet/architecture/plateau/plateau-core/structure/BuildingBlocks/plateau-core--csproj-building-blocks.skill.md#allowed-dependencies); no wider project boundary.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create|BuildingBlocks.Tests.csproj]]

# Check list
- [ ] References match the mirrored production boundary and any explicitly applied architecture-test extension.
- [ ] Each scenario asserts the exact `Result` the real behavior returns.
