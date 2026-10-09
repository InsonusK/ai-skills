---
name: plateau-core--csproj-module-interfaces-tests
description: Project {Module}.Interfaces.Tests in the plateau-core plateau — the dedicated test project for {Module}.Interfaces, referencing that module's Interfaces only
whenToUse: when adding a Gherkin scenario or unit test that pins a public contract's shape (a command's marker, a DTO's fields), or checking {Module}.Interfaces.Tests keeps to the Interfaces-only boundary
domain: skill
type: template
plateau: core
version: 20261009220000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Interfaces` a dedicated test project referencing that module's `Interfaces` only.
- Pin the shape of the module's public contracts — a command implements the right marker, a response DTO carries the expected fields as `Soft{ValueObject}`/primitives.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create|{Module}.Interfaces.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are contract-shaped: construct the declared type and assert it is assignable to the right marker / carries the right members.
- No handler, no validator, no domain type is referenced — only the module's `Interfaces`.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Interfaces.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings constructing and inspecting the real contract types | [[skills/dotnet/architecture/plateau/plateau-core/structure/{Module}.Interfaces.Tests/classes/plateau-core--class-module-interfaces-rule-steps.skill\|class-module-interfaces-rule-steps]] |

## What Does NOT Belong Here
- Handler/validator behavior — belongs to `{Module}.Application.Tests`.
- A reference to `{Module}.Application`, `{Module}.Domain`, or another module.

## Allowed Dependencies
- `{Module}.Interfaces` (and transitively `Shared`) — nothing else.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Reference `{Module}.Interfaces` only.
- Never reach into `{Module}.Application` or `{Module}.Domain` for a shortcut.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create|{Module}.Interfaces.Tests.csproj]]

# Check list
- [ ] `{Module}.Interfaces.Tests.csproj` references only `{Module}.Interfaces`.
- [ ] Every scenario constructs and inspects a real contract type.
