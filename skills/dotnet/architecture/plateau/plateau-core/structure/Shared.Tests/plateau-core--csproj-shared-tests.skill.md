---
name: plateau-core--csproj-shared-tests
description: Project Shared.Tests in the plateau-core plateau — tests its production counterpart within the mirrored Allowed Dependencies
whenToUse: when adding a Gherkin scenario or unit test for a Shared primitive/marker, or checking that Shared.Tests keeps to Shared's own zero-project-reference boundary
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
- Give `Shared` its catalog-selected test project with its production dependency boundary mirrored.
- Prove `Shared`'s primitives/markers with value-shaped Gherkin scenarios.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are value-shaped: given one or more primitive values, prove how they compare/combine — never "is this input valid" (a module concern).

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/Shared.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings asserting against the real `Shared` type | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared.Tests/classes/plateau-core--class-shared-rule-steps.skill\|class-shared-rule-steps]] |

## What Does NOT Belong Here
- Any module-specific concept — a module's scenarios go in that module's own `.Tests` project.
- A project reference outside the production Allowed Dependencies and any explicitly applied architecture-test extension.

## Allowed Dependencies
- Reference `Shared` and mirror its production project's assembled [Allowed Dependencies](skills/dotnet/architecture/plateau/plateau-core/structure/Shared/plateau-core--csproj-shared.skill.md#allowed-dependencies); no wider project boundary.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.
- Never introduce a module-specific concept into a `Shared.Tests` scenario.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]]

# Check list
- [ ] References match the mirrored production boundary and any explicitly applied architecture-test extension.
