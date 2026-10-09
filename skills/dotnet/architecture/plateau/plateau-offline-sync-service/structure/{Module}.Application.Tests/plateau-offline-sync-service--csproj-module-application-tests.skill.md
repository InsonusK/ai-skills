---
name: plateau-offline-sync-service--csproj-module-application-tests
description: Project {Module}.Application.Tests in the plateau-offline-sync-service plateau — tests its production counterpart within the mirrored Allowed Dependencies
whenToUse: when adding a Gherkin scenario or unit test for a handler or validator, or checking {Module}.Application.Tests keeps to the same reference boundary as {Module}.Application
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220001
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Application` its catalog-selected test project with its production dependency boundary mirrored.
- Prove each handler's orchestration and each validator's rules against the real classes.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create|{Module}.Application.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are command-shaped: a command goes in, a `Result` comes out, asserted against the real handler.
- Validator scenarios run the real `AbstractValidator<T>` and assert the failing rule.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Application.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings driving the real handler / validator | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Application.Tests/classes/plateau-offline-sync-service--class-module-application-rule-steps.skill\|class-module-application-rule-steps]] |

## What Does NOT Belong Here
- Contract-shape assertions — belong to `{Module}.Interfaces.Tests`.
- A reference to another module's project, or to `App.Host` / infrastructure.

## Allowed Dependencies
- Reference `{Module}.Application` and mirror its production project's assembled [Allowed Dependencies](skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md#allowed-dependencies); no wider project boundary.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.
- Handler scenarios prove command orchestration; validator scenarios prove the input boundary.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Check list
- [ ] References match the mirrored production boundary and any explicitly applied architecture-test extension.
- [ ] Every scenario calls the real handler/validator and asserts its actual result.
